import { useState, type ChangeEvent, type FormEvent } from 'react'
import { Button, CircularProgress, Link, TextField, Typography } from '@mui/material'
import { AuthCard, PasswordField, type AuthErrors, type AuthValues } from '../components/auth/AuthForm'
import { AuthPageLayout } from '../components/auth/AuthPageLayout'
import { register } from '../services/authApi'
import { storeAuthToken } from '../services/authStorage'
import { useNavigate, Link as RouterLink } from 'react-router-dom'

const initialValues: AuthValues = { name: '', username: '', email: '', password: '' }

function validate(values: AuthValues): AuthErrors {
  const errors: AuthErrors = {}
  if (!values.name.trim()) errors.name = 'Name is required.'
  if (!values.username.trim()) errors.username = 'Username is required.'
  else if (values.username.trim().length < 3) errors.username = 'Username must be at least 3 characters.'
  if (!values.email.trim()) errors.email = 'Email is required.'
  else if (!/^\S+@\S+\.\S+$/.test(values.email)) errors.email = 'Enter a valid email address.'
  if (!values.password) errors.password = 'Password is required.'
  else if (values.password.length < 6) errors.password = 'Password must be at least 6 characters.'
  return errors
}

export function Register() {
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
      const response = await register({ name: values.name.trim(), username: values.username.trim(), email: values.email.trim(), password: values.password })
      storeAuthToken(response.token)
      navigate('/', { replace: true })
    } catch (error) {
      setApiError(error instanceof Error ? error.message : 'Registration failed. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return <AuthPageLayout><AuthCard title="Create your account" subtitle="Set up your Enterprise Knowledge Platform workspace." onSubmit={handleSubmit} error={apiError} notice={notice}>
    <TextField fullWidth required label="Name" name="name" value={values.name} onChange={handleChange} error={Boolean(errors.name)} helperText={errors.name} autoComplete="name" disabled={loading} />
    <TextField fullWidth required label="Username" name="username" value={values.username} onChange={handleChange} error={Boolean(errors.username)} helperText={errors.username} autoComplete="username" disabled={loading} />
    <TextField fullWidth required label="Email" name="email" type="email" value={values.email} onChange={handleChange} error={Boolean(errors.email)} helperText={errors.email} autoComplete="email" disabled={loading} />
    <PasswordField label="Password" name="password" value={values.password} onChange={handleChange} error={errors.password} disabled={loading} />
    <Button type="submit" variant="contained" size="large" disabled={loading} sx={{ mt: 1, py: 1.35 }}>{loading ? <CircularProgress size={22} color="inherit" /> : 'Create account'}</Button>
    <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', pt: 1 }}>Already have an account? <Link component={RouterLink} to="/login" underline="hover">Sign in</Link></Typography>
  </AuthCard></AuthPageLayout>
}
