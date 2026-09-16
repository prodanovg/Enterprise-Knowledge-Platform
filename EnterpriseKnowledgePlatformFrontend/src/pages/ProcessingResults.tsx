import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Divider,
  Stack,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tabs,
  Typography,
} from '@mui/material'
import type { DocumentResponse } from '../services/documentApi'
import type { ProcessingJobResponse } from '../services/processingJobApi'
import {
  getDocument,
  getProcessingJob,
  getSemanticBlocks,
  getTripleProvenance,
  getTriples,
  type SemanticBlockResponse,
  type TripleProvenanceResponse,
  type TripleResponse,
} from '../services/processingResultsApi'

function enumLabel(value: string | number, values: string[]) {
  return typeof value === 'number' ? values[value] ?? 'Unknown' : value
}

function formatDate(value: string | null | undefined) {
  if (!value) return '—'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : date.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
}

function confidenceLabel(value: number) {
  return `${value <= 1 ? Math.round(value * 100) : Math.round(value)}%`
}

function statusColor(status: string): 'default' | 'success' | 'error' | 'warning' {
  if (status === 'Validated') return 'success'
  if (status === 'Rejected') return 'error'
  if (status === 'Pending') return 'warning'
  return 'default'
}

export function ProcessingResults() {
  const { jobId } = useParams<{ jobId: string }>()
  const navigate = useNavigate()
  const [job, setJob] = useState<ProcessingJobResponse | null>(null)
  const [document, setDocument] = useState<DocumentResponse | null>(null)
  const [blocks, setBlocks] = useState<SemanticBlockResponse[]>([])
  const [triples, setTriples] = useState<TripleResponse[]>([])
  const [provenance, setProvenance] = useState<TripleProvenanceResponse[]>([])
  const [tab, setTab] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!jobId) {
      return
    }

    const loadResults = async () => {
      setLoading(true)
      setError('')
      try {
        const [loadedJob, loadedBlocks, loadedTriples, loadedProvenance] = await Promise.all([
          getProcessingJob(jobId),
          getSemanticBlocks(),
          getTriples(),
          getTripleProvenance(),
        ])
        const loadedDocument = await getDocument(loadedJob.documentId)
        const documentBlocks = loadedBlocks.filter((block) => block.documentId === loadedJob.documentId)
        const documentProvenance = loadedProvenance.filter((item) => item.documentId === loadedJob.documentId)
        const tripleIds = new Set(documentProvenance.map((item) => item.tripleId))

        setJob(loadedJob)
        setDocument(loadedDocument)
        setBlocks(documentBlocks)
        setProvenance(documentProvenance)
        setTriples(loadedTriples.filter((triple) => tripleIds.has(triple.id)))
      } catch (requestError) {
        setError(requestError instanceof Error ? requestError.message : 'Processing results could not be loaded.')
      } finally {
        setLoading(false)
      }
    }

    void loadResults()
  }, [jobId])

  const blockById = useMemo(() => new Map(blocks.map((block) => [block.id, block])), [blocks])
  const provenanceByTriple = useMemo(() => new Map(provenance.map((item) => [item.tripleId, item])), [provenance])

  if (!jobId) return <Stack spacing={2}><Alert severity="error">No processing job was specified.</Alert><Button onClick={() => navigate('/processing')}>Back to processing jobs</Button></Stack>
  if (loading) return <Box sx={{ display: 'grid', placeItems: 'center', py: 12 }}><CircularProgress /></Box>
  if (error) return <Stack spacing={2}><Alert severity="error">{error}</Alert><Button onClick={() => navigate('/processing')}>Back to processing jobs</Button></Stack>
  if (!job) return null

  const jobStatus = enumLabel(job.status, ['Pending', 'Processing', 'Completed', 'Failed'])
  const sortedBlocks = [...blocks].sort((a, b) => a.blockIndex - b.blockIndex)

  return <Stack spacing={3}>
    <Box>
      <Button onClick={() => navigate('/processing')} sx={{ mb: 2, pl: 0 }}>← Back to processing jobs</Button>
      <Typography variant="h4" gutterBottom>Processing results</Typography>
      <Typography color="text.secondary">Inspect the extracted knowledge from this processing job.</Typography>
    </Box>

    <Card><CardContent>
      <Stack direction={{ xs: 'column', sm: 'row' }} sx={{ justifyContent: 'space-between', gap: 2 }}>
        <Box><Typography variant="h6">{document?.name ?? 'Document'}</Typography><Typography variant="body2" color="text.secondary">Job created {formatDate(job.createdAt)} · {document?.fileType || 'Unknown file type'}</Typography></Box>
        <Chip label={jobStatus} color={jobStatus === 'Completed' ? 'success' : 'default'} />
      </Stack>
      <Divider sx={{ my: 2 }} />
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={3}><Typography variant="body2" color="text.secondary">Started: {formatDate(job.startedAt)}</Typography><Typography variant="body2" color="text.secondary">Finished: {formatDate(job.finishedAt)}</Typography></Stack>
    </CardContent></Card>

    <Card>
      <Tabs value={tab} onChange={(_, value: number) => setTab(value)} variant="scrollable" scrollButtons="auto"><Tab label={`Semantic blocks (${blocks.length})`} /><Tab label={`Extracted triples (${triples.length})`} /></Tabs>
      <Divider />
      {tab === 0 && <CardContent>{sortedBlocks.length === 0 ? <Typography color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>No semantic blocks were returned for this job.</Typography> : <Stack spacing={2}>{sortedBlocks.map((block) => <Box key={block.id} sx={{ p: 2, border: 1, borderColor: 'divider', borderRadius: 2 }}><Stack direction="row" sx={{ justifyContent: 'space-between', gap: 2, mb: 1 }}><Typography sx={{ fontWeight: 700 }}>Block {block.blockIndex + 1}</Typography><Typography variant="caption" color="text.secondary">Page {block.page}</Typography></Stack><Typography sx={{ whiteSpace: 'pre-wrap', overflowWrap: 'anywhere', lineHeight: 1.7 }}>{block.text}</Typography></Box>)}</Stack>}</CardContent>}
      {tab === 1 && <CardContent>{triples.length === 0 ? <Typography color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>No extracted triples were returned for this job.</Typography> : <TableContainer><Table sx={{ minWidth: 680 }}><TableHead><TableRow><TableCell>Subject</TableCell><TableCell>Predicate</TableCell><TableCell>Object</TableCell><TableCell>Confidence</TableCell><TableCell>Status</TableCell><TableCell>Source</TableCell></TableRow></TableHead><TableBody>{triples.map((triple) => { const source = provenanceByTriple.get(triple.id); const block = source ? blockById.get(source.semanticBlockId) : undefined; const status = enumLabel(triple.status, ['Pending', 'Validated', 'Rejected']); return <TableRow key={triple.id} hover><TableCell sx={{ maxWidth: 180, verticalAlign: 'top', overflowWrap: 'anywhere' }}>{triple.subject}</TableCell><TableCell sx={{ maxWidth: 180, verticalAlign: 'top', overflowWrap: 'anywhere' }}>{triple.predicate}</TableCell><TableCell sx={{ maxWidth: 220, verticalAlign: 'top', overflowWrap: 'anywhere' }}>{triple.object}</TableCell><TableCell sx={{ verticalAlign: 'top', whiteSpace: 'nowrap' }}>{confidenceLabel(triple.confidence)}</TableCell><TableCell sx={{ verticalAlign: 'top' }}><Chip size="small" label={status} color={statusColor(status)} /></TableCell><TableCell sx={{ minWidth: 180, verticalAlign: 'top' }}>{block ? <><Typography variant="caption" sx={{ display: 'block' }} color="text.secondary">Page {block.page}, block {block.blockIndex + 1}</Typography><Typography variant="caption" sx={{ display: 'block', mt: 0.5, whiteSpace: 'normal', overflowWrap: 'anywhere' }}>{block.text}</Typography></> : '—'}</TableCell></TableRow>})}</TableBody></Table></TableContainer>}</CardContent>}
    </Card>
  </Stack>
}
