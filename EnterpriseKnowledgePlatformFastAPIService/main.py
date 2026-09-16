import asyncio
import logging
import os
from pathlib import Path

import httpx
from fastapi import FastAPI, File, Form, HTTPException, UploadFile

app = FastAPI(title="Mock document processing service")
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

DOTNET_API_BASE_URL = os.getenv("DOTNET_API_BASE_URL", "https://localhost:5001").rstrip("/")
PROCESSING_RESULT_PATH = os.getenv("PROCESSING_RESULT_PATH", "/api/ProcessingJob/{processing_job_id}/result")
MOCK_DELAY_SECONDS = float(os.getenv("MOCK_DELAY_SECONDS", "2"))
DOTNET_API_KEY = os.getenv("DOTNET_API_KEY", "")


async def send_processing_result(processing_job_id: str, payload: dict) -> None:
    if not DOTNET_API_KEY:
        raise RuntimeError("DOTNET_API_KEY is required for callback authentication")
    callback_path = PROCESSING_RESULT_PATH.replace("{processing_job_id}", processing_job_id)
    callback_url = f"{DOTNET_API_BASE_URL}/{callback_path.lstrip('/')}"
    async with httpx.AsyncClient(verify=False, timeout=30) as client:
        response = await client.post(callback_url, json=payload,
                                     headers={"X-API-Key": DOTNET_API_KEY})
        response.raise_for_status()


@app.post("/process")
async def process_document(
    processing_job_id: str = Form(..., alias="processingJobId"),
    document_id: str = Form(..., alias="documentId"),
    file: UploadFile = File(...),
):
    if not processing_job_id.strip() or not document_id.strip() or not file.filename:
        raise HTTPException(status_code=400, detail="processingJobId, documentId and file are required")

    logger.info("Processing started: processingJobId=%s documentId=%s filename=%s",
                processing_job_id, document_id, Path(file.filename).name)
    await file.read()  # Verify upload arrival; never log the contents.
    await asyncio.sleep(MOCK_DELAY_SECONDS)

    result = {
        "semanticBlocks": [
            {"text": "Machine learning is a field of artificial intelligence.", "blockIndex": 0, "page": 1},
            {"text": "Artificial intelligence processes data.", "blockIndex": 1, "page": 1},
        ],
        "triples": [
            {"subject": "Machine Learning", "predicate": "is a field of", "object": "Artificial Intelligence",
             "confidence": 0.99, "status": 1, "semanticBlockIndex": 0},
            {"subject": "Artificial Intelligence", "predicate": "processes", "object": "data",
             "confidence": 0.98, "status": 1, "semanticBlockIndex": 1},
        ],
    }
    try:
        await send_processing_result(processing_job_id, result)
    except (httpx.HTTPError, RuntimeError) as exc:
        logger.error("Callback failed for processingJobId=%s: %s", processing_job_id, type(exc).__name__)
        raise HTTPException(status_code=502, detail=".NET processing-result callback failed") from exc

    logger.info("Processing completed and callback succeeded: processingJobId=%s documentId=%s",
                processing_job_id, document_id)
    return {"status": "completed", "processingJobId": processing_job_id, "documentId": document_id}
