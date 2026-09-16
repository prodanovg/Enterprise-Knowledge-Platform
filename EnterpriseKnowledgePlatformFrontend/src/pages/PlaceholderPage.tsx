import { Card, CardContent, Typography } from '@mui/material'

export function PlaceholderPage({ title, description }: { title: string; description: string }) {
  return (
    <Card>
      <CardContent sx={{ p: { xs: 3, sm: 5 } }}>
        <Typography variant="h4" gutterBottom>{title}</Typography>
        <Typography color="text.secondary">{description}</Typography>
      </CardContent>
    </Card>
  )
}
