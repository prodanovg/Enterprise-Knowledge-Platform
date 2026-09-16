import { useState, type ChangeEvent, type FormEvent } from 'react'
import { Button, CircularProgress, Link, TextField, Typography } from '@mui/material'
import { AuthCard, PasswordField, type AuthErrors, type AuthValues } from '../components/auth/AuthForm'
import { AuthPageLayout } from '../components/auth/AuthPageLayout'
import { login } from '../services/authApi'
import { storeAuthToken } from '../services/authStorage'
import { useNavigate, Link as RouterLink } from 'react-router-dom'

const initialValues: AuthValues = { username: '', password: '' }

function validate(values: AuthValues): AuthErrors {
  const errors: AuthErrors = {}
  if (!values.username.trim()) errors.username = 'Username is required.'
  if (!values.password) errors.password = 'Password is required.'
  return errors
}

export function Login() {
  const navigate = useNavigate()
  const [values, setValues] = useState(initialValues)
  const [errors, setErrors] = useState<AuthErrors>({})
  const [notice, setNotice] = useState('')
  const [apiError, setApiError] = useState('')
  const [loading, setLoading] = useState(false)
  const handleChange = (event: ChangeEvent<HTMLInputElement>) => setValues((current) => ({ ...current, [event.target.name]: event.target.value }))
  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (loading) return
    const nextErrors = validate(values)
    setErrors(nextErrors)
    setApiError('')
    setNotice('')
    if (Object.keys(nextErrors).length > 0) return

    setLoading(true)
    try {
      const response = await login({ username: values.username.trim(), password: values.password })
      storeAuthToken(response.token)
      navigate('/', { replace: true })
    } catch (error) {
      setApiError(error instanceof Error ? error.message : 'Sign-in failed. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return <AuthPageLayout><AuthCard title="Welcome back" subtitle="Sign in to access your knowledge workspace." onSubmit={handleSubmit} error={apiError} notice={notice}>
    <TextField fullWidth required label="Username" name="username" value={values.username} onChange={handleChange} error={Boolean(errors.username)} helperText={errors.username} autoComplete="username" disabled={loading} />
    <PasswordField label="Password" name="password" value={values.password} onChange={handleChange} error={errors.password} disabled={loading} />
    <Button type="submit" variant="contained" size="large" disabled={loading} sx={{ mt: 1, py: 1.35 }}>{loading ? <CircularProgress size={22} color="inherit" /> : 'Sign in'}</Button>
    <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', pt: 1 }}>New to the platform? <Link component={RouterLink} to="/register" underline="hover">Create an account</Link></Typography>
  </AuthCard></AuthPageLayout>
}
