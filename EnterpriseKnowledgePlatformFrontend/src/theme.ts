import { createTheme } from '@mui/material/styles'

export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#3d5a48', dark: '#2d4537', contrastText: '#ffffff' },
    secondary: { main: '#c36b3d' },
    background: { default: '#f3f4ef', paper: '#fcfcf8' },
    text: { primary: '#202b29', secondary: '#68736e' },
    divider: '#dce1d9',
  },
  typography: {
    fontFamily: 'Inter, "Segoe UI", Roboto, Arial, sans-serif',
    h4: { fontWeight: 760, letterSpacing: '-0.045em' },
    h6: { fontWeight: 750, letterSpacing: '-0.025em' },
    button: { textTransform: 'none', fontWeight: 650, letterSpacing: '0.01em' },
  },
  shape: { borderRadius: 7 },
  components: {
    MuiCssBaseline: { styleOverrides: { body: { backgroundImage: 'linear-gradient(rgba(61,90,72,.035) 1px, transparent 1px), linear-gradient(90deg, rgba(61,90,72,.035) 1px, transparent 1px)', backgroundSize: '28px 28px' } } },
    MuiCard: { styleOverrides: { root: { border: '1px solid #dce1d9', boxShadow: '0 2px 5px rgba(32, 43, 41, 0.035)', backgroundImage: 'none' } } },
    MuiButton: { styleOverrides: { root: { minHeight: 38, borderRadius: 6 }, contained: { boxShadow: '0 2px 0 rgba(32, 43, 41, 0.16)', '&:hover': { boxShadow: '0 2px 0 rgba(32, 43, 41, 0.16)' } } } },
    MuiChip: { styleOverrides: { root: { fontWeight: 650, borderRadius: 6 } } },
    MuiTableCell: { styleOverrides: { head: { fontWeight: 750, color: '#53615a', backgroundColor: '#eef1eb' }, root: { borderColor: '#e0e5dd' } } },
    MuiTextField: { defaultProps: { size: 'small' } },
    MuiDialog: { styleOverrides: { paper: { border: '1px solid #dce3df', boxShadow: '0 16px 40px rgba(28, 45, 38, 0.15)' } } },
  },
})
