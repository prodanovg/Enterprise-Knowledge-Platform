import asyncio
import unittest
from unittest.mock import AsyncMock, patch

import httpx
import main


class FakeResponse:
    def raise_for_status(self):
        return None


class CallbackTests(unittest.TestCase):
    def test_callback_sends_configured_api_key_and_payload(self):
        client = AsyncMock()
        client.post.return_value = FakeResponse()
        context = client.__aenter__.return_value
        context.post.return_value = FakeResponse()
        with patch.object(main, "DOTNET_API_KEY", "test-key"), \
             patch.object(main.httpx, "AsyncClient", return_value=client):
            asyncio.run(main.send_processing_result("job-1", {"triples": []}))
        context.post.assert_awaited_once_with(
            "http://localhost:5182/api/ProcessingJob/job-1/result",
            json={"triples": []}, headers={"X-API-Key": "test-key"})

    def test_missing_api_key_fails_without_sending(self):
        with patch.object(main, "DOTNET_API_KEY", ""):
            with self.assertRaises(RuntimeError):
                asyncio.run(main.send_processing_result("job-1", {}))

    def test_process_accepts_multipart_and_sends_mock_result(self):
        callback = AsyncMock()

        async def run_test():
            with patch.object(main, "DOTNET_API_KEY", "test-key"), \
                 patch.object(main, "MOCK_DELAY_SECONDS", 0), \
                 patch.object(main, "send_processing_result", callback):
                transport = httpx.ASGITransport(app=main.app)
                async with httpx.AsyncClient(transport=transport, base_url="http://test") as client:
                    response = await client.post(
                        "/process",
                        data={"processingJobId": "job-1", "documentId": "doc-1"},
                        files={"file": ("sample.txt", b"document contents", "text/plain")},
                    )
                    return response

        response = asyncio.run(run_test())
        self.assertEqual(response.status_code, 200)
        callback.assert_awaited_once()
        args, kwargs = callback.await_args
        self.assertEqual(args[0], "job-1")
        self.assertEqual(len(args[1]["semanticBlocks"]), 2)
        self.assertEqual(len(args[1]["triples"]), 2)

    def test_process_requires_multipart_fields_and_file(self):
        async def run_test():
            transport = httpx.ASGITransport(app=main.app)
            async with httpx.AsyncClient(transport=transport, base_url="http://test") as client:
                return await client.post(
                    "/process",
                    data={"processingJobId": "job-1", "documentId": "doc-1"},
                )

        response = asyncio.run(run_test())
        self.assertEqual(response.status_code, 422)

    def test_callback_failure_returns_processing_failure_without_exposing_key(self):
        callback = AsyncMock(side_effect=httpx.ConnectError("callback unavailable"))

        async def run_test():
            with patch.object(main, "DOTNET_API_KEY", "secret-key"), \
                 patch.object(main, "MOCK_DELAY_SECONDS", 0), \
                 patch.object(main, "send_processing_result", callback):
                transport = httpx.ASGITransport(app=main.app)
                async with httpx.AsyncClient(transport=transport, base_url="http://test") as client:
                    return await client.post(
                        "/process",
                        data={"processingJobId": "job-1", "documentId": "doc-1"},
                        files={"file": ("sample.txt", b"document contents", "text/plain")},
                    )

        response = asyncio.run(run_test())
        self.assertEqual(response.status_code, 502)
        self.assertNotIn("secret-key", response.text)


if __name__ == "__main__":
    unittest.main()
