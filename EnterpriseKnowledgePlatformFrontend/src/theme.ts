import { createTheme } from '@mui/material/styles'

export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#2f5bea', contrastText: '#ffffff' },
    secondary: { main: '#0f9d8a' },
    background: { default: '#f5f7fb', paper: '#ffffff' },
    text: { primary: '#182230', secondary: '#687386' },
  },
  typography: {
    fontFamily: 'Inter, Roboto, "Helvetica Neue", Arial, sans-serif',
    h4: { fontWeight: 700, letterSpacing: '-0.02em' },
    h6: { fontWeight: 700 },
    button: { textTransform: 'none', fontWeight: 600 },
  },
  shape: { borderRadius: 12 },
  components: {
    MuiCard: { styleOverrides: { root: { border: '1px solid #e5e9f2', boxShadow: '0 4px 20px rgba(28, 45, 78, 0.06)' } } },
    MuiButton: { styleOverrides: { root: { minHeight: 40, borderRadius: 8 }, contained: { boxShadow: 'none', '&:hover': { boxShadow: 'none' } } } },
    MuiChip: { styleOverrides: { root: { fontWeight: 600 } } },
    MuiDialog: { styleOverrides: { paper: { border: '1px solid #e5e9f2', boxShadow: '0 18px 50px rgba(28, 45, 78, 0.16)' } } },
  },
})
