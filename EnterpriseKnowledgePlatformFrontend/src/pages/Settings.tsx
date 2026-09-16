import { useEffect, useState, type FormEvent } from 'react'
import {
  Alert, Box, Button, Card, CardContent, Chip, CircularProgress, Dialog,
  DialogActions, DialogContent, DialogTitle, Divider, Stack,
  TextField, Typography,
} from '@mui/material'
import { createApiKey, deleteApiKey, getApiKeys, updateApiKey, type ApiKeyResponse } from '../services/apiKeyApi'

function formatDate(value: string | null) {
  if (!value) return 'Never'
  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? '—' : parsed.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}

export function Settings() {
  const [keys, setKeys] = useState<ApiKeyResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [workingId, setWorkingId] = useState<string | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [label, setLabel] = useState('')
  const [expiresAt, setExpiresAt] = useState('')
  const [newKey, setNewKey] = useState('')
  const [copied, setCopied] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const loadKeys = async () => {
    setLoading(true); setError('')
    try { setKeys(await getApiKeys()) }
    catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'API keys could not be loaded.') }
    finally { setLoading(false) }
  }

  useEffect(() => {
    const timer = window.setTimeout(() => { void loadKeys() }, 0)
    return () => window.clearTimeout(timer)
  }, [])

  const closeDialog = () => { setDialogOpen(false); setLabel(''); setExpiresAt(''); setNewKey(''); setCopied(false) }
  const handleCreate = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (saving || !label.trim()) return
    setSaving(true); setError(''); setSuccess('')
    try {
      const response = await createApiKey(label.trim(), expiresAt ? new Date(`${expiresAt}T23:59:59`).toISOString() : null)
      const { apiKey, ...metadata } = response
      setKeys((current) => [metadata, ...current])
      setNewKey(apiKey)
      setSuccess('API key created. Copy it now; it will not be shown again after this dialog is closed.')
    } catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'API key could not be created.') }
    finally { setSaving(false) }
  }

  const handleCopy = async () => {
    if (!newKey) return
    try { await navigator.clipboard.writeText(newKey); setCopied(true) }
    catch { setError('Copy failed. Select and copy the key manually.') }
  }

  const handleDeactivate = async (key: ApiKeyResponse) => {
    if (workingId || !window.confirm(`Deactivate “${key.label}”?`)) return
    setWorkingId(key.id); setError(''); setSuccess('')
    try { const updated = await updateApiKey(key, false); setKeys((current) => current.map((item) => item.id === key.id ? updated : item)); setSuccess('API key deactivated.') }
    catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'API key could not be deactivated.') }
    finally { setWorkingId(null) }
  }

  const handleDelete = async (key: ApiKeyResponse) => {
    if (workingId || !window.confirm(`Permanently delete “${key.label}”?`)) return
    setWorkingId(key.id); setError(''); setSuccess('')
    try { await deleteApiKey(key.id); setKeys((current) => current.filter((item) => item.id !== key.id)); setSuccess('API key deleted.') }
    catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'API key could not be deleted.') }
    finally { setWorkingId(null) }
  }

  return <Stack spacing={3}>
    <Box><Typography variant="h4" gutterBottom>Settings</Typography><Typography color="text.secondary">Manage API access for your Enterprise Knowledge Platform workspace.</Typography></Box>
    {error && <Alert severity="error">{error}</Alert>}
    {success && !dialogOpen && <Alert severity="success">{success}</Alert>}
    <Card><CardContent sx={{ p: { xs: 2.5, sm: 3 } }}><Stack direction={{ xs: 'column', sm: 'row' }} sx={{ justifyContent: 'space-between', alignItems: { xs: 'flex-start', sm: 'center' }, gap: 2, mb: 2 }}><Box><Typography variant="h6">API keys</Typography><Typography variant="body2" color="text.secondary">Use keys to connect approved tools to the platform API.</Typography></Box><Button variant="contained" onClick={() => { setError(''); setSuccess(''); setDialogOpen(true) }}>Create API key</Button></Stack>{loading ? <Box sx={{ display: 'grid', placeItems: 'center', py: 8 }}><CircularProgress /></Box> : keys.length === 0 ? <Box sx={{ py: 6, textAlign: 'center' }}><Typography variant="h6" gutterBottom>No API keys yet</Typography><Typography color="text.secondary">Create a key when you are ready to connect an external tool.</Typography></Box> : <Stack divider={<Divider flexItem />} spacing={0}>{keys.map((key) => <Box key={key.id} sx={{ py: 2 }}><Stack direction={{ xs: 'column', md: 'row' }} sx={{ justifyContent: 'space-between', alignItems: { xs: 'flex-start', md: 'center' }, gap: 2 }}><Box><Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 0.5 }}><Typography sx={{ fontWeight: 700 }}>{key.label}</Typography><Chip size="small" label={key.isActive ? 'Active' : 'Inactive'} color={key.isActive ? 'success' : 'default'} /></Stack><Typography variant="body2" color="text.secondary">Created {formatDate(key.createdAt)} · Expires {formatDate(key.expiresAt)}</Typography></Box><Stack direction="row" spacing={1}>{key.isActive && <Button size="small" onClick={() => void handleDeactivate(key)} disabled={workingId === key.id}>{workingId === key.id ? <CircularProgress size={18} /> : 'Deactivate'}</Button>}<Button size="small" color="error" onClick={() => void handleDelete(key)} disabled={workingId === key.id}>Delete</Button></Stack></Stack></Box>)}</Stack>}</CardContent></Card>
    <Dialog open={dialogOpen} onClose={newKey ? undefined : closeDialog} fullWidth maxWidth="sm"><DialogTitle>{newKey ? 'API key created' : 'Create API key'}</DialogTitle><DialogContent>{newKey ? <Stack spacing={2} sx={{ pt: 1 }}><Alert severity="warning">Copy this key now. For security, the plaintext key will be cleared when this dialog closes.</Alert><TextField fullWidth label="New API key" value={newKey} slotProps={{ input: { readOnly: true } }} /><Button variant="outlined" onClick={() => void handleCopy()}>{copied ? 'Copied' : 'Copy to clipboard'}</Button></Stack> : <Box component="form" id="create-api-key-form" onSubmit={handleCreate} sx={{ display: 'grid', gap: 2, pt: 1 }}><TextField required fullWidth label="Label" value={label} onChange={(event) => setLabel(event.target.value)} disabled={saving} helperText="A name that helps you identify this key." /><TextField fullWidth type="date" label="Expiration date" value={expiresAt} onChange={(event) => setExpiresAt(event.target.value)} disabled={saving} slotProps={{ inputLabel: { shrink: true } }} /></Box>}</DialogContent><DialogActions><Button onClick={closeDialog}>{newKey ? 'Done' : 'Cancel'}</Button>{!newKey && <Button type="submit" form="create-api-key-form" variant="contained" disabled={saving || !label.trim()}>{saving ? <CircularProgress size={22} color="inherit" /> : 'Create key'}</Button>}</DialogActions></Dialog>
  </Stack>
}
