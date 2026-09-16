import { useState, type ChangeEvent, type FormEvent, type ReactNode } from 'react'
import { Alert, Box, Card, CardContent, IconButton, InputAdornment, SvgIcon, TextField } from '@mui/material'

export type AuthValues = Record<string, string>
export type AuthErrors = Record<string, string>

function VisibilityIcon() { return <SvgIcon fontSize="small"><path d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5m0 12.5c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5m0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3" /></SvgIcon> }
function VisibilityOffIcon() { return <SvgIcon fontSize="small"><path d="m2.1 3.51 18.38 18.38 1.1-1.1-3.5-3.5A12.8 12.8 0 0 0 23 12c-1.73-4.39-6-7.5-11-7.5-1.37 0-2.68.23-3.9.65L2.1 3.51ZM12 19.5c-5 0-9.27-3.11-11-7.5a12.8 12.8 0 0 1 4.05-5.4l2.2 2.2A5 5 0 0 0 12 17c.77 0 1.49-.17 2.14-.48l2.2 2.2c-1.35.5-2.8.78-4.34.78ZM12 6.5c5 0 9.27 3.11 11 7.5a12.8 12.8 0 0 1-2.9 4.2l-2.18-2.18A5 5 0 0 0 12 7c-.77 0-1.49.17-2.14.48L7.66 5.3A12.9 12.9 0 0 1 12 4.5Z" /></SvgIcon> }

export function PasswordField({ label, name, value, error, onChange, disabled = false }: { label: string; name: string; value: string; error?: string; onChange: (event: ChangeEvent<HTMLInputElement>) => void; disabled?: boolean }) {
  const [visible, setVisible] = useState(false)
  return <TextField fullWidth required label={label} name={name} type={visible ? 'text' : 'password'} value={value} onChange={onChange} error={Boolean(error)} helperText={error} disabled={disabled} autoComplete={name === 'password' ? 'current-password' : 'new-password'} slotProps={{ input: { endAdornment: <InputAdornment position="end"><IconButton aria-label={visible ? 'Hide password' : 'Show password'} onClick={() => setVisible((current) => !current)} edge="end">{visible ? <VisibilityOffIcon /> : <VisibilityIcon />}</IconButton></InputAdornment> } }} />
}

export function AuthCard({ title, subtitle, children, onSubmit, error, notice }: { title: string; subtitle: string; children: ReactNode; onSubmit: (event: FormEvent<HTMLFormElement>) => void; error?: string; notice?: string }) {
  return <Card sx={{ width: '100%', maxWidth: 480 }}><CardContent sx={{ p: { xs: 3, sm: 5 } }}><Box component="form" onSubmit={onSubmit} noValidate><Box sx={{ mb: 3 }}><Box sx={{ width: 42, height: 42, mb: 3, borderRadius: 2, bgcolor: 'primary.main', display: 'grid', placeItems: 'center', color: 'white', fontSize: 22, fontWeight: 800 }}>E</Box><Box component="h1" sx={{ m: 0, mb: 1, fontSize: { xs: 26, sm: 30 }, fontWeight: 700, color: 'text.primary' }}>{title}</Box><Box component="p" sx={{ m: 0, color: 'text.secondary' }}>{subtitle}</Box></Box>{error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}{notice && <Alert severity="info" sx={{ mb: 2 }}>{notice}</Alert>}<Box sx={{ display: 'grid', gap: 2 }}>{children}</Box></Box></CardContent></Card>
}
