import { Box, Container, Typography } from '@mui/material'
import type { ReactNode } from 'react'

export function AuthPageLayout({ children }: { children: ReactNode }) {
  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', display: 'flex', alignItems: 'center', py: { xs: 3, sm: 6 } }}>
      <Container maxWidth="sm">
        {children}
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 3,mr:12, textAlign: 'center' }}>
          Enterprise Knowledge Platform
        </Typography>
      </Container>
    </Box>
  )
}
