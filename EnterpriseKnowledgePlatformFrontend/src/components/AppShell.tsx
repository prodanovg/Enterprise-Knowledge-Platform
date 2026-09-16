import { useEffect, useState, type ReactNode } from 'react'
import { AppBar, Avatar, Badge, Box, Button, Divider, IconButton, List, ListItemButton, ListItemText, Toolbar, Tooltip, Typography } from '@mui/material'
import { useLocation, useNavigate } from 'react-router-dom'
import { clearAuthToken } from '../services/authStorage'
import { getNotifications, getReadNotificationIds, NOTIFICATIONS_CHANGED_EVENT } from '../services/notificationApi'

const navigationItems = [
  { label: 'Dashboard', path: '/' },
  { label: 'Documents', path: '/documents' },
  { label: 'Processing Jobs', path: '/processing' },
  { label: 'Knowledge Graph', path: '/knowledge-graph' },
  { label: 'Notifications', path: '/notifications' },
  { label: 'Settings', path: '/settings' },
]

function isActivePath(pathname: string, itemPath: string) {
  return itemPath === '/' ? pathname === '/' : pathname === itemPath || pathname.startsWith(`${itemPath}/`)
}

function Brand() {
  return <Box sx={{ px: 3, py: 3, display: 'flex', alignItems: 'center', gap: 1.5 }}><Box sx={{ width: 34, height: 34, borderRadius: 2, bgcolor: 'primary.main', display: 'grid', placeItems: 'center', color: 'white', fontWeight: 800 }}>E</Box><Box><Typography sx={{ fontWeight: 800, lineHeight: 1.1 }}>Enterprise</Typography><Typography variant="caption" color="text.secondary">Knowledge Platform</Typography></Box></Box>
}

function Navigation({ mobile = false }: { mobile?: boolean }) {
  const location = useLocation()
  const navigate = useNavigate()
  return <List sx={{ px: 1.5, py: mobile ? 0.5 : 1 }}>{navigationItems.map((item) => { const active = isActivePath(location.pathname, item.path); return <ListItemButton key={item.path} selected={active} onClick={() => navigate(item.path)} sx={{ borderRadius: 2, mb: 0.5, py: 1.15, '&.Mui-selected': { bgcolor: 'primary.main', color: 'primary.contrastText', '&:hover': { bgcolor: 'primary.dark' } }, '&:hover': { bgcolor: 'action.hover' } }}><Box sx={{ width: 8, height: 8, borderRadius: '50%', mr: 2, bgcolor: active ? 'inherit' : 'secondary.main', opacity: active ? 0.8 : 1 }} /><ListItemText primary={item.label} sx={{ fontSize: 14, fontWeight: active ? 700 : 500 }} /></ListItemButton> })}</List>
}

export function AppShell({ children }: { children: ReactNode }) {
  const navigate = useNavigate()
  const location = useLocation()
  const currentPage = navigationItems.find((item) => isActivePath(location.pathname, item.path))?.label ?? 'Page not found'
  const [unreadCount, setUnreadCount] = useState(0)

  useEffect(() => {
    const refreshUnreadCount = async () => {
      try {
        const notifications = await getNotifications()
        const readIds = new Set(getReadNotificationIds())
        setUnreadCount(notifications.filter((notification) => !readIds.has(notification.id)).length)
      } catch { setUnreadCount(0) }
    }
    const handleChanged = () => { void refreshUnreadCount() }
    void refreshUnreadCount()
    window.addEventListener(NOTIFICATIONS_CHANGED_EVENT, handleChanged)
    return () => window.removeEventListener(NOTIFICATIONS_CHANGED_EVENT, handleChanged)
  }, [])

  const handleLogout = () => { clearAuthToken(); navigate('/login', { replace: true }) }

  return <Box sx={{ display: 'flex', minHeight: '100vh' }}><Box component="aside" sx={{ width: 260, bgcolor: 'background.paper', borderRight: 1, borderColor: 'divider', display: { xs: 'none', md: 'block' }, flexShrink: 0 }}><Brand /><Divider /><Navigation /></Box><Box sx={{ flexGrow: 1, minWidth: 0 }}><AppBar position="sticky" color="inherit" elevation={0} sx={{ borderBottom: 1, borderColor: 'divider', bgcolor: 'rgba(255,255,255,0.9)', backdropFilter: 'blur(8px)' }}><Toolbar sx={{ justifyContent: 'space-between', minHeight: { xs: 64, sm: 72 } }}><Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}><Typography variant="h6" sx={{ display: { xs: 'block', md: 'none' } }}>EKP</Typography><Typography variant="h6" sx={{ display: { xs: 'none', sm: 'block' } }}>{currentPage}</Typography></Box><Box sx={{ display: 'flex', alignItems: 'center', gap: { xs: 0.5, sm: 1.5 } }}><Tooltip title="Notifications"><IconButton size="small" aria-label={`${unreadCount} unread notifications`} onClick={() => navigate('/notifications')}><Badge badgeContent={unreadCount || undefined} color="primary"><Box component="span" sx={{ fontSize: 18, lineHeight: 1, color: 'primary.main' }}>●</Box></Badge></IconButton></Tooltip><Avatar sx={{ width: 34, height: 34, bgcolor: 'primary.main', fontSize: 14 }}>JD</Avatar><Typography variant="body2" sx={{ display: { xs: 'none', sm: 'block' }, fontWeight: 600 }}>Jordan Doe</Typography><Button size="small" color="inherit" onClick={handleLogout}>Logout</Button></Box></Toolbar></AppBar><Box sx={{ display: { xs: 'block', md: 'none' }, bgcolor: 'background.paper', borderBottom: 1, borderColor: 'divider', overflowX: 'auto' }}><Navigation mobile /></Box><Box component="main" sx={{ width: '100%', maxWidth: 1440, mx: 'auto', boxSizing: 'border-box', p: { xs: 2, sm: 3, lg: 5 } }}>{children}</Box></Box></Box>
}
