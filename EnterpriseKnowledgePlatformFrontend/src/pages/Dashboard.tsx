import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Alert, Box, Button, Card, CardContent, Chip, CircularProgress, Divider, Grid, Stack, Typography } from '@mui/material'
import { getDocuments, type DocumentResponse } from '../services/documentApi'
import { getNotifications, type NotificationResponse } from '../services/notificationApi'
import { getProcessingJobs, type ProcessingJobResponse } from '../services/processingJobApi'
import { getAuthIdentity } from '../services/authStorage'

function statusLabel(status: string | number, values: string[]) { return typeof status === 'number' ? values[status] ?? 'Unknown' : status }
function statusColor(status: string): 'default' | 'primary' | 'success' | 'error' | 'warning' { if (status === 'Completed' || status === 'Processed') return 'success'; if (status === 'Failed') return 'error'; if (status === 'Processing') return 'primary'; if (status === 'Pending') return 'warning'; return 'default' }
function formatDate(value: string | null) { const date = new Date(value ?? ''); return Number.isNaN(date.getTime()) ? '—' : date.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' }) }

function SectionError({ message }: { message?: string }) { return message ? <Alert severity="error" sx={{ mb: 2 }}>{message}</Alert> : null }

export function Dashboard() {
  const navigate = useNavigate()
  const displayIdentity = getAuthIdentity() ?? 'there'
  const [documents, setDocuments] = useState<DocumentResponse[]>([])
  const [jobs, setJobs] = useState<ProcessingJobResponse[]>([])
  const [notifications, setNotifications] = useState<NotificationResponse[]>([])
  const [errors, setErrors] = useState<string[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const loadOverview = async () => {
      const [documentResult, jobResult, notificationResult] = await Promise.allSettled([getDocuments(), getProcessingJobs(), getNotifications()])
      const nextErrors: string[] = []
      if (documentResult.status === 'fulfilled') setDocuments(documentResult.value); else nextErrors.push('Documents are currently unavailable.')
      if (jobResult.status === 'fulfilled') setJobs(jobResult.value); else nextErrors.push('Processing jobs are currently unavailable.')
      if (notificationResult.status === 'fulfilled') setNotifications(notificationResult.value); else nextErrors.push('Notifications are currently unavailable.')
      setErrors(nextErrors)
      setLoading(false)
    }
    void loadOverview()
  }, [])

  const jobCounts = useMemo(() => ['Pending', 'Processing', 'Completed', 'Failed'].map((status) => ({ status, count: jobs.filter((job) => statusLabel(job.status, ['Pending', 'Processing', 'Completed', 'Failed']) === status).length })), [jobs])
  const recentDocuments = useMemo(() => [...documents].sort((a, b) => b.createdAt.localeCompare(a.createdAt)).slice(0, 5), [documents])
  const recentJobs = useMemo(() => [...jobs].sort((a, b) => b.createdAt.localeCompare(a.createdAt)).slice(0, 5), [jobs])
  const recentNotifications = notifications.slice(0, 4)
  const documentNames = useMemo(() => new Map(documents.map((document) => [document.id, document.name])), [documents])

  if (loading) return <Box sx={{ display: 'grid', placeItems: 'center', py: 12 }}><CircularProgress /></Box>

  return <Stack spacing={3.5}>
    <Box><Typography variant="h4" gutterBottom>Welcome back, {displayIdentity}</Typography><Typography color="text.secondary">Here’s an overview of your knowledge workspace.</Typography></Box>
    {errors.length > 0 && <Alert severity="warning">Some dashboard sections could not be loaded: {errors.join(' ')}</Alert>}

    <Grid container spacing={2.5}>
      <Grid size={{ xs: 12, sm: 4 }}><Card sx={{ height: '100%' }}><CardContent><Typography variant="body2" color="text.secondary">Total documents</Typography><Typography variant="h3" sx={{ mt: 1, fontWeight: 700 }}>{documents.length}</Typography><Button size="small" onClick={() => navigate('/documents')} sx={{ mt: 1, pl: 0 }}>View documents →</Button></CardContent></Card></Grid>
      <Grid size={{ xs: 12, sm: 8 }}><Card sx={{ height: '100%' }}><CardContent><Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 2 }}><Box><Typography variant="body2" color="text.secondary">Processing jobs</Typography><Typography variant="h6" sx={{ mt: 0.5 }}>Status overview</Typography></Box><Button size="small" onClick={() => navigate('/processing')}>Open jobs</Button></Stack><Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>{jobCounts.map((item) => <Chip key={item.status} size="small" label={`${item.status}: ${item.count}`} color={statusColor(item.status)} variant={item.count ? 'filled' : 'outlined'} />)}</Stack></CardContent></Card></Grid>
    </Grid>

    <Grid container spacing={2.5}>
      <Grid size={{ xs: 12, lg: 7 }}><Card sx={{ height: '100%' }}><CardContent><Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 1 }}><Typography variant="h6">Recent documents</Typography><Button size="small" onClick={() => navigate('/documents')}>View all</Button></Stack><SectionError message={errors.some((error) => error.startsWith('Documents')) ? 'Documents could not be loaded.' : undefined} />{recentDocuments.length === 0 ? <Typography color="text.secondary" sx={{ py: 3 }}>No documents have been uploaded yet.</Typography> : <Stack divider={<Divider flexItem />} spacing={0}>{recentDocuments.map((document) => <Stack key={document.id} direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', gap: 2, py: 1.5 }}><Box sx={{ minWidth: 0 }}><Typography sx={{ fontWeight: 600 }} noWrap>{document.name}</Typography><Typography variant="caption" color="text.secondary">{document.fileType || 'Unknown type'} · Added {formatDate(document.createdAt)}</Typography></Box><Chip size="small" label={statusLabel(document.status, ['Pending', 'Processing', 'Processed', 'Failed'])} color={statusColor(statusLabel(document.status, ['Pending', 'Processing', 'Processed', 'Failed']))} /></Stack>)}</Stack>}</CardContent></Card></Grid>
      <Grid size={{ xs: 12, lg: 5 }}><Card sx={{ height: '100%' }}><CardContent><Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 1 }}><Typography variant="h6">Recent notifications</Typography><Button size="small" onClick={() => navigate('/notifications')}>View all</Button></Stack><SectionError message={errors.some((error) => error.startsWith('Notifications')) ? 'Notifications could not be loaded.' : undefined} />{recentNotifications.length === 0 ? <Typography color="text.secondary" sx={{ py: 3 }}>No recent notifications.</Typography> : <Stack divider={<Divider flexItem />} spacing={0}>{recentNotifications.map((notification) => <Box key={notification.id} sx={{ py: 1.5 }}><Typography sx={{ fontWeight: 700 }} noWrap>{notification.title || 'Notification'}</Typography><Typography variant="body2" color="text.secondary" sx={{ mt: 0.5, overflowWrap: 'anywhere' }}>{notification.message}</Typography><Typography variant="caption" color="text.secondary">{formatDate(notification.sentAt ?? notification.createdAt)}</Typography></Box>)}</Stack>}</CardContent></Card></Grid>
    </Grid>

    <Card><CardContent><Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 1 }}><Typography variant="h6">Recent processing jobs</Typography><Button size="small" onClick={() => navigate('/processing')}>View all</Button></Stack><SectionError message={errors.some((error) => error.startsWith('Processing')) ? 'Processing jobs could not be loaded.' : undefined} />{recentJobs.length === 0 ? <Typography color="text.secondary" sx={{ py: 3 }}>No processing jobs have been started yet.</Typography> : <Stack divider={<Divider flexItem />} spacing={0}>{recentJobs.map((job) => <Stack key={job.id} direction={{ xs: 'column', sm: 'row' }} sx={{ justifyContent: 'space-between', alignItems: { xs: 'flex-start', sm: 'center' }, gap: 1, py: 1.5 }}><Box><Typography sx={{ fontWeight: 600 }}>{documentNames.get(job.documentId) ?? `Document ${job.documentId.slice(0, 8)}`}</Typography><Typography variant="caption" color="text.secondary">Created {formatDate(job.createdAt)}</Typography></Box><Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}><Chip size="small" label={statusLabel(job.status, ['Pending', 'Processing', 'Completed', 'Failed'])} color={statusColor(statusLabel(job.status, ['Pending', 'Processing', 'Completed', 'Failed']))} />{statusLabel(job.status, ['Pending', 'Processing', 'Completed', 'Failed']) === 'Completed' && <Button size="small" onClick={() => navigate(`/processing/${job.id}/results`)}>Results</Button>}</Stack></Stack>)}</Stack>}</CardContent></Card>

    <Card sx={{ bgcolor: 'primary.main', color: 'primary.contrastText' }}><CardContent sx={{ p: { xs: 2.5, sm: 3 } }}><Stack direction={{ xs: 'column', sm: 'row' }} sx={{ justifyContent: 'space-between', alignItems: { xs: 'flex-start', sm: 'center' }, gap: 2 }}><Box><Typography variant="h6">Continue exploring your workspace</Typography><Typography variant="body2" sx={{ opacity: 0.85, mt: 0.5 }}>Manage content, monitor processing, or explore connected knowledge.</Typography></Box><Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}><Button variant="contained" color="inherit" onClick={() => navigate('/documents')} sx={{ color: 'primary.main' }}>Documents</Button><Button variant="outlined" color="inherit" onClick={() => navigate('/processing')}>Processing</Button><Button variant="outlined" color="inherit" onClick={() => navigate('/knowledge-graph')}>Knowledge graph</Button></Stack></Stack></CardContent></Card>
  </Stack>
}
