import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Chip,
  CircularProgress,
  Divider,
  FormControlLabel,
  Grid,
  Stack,
  Typography,
} from '@mui/material'
import { getDocuments, type DocumentResponse } from '../services/documentApi'
import { getProcessingJobs, retryProcessingJob, startProcessing, type ProcessingJobResponse, type ProcessingJobStatus } from '../services/processingJobApi'

function statusLabel(status: ProcessingJobStatus) {
  if (typeof status === 'number') return ['Pending', 'Processing', 'Completed', 'Failed'][status] ?? 'Unknown'
  return status
}

function statusColor(status: ProcessingJobStatus): 'default' | 'primary' | 'success' | 'error' {
  switch (statusLabel(status)) {
    case 'Processing': return 'primary'
    case 'Completed': return 'success'
    case 'Failed': return 'error'
    default: return 'default'
  }
}

function isActive(status: ProcessingJobStatus) {
  return statusLabel(status) === 'Pending' || statusLabel(status) === 'Processing'
}

function formatDate(date: string | null) {
  if (!date) return '—'
  const parsed = new Date(date)
  return Number.isNaN(parsed.getTime()) ? '—' : parsed.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
}

export function Processing() {
  const navigate = useNavigate()
  const [documents, setDocuments] = useState<DocumentResponse[]>([])
  const [jobs, setJobs] = useState<ProcessingJobResponse[]>([])
  const [selectedDocuments, setSelectedDocuments] = useState<string[]>([])
  const [loading, setLoading] = useState(true)
  const [starting, setStarting] = useState(false)
  const [retryingId, setRetryingId] = useState<string | null>(null)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const requestInFlight = useRef(false)

  const loadJobs = async (showLoading = false) => {
    if (requestInFlight.current) return
    requestInFlight.current = true
    if (showLoading) setLoading(true)
    try {
      setJobs(await getProcessingJobs())
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Processing jobs could not be loaded.')
    } finally {
      requestInFlight.current = false
      if (showLoading) setLoading(false)
    }
  }

  useEffect(() => {
    const loadPage = async () => {
      setLoading(true)
      try {
        const [loadedDocuments, loadedJobs] = await Promise.all([getDocuments(), getProcessingJobs()])
        setDocuments(loadedDocuments)
        setJobs(loadedJobs)
      } catch (requestError) {
        setError(requestError instanceof Error ? requestError.message : 'Processing data could not be loaded.')
      } finally {
        setLoading(false)
      }
    }
    void loadPage()
  }, [])

  useEffect(() => {
    if (!jobs.some((job) => isActive(job.status))) return undefined
    const interval = window.setInterval(() => { void loadJobs() }, 5000)
    return () => window.clearInterval(interval)
  }, [jobs])

  const toggleDocument = (documentId: string) => {
    setSelectedDocuments((current) => current.includes(documentId) ? current.filter((id) => id !== documentId) : [...current, documentId])
  }

  const handleStart = async () => {
    if (starting || selectedDocuments.length === 0) return
    setStarting(true)
    setError('')
    setSuccess('')
    try {
      const createdJobs = await startProcessing(selectedDocuments)
      setJobs((current) => [...createdJobs, ...current.filter((job) => !createdJobs.some((created) => created.id === job.id))])
      setSelectedDocuments([])
      setSuccess(`Processing started for ${createdJobs.length} document${createdJobs.length === 1 ? '' : 's'}.`)
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Processing could not be started.')
    } finally {
      setStarting(false)
    }
  }

  const handleRetry = async (job: ProcessingJobResponse) => {
    if (retryingId) return
    setRetryingId(job.id)
    setError('')
    setSuccess('')
    try {
      const retriedJob = await retryProcessingJob(job.id)
      setJobs((current) => current.map((currentJob) => currentJob.id === retriedJob.id ? retriedJob : currentJob))
      setSuccess('Processing job queued for retry.')
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Processing job could not be retried.')
    } finally {
      setRetryingId(null)
    }
  }

  const documentNames = new Map(documents.map((document) => [document.id, document.name]))
  const activeJobs = jobs.some((job) => isActive(job.status))

  return <Stack spacing={4}>
    <Box><Typography variant="h4" gutterBottom>Processing Jobs</Typography><Typography color="text.secondary">Select documents and monitor their processing progress.</Typography></Box>
    {error && <Alert severity="error">{error}</Alert>}
    {success && <Alert severity="success">{success}</Alert>}

    <Card><CardContent sx={{ p: { xs: 2.5, sm: 3 } }}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 2 }}><Box><Typography variant="h6">Start processing</Typography><Typography variant="body2" color="text.secondary">Choose one or more existing documents.</Typography></Box><Button variant="contained" onClick={() => void handleStart()} disabled={starting || selectedDocuments.length === 0}>{starting ? <CircularProgress size={22} color="inherit" /> : 'Start processing'}</Button></Stack>
      {documents.length === 0 ? <Typography color="text.secondary" sx={{ py: 2 }}>Upload a document before starting a processing job.</Typography> : <Stack divider={<Divider flexItem />}>
        {documents.map((document) => <FormControlLabel key={document.id} control={<Checkbox checked={selectedDocuments.includes(document.id)} onChange={() => toggleDocument(document.id)} disabled={starting} />} label={<Box><Typography variant="body2" sx={{ fontWeight: 600 }}>{document.name}</Typography><Typography variant="caption" color="text.secondary">{document.fileType || 'Unknown file type'}</Typography></Box>} sx={{ py: 0.75, m: 0 }} />)}
      </Stack>}
    </CardContent></Card>

    <Box><Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 2 }}><Box><Typography variant="h6">Your processing jobs</Typography>{activeJobs && <Typography variant="caption" color="text.secondary">Refreshing active jobs automatically</Typography>}</Box><Button onClick={() => void loadJobs(true)} disabled={loading || starting}>Refresh</Button></Stack>
      {loading ? <Box sx={{ display: 'grid', placeItems: 'center', py: 8 }}><CircularProgress /></Box> : jobs.length === 0 ? <Card><CardContent sx={{ py: 7, textAlign: 'center' }}><Typography variant="h6" gutterBottom>No processing jobs yet</Typography><Typography color="text.secondary">Jobs will appear here after you start processing a document.</Typography></CardContent></Card> : <Grid container spacing={2}>{jobs.map((job) => <Grid key={job.id} size={{ xs: 12, md: 6 }}><Card sx={{ height: '100%' }}><CardContent><Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', gap: 2 }}><Box sx={{ minWidth: 0 }}><Typography sx={{ fontWeight: 700 }} noWrap>{documentNames.get(job.documentId) ?? `Document ${job.documentId.slice(0, 8)}`}</Typography><Typography variant="caption" color="text.secondary">Created {formatDate(job.createdAt)}</Typography></Box><Chip size="small" label={statusLabel(job.status)} color={statusColor(job.status)} /></Stack><Divider sx={{ my: 2 }} /><Stack spacing={0.5}><Typography variant="body2" color="text.secondary">Started: {formatDate(job.startedAt)}</Typography><Typography variant="body2" color="text.secondary">Finished: {formatDate(job.finishedAt)}</Typography>{job.errorMessage && <Typography variant="body2" color="error.main">{job.errorMessage}</Typography>}</Stack><Stack direction="row" spacing={1} sx={{ mt: 2 }}>{statusLabel(job.status) === 'Completed' && <Button color="primary" size="small" variant="outlined" onClick={() => navigate(`/processing/${job.id}/results`)}>View results</Button>}{statusLabel(job.status) === 'Failed' && <Button color="primary" size="small" onClick={() => void handleRetry(job)} disabled={retryingId === job.id}>{retryingId === job.id ? <CircularProgress size={18} color="inherit" /> : 'Retry job'}</Button>}</Stack></CardContent></Card></Grid>)}</Grid>}
    </Box>
  </Stack>
}
