import { createTheme } from '@mui/material/styles'

export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#3157d5', contrastText: '#ffffff' },
    secondary: { main: '#0b8f7d' },
    background: { default: '#f2f4f1', paper: '#fbfcfa' },
    text: { primary: '#1c2730', secondary: '#66737b' },
  },
  typography: {
    fontFamily: 'Inter, "Segoe UI", Roboto, Arial, sans-serif',
    h4: { fontWeight: 750, letterSpacing: '-0.035em' },
    h6: { fontWeight: 750, letterSpacing: '-0.02em' },
    button: { textTransform: 'none', fontWeight: 650, letterSpacing: '0.01em' },
  },
  shape: { borderRadius: 9 },
  components: {
    MuiCssBaseline: { styleOverrides: { body: { backgroundImage: 'radial-gradient(#d9dfdb 0.7px, transparent 0.7px)', backgroundSize: '18px 18px' } } },
    MuiCard: { styleOverrides: { root: { border: '1px solid #dce3df', boxShadow: '0 2px 8px rgba(28, 45, 38, 0.045)', backgroundImage: 'none' } } },
    MuiButton: { styleOverrides: { root: { minHeight: 38, borderRadius: 7 }, contained: { boxShadow: '0 2px 0 rgba(24, 39, 48, 0.12)', '&:hover': { boxShadow: '0 2px 0 rgba(24, 39, 48, 0.12)' } } } },
    MuiChip: { styleOverrides: { root: { fontWeight: 650, borderRadius: 6 } } },
    MuiTableCell: { styleOverrides: { head: { fontWeight: 750, color: '#53616a', backgroundColor: '#f0f3f0' }, root: { borderColor: '#e0e6e2' } } },
    MuiTextField: { defaultProps: { size: 'small' } },
    MuiDialog: { styleOverrides: { paper: { border: '1px solid #dce3df', boxShadow: '0 16px 40px rgba(28, 45, 38, 0.15)' } } },
  },
})
