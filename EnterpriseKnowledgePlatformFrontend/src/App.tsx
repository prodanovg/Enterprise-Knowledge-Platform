import { CssBaseline, ThemeProvider } from '@mui/material'
import { BrowserRouter, Navigate, Outlet, Route, Routes } from 'react-router-dom'
import { AppShell } from './components/AppShell'
import { Dashboard } from './pages/Dashboard'
import { Documents } from './pages/Documents'
import { Login } from './pages/Login'
import { KnowledgeGraph } from './pages/KnowledgeGraph'
import { Notifications } from './pages/Notifications'
import { PlaceholderPage } from './pages/PlaceholderPage'
import { Processing } from './pages/Processing'
import { ProcessingResults } from './pages/ProcessingResults'
import { Register } from './pages/Register'
import { Settings } from './pages/Settings'
import { getAuthToken } from './services/authStorage'
import { theme } from './theme'

function ProtectedRoutes() {
  return getAuthToken() ? <Outlet /> : <Navigate to="/login" replace />
}

function PublicRoutes() {
  return getAuthToken() ? <Navigate to="/" replace /> : <Outlet />
}

function ApplicationLayout() {
  return <AppShell><Outlet /></AppShell>
}

function App() {
  return <ThemeProvider theme={theme}><CssBaseline /><BrowserRouter><Routes>
    <Route element={<PublicRoutes />}>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
    </Route>
    <Route element={<ProtectedRoutes />}>
      <Route element={<ApplicationLayout />}>
        <Route path="/" element={<Dashboard />} />
        <Route path="/documents" element={<Documents />} />
        <Route path="/processing" element={<Processing />} />
        <Route path="/processing/:jobId/results" element={<ProcessingResults />} />
        <Route path="/knowledge-graph" element={<KnowledgeGraph />} />
        <Route path="/notifications" element={<Notifications />} />
        <Route path="/settings" element={<Settings />} />
        <Route path="*" element={<PlaceholderPage title="Page not found" description="The page you requested does not exist." />} />
      </Route>
    </Route>
  </Routes></BrowserRouter></ThemeProvider>
}

export default App
