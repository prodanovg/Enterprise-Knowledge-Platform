import { useEffect, useMemo, useState } from 'react'
import { Alert, Box, Button, Card, CardContent, Chip, CircularProgress, Divider, Stack, Typography } from '@mui/material'
import { getNotifications, getReadNotificationIds, markNotificationRead, type NotificationResponse } from '../services/notificationApi'

function formatDate(value: string | null) {
  const parsed = new Date(value ?? '')
  return Number.isNaN(parsed.getTime()) ? '—' : parsed.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
}

export function Notifications() {
  const [notifications, setNotifications] = useState<NotificationResponse[]>([])
  const [readIds, setReadIds] = useState<string[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const loadNotifications = async () => {
    setLoading(true); setError('')
    try { setNotifications(await getNotifications()); setReadIds(getReadNotificationIds()) }
    catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'Notifications could not be loaded.') }
    finally { setLoading(false) }
  }

  useEffect(() => {
    const timer = window.setTimeout(() => { void loadNotifications() }, 0)
    return () => window.clearTimeout(timer)
  }, [])
  const unreadCount = useMemo(() => notifications.filter((notification) => !readIds.includes(notification.id)).length, [notifications, readIds])
  const handleMarkRead = (id: string) => { markNotificationRead(id); setReadIds(getReadNotificationIds()) }

  return <Stack spacing={3}>
    <Box><Stack direction={{ xs: 'column', sm: 'row' }} sx={{ justifyContent: 'space-between', alignItems: { xs: 'flex-start', sm: 'center' }, gap: 2 }}><Box><Typography variant="h4" gutterBottom>Notifications</Typography><Typography color="text.secondary">Stay up to date with activity in your knowledge workspace.</Typography></Box><Button onClick={() => void loadNotifications()} disabled={loading}>Refresh</Button></Stack></Box>
    {error && <Alert severity="error">{error}</Alert>}
    {loading ? <Box sx={{ display: 'grid', placeItems: 'center', py: 10 }}><CircularProgress /></Box> : notifications.length === 0 ? <Card><CardContent sx={{ py: 8, textAlign: 'center' }}><Typography variant="h6" gutterBottom>No notifications</Typography><Typography color="text.secondary">You’re all caught up. New activity will appear here.</Typography></CardContent></Card> : <Stack spacing={1.5}><Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}><Typography variant="h6">Recent activity</Typography><Chip size="small" color={unreadCount ? 'primary' : 'default'} label={`${unreadCount} unread`} /></Stack>{notifications.map((notification) => { const isRead = readIds.includes(notification.id); return <Card key={notification.id} sx={{ bgcolor: isRead ? 'background.paper' : '#f0f4ff', borderLeft: 3, borderColor: isRead ? 'divider' : 'primary.main' }}><CardContent sx={{ py: 2.25, '&:last-child': { pb: 2.25 } }}><Stack direction={{ xs: 'column', sm: 'row' }} sx={{ justifyContent: 'space-between', alignItems: { xs: 'flex-start', sm: 'center' }, gap: 2 }}><Box sx={{ minWidth: 0, flexGrow: 1 }}><Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 0.75 }}><Typography sx={{ fontWeight: isRead ? 600 : 750, overflowWrap: 'anywhere' }}>{notification.title || 'Notification'}</Typography>{!isRead && <Chip size="small" color="primary" label="Unread" />}</Stack><Typography sx={{ whiteSpace: 'pre-wrap', overflowWrap: 'anywhere' }}>{notification.message}</Typography><Divider sx={{ my: 1.5 }} /><Typography variant="caption" color="text.secondary">{notification.sentAt ? `Sent ${formatDate(notification.sentAt)}` : `Created ${formatDate(notification.createdAt)}`}</Typography></Box>{!isRead && <Button size="small" onClick={() => handleMarkRead(notification.id)}>Mark as read</Button>}</Stack></CardContent></Card>})}</Stack>}
  </Stack>
}
