import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Divider,
  Grid,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { deleteDocument, getDocuments, uploadDocument, type DocumentResponse } from '../services/documentApi'

function formatStatus(status: DocumentResponse['status']) {
  if (typeof status === 'number') return ['Pending', 'Processing', 'Processed', 'Failed'][status] ?? 'Unknown'
  return status
}

function formatDate(date: string) {
  const parsed = new Date(date)
  return Number.isNaN(parsed.getTime()) ? '—' : parsed.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}

function statusColor(status: DocumentResponse['status']) {
  return formatStatus(status) === 'Failed' ? 'error.main' : formatStatus(status) === 'Processed' ? 'success.main' : 'primary.main'
}

export function Documents() {
  const [documents, setDocuments] = useState<DocumentResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [uploading, setUploading] = useState(false)
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [file, setFile] = useState<File | null>(null)
  const [name, setName] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const loadDocuments = async () => {
    setLoading(true)
    setError('')
    try {
      setDocuments(await getDocuments())
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Documents could not be loaded.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    const timer = window.setTimeout(() => { void loadDocuments() }, 0)
    return () => window.clearTimeout(timer)
  }, [])

  const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    const selectedFile = event.target.files?.[0] ?? null
    setFile(selectedFile)
    if (selectedFile && !name) setName(selectedFile.name)
    setError('')
    setSuccess('')
  }

  const handleUpload = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (uploading || !file) {
      if (!file) setError('Choose a file before uploading.')
      return
    }
    if (!name.trim()) {
      setError('Enter a document name before uploading.')
      return
    }

    setUploading(true)
    setError('')
    setSuccess('')
    try {
      await uploadDocument(name.trim(), file)
      setFile(null)
      setName('')
      setSuccess('Document uploaded successfully.')
      await loadDocuments()
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Document upload failed.')
    } finally {
      setUploading(false)
    }
  }

  const handleDelete = async (document: DocumentResponse) => {
    if (deletingId || !window.confirm(`Delete “${document.name}”?`)) return
    setDeletingId(document.id)
    setError('')
    setSuccess('')
    try {
      await deleteDocument(document.id)
      setDocuments((current) => current.filter((item) => item.id !== document.id))
      setSuccess('Document deleted successfully.')
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Document could not be deleted.')
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <Stack spacing={4}>
      <Box>
        <Typography variant="h4" gutterBottom>Documents</Typography>
        <Typography color="text.secondary">Upload and manage the documents in your knowledge workspace.</Typography>
      </Box>

      {error && <Alert severity="error">{error}</Alert>}
      {success && <Alert severity="success">{success}</Alert>}

      <Card>
        <CardContent sx={{ p: { xs: 2.5, sm: 3 } }}>
          <Typography variant="h6" gutterBottom>Upload a document</Typography>
          <Box component="form" onSubmit={handleUpload} sx={{ display: 'flex', gap: 2, alignItems: 'flex-start', flexWrap: 'wrap' }}>
            <TextField label="Document name" value={name} onChange={(event) => setName(event.target.value)} disabled={uploading} sx={{ flex: '1 1 220px' }} />
            <Button component="label" variant="outlined" disabled={uploading} sx={{ height: 56 }}>
              {file ? 'Change file' : 'Choose file'}
              <input hidden type="file" onChange={handleFileChange} />
            </Button>
            <Button type="submit" variant="contained" disabled={uploading || !file} sx={{ height: 56, minWidth: 130 }}>
              {uploading ? <CircularProgress size={22} color="inherit" /> : 'Upload'}
            </Button>
          </Box>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5 }}>{file ? `Selected: ${file.name}` : 'Select a file to begin.'}</Typography>
        </CardContent>
      </Card>

      <Box>
        <Stack direction="row" sx={{ mb: 2, justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography variant="h6">Your documents</Typography>
          <Button onClick={() => void loadDocuments()} disabled={loading || uploading}>Refresh</Button>
        </Stack>
        {loading ? <Box sx={{ display: 'grid', placeItems: 'center', py: 8 }}><CircularProgress /></Box> : documents.length === 0 ? <Card><CardContent sx={{ py: 7, textAlign: 'center' }}><Typography variant="h6" gutterBottom>No documents yet</Typography><Typography color="text.secondary">Upload your first document to start building your knowledge workspace.</Typography></CardContent></Card> : <Grid container spacing={2}>
          {documents.map((document) => <Grid key={document.id} size={{ xs: 12, md: 6 }}><Card sx={{ height: '100%' }}><CardContent><Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', gap: 2 }}><Box sx={{ minWidth: 0 }}><Typography sx={{ fontWeight: 700 }} noWrap title={document.name}>{document.name}</Typography><Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>{document.fileType || 'Unknown file type'}</Typography></Box><Typography variant="body2" sx={{ color: statusColor(document.status), fontWeight: 700, whiteSpace: 'nowrap' }}>{formatStatus(document.status)}</Typography></Stack><Divider sx={{ my: 2 }} /><Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}><Typography variant="caption" color="text.secondary">Added {formatDate(document.createdAt)}</Typography><Button color="error" size="small" onClick={() => void handleDelete(document)} disabled={deletingId === document.id}>{deletingId === document.id ? <CircularProgress size={18} color="inherit" /> : 'Delete'}</Button></Stack></CardContent></Card></Grid>)}
        </Grid>}
      </Box>
    </Stack>
  )
}
