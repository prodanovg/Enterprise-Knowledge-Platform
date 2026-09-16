import { useEffect, useMemo, useState, type FormEvent } from 'react'
import {
  Alert, Box, Button, Card, CardContent, Chip, CircularProgress, Divider,
  List, ListItemButton, ListItemText, Stack, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, TextField, Typography,
} from '@mui/material'
import {
  getGraphEntity, getGraphSubgraph, searchGraphEntities,
  type EntityGraphResponse, type GraphEntityResponse,
  type GraphRelationshipResponse, type GraphSubgraphResponse,
} from '../services/graphQueryApi'
import { GraphVisualization } from '../components/knowledgeGraph/GraphVisualization'

function confidenceLabel(value: number) { return `${value <= 1 ? Math.round(value * 100) : Math.round(value)}%` }

function RelationshipList({ title, relationships, entities, selectedId }: { title: string; relationships: GraphRelationshipResponse[]; entities: GraphEntityResponse[]; selectedId: string }) {
  const names = new Map(entities.map((entity) => [entity.id, entity.name]))
  return <Card sx={{ height: '100%' }}><CardContent><Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 2 }}><Typography variant="h6">{title}</Typography><Chip size="small" label={relationships.length} /></Stack>{relationships.length === 0 ? <Typography variant="body2" color="text.secondary">No {title.toLowerCase()} found.</Typography> : <Stack divider={<Divider flexItem />} spacing={0}>{relationships.map((relationship) => { const otherId = relationship.sourceEntityId === selectedId ? relationship.targetEntityId : relationship.sourceEntityId; return <Box key={relationship.id} sx={{ py: 1.5 }}><Typography sx={{ fontWeight: 600, overflowWrap: 'anywhere' }}>{relationship.predicate}</Typography><Typography variant="body2" color="text.secondary" sx={{ overflowWrap: 'anywhere' }}>{names.get(otherId) ?? otherId}</Typography><Typography variant="caption" color="text.secondary">Confidence {confidenceLabel(relationship.confidence)}</Typography></Box> })}</Stack>}</CardContent></Card>
}

export function KnowledgeGraph() {
  const [query, setQuery] = useState('')
  const [entities, setEntities] = useState<GraphEntityResponse[]>([])
  const [selectedId, setSelectedId] = useState('')
  const [details, setDetails] = useState<EntityGraphResponse | null>(null)
  const [subgraph, setSubgraph] = useState<GraphSubgraphResponse | null>(null)
  const [searchLoading, setSearchLoading] = useState(true)
  const [detailsLoading, setDetailsLoading] = useState(false)
  const [error, setError] = useState('')

  const runSearch = async (searchQuery: string) => {
    setSearchLoading(true); setError('')
    try { setEntities(await searchGraphEntities(searchQuery.trim())) }
    catch (requestError) { setEntities([]); setError(requestError instanceof Error ? requestError.message : 'Entities could not be loaded.') }
    finally { setSearchLoading(false) }
  }

  useEffect(() => {
    const timer = window.setTimeout(() => { void runSearch('') }, 0)
    return () => window.clearTimeout(timer)
  }, [])

  const selectEntity = async (entity: GraphEntityResponse) => {
    setSelectedId(entity.id); setDetailsLoading(true); setError('')
    try {
      const [loadedDetails, loadedSubgraph] = await Promise.all([getGraphEntity(entity.id), getGraphSubgraph(entity.id, 1)])
      setDetails(loadedDetails); setSubgraph(loadedSubgraph)
    } catch (requestError) { setDetails(null); setSubgraph(null); setError(requestError instanceof Error ? requestError.message : 'Entity details could not be loaded.') }
    finally { setDetailsLoading(false) }
  }

  const handleSearch = (event: FormEvent<HTMLFormElement>) => { event.preventDefault(); void runSearch(query) }
  const subgraphNames = useMemo(() => new Map(subgraph?.entities.map((entity) => [entity.id, entity.name]) ?? []), [subgraph])
  const handleGraphEntitySelect = (entityId: string) => {
    const entity = subgraph?.entities.find((item) => item.id === entityId)
    if (entity) void selectEntity(entity)
  }

  return <Stack spacing={3}>
    <Box><Typography variant="h4" gutterBottom>Knowledge Graph</Typography><Typography color="text.secondary">Search entities and explore their connected knowledge.</Typography></Box>
    {error && <Alert severity="error">{error}</Alert>}
    <Card><CardContent><Box component="form" onSubmit={handleSearch} sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}><TextField fullWidth label="Search entities" placeholder="Try a person, organization, or concept" value={query} onChange={(event) => setQuery(event.target.value)} sx={{ flex: '1 1 300px' }} /><Button type="submit" variant="contained" disabled={searchLoading} sx={{ minWidth: 110 }}>Search</Button></Box></CardContent></Card>
    <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'minmax(260px, 0.8fr) minmax(0, 2fr)' }, gap: 3, alignItems: 'start' }}>
      <Card><CardContent sx={{ p: 0 }}><Box sx={{ p: 2.5, pb: 1.5 }}><Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}><Typography variant="h6">Entities</Typography><Chip size="small" label={entities.length} /></Stack></Box><Divider />{searchLoading ? <Box sx={{ display: 'grid', placeItems: 'center', py: 6 }}><CircularProgress size={28} /></Box> : entities.length === 0 ? <Typography color="text.secondary" sx={{ p: 2.5 }}>No matching entities found.</Typography> : <List sx={{ p: 1 }}>{entities.map((entity) => <ListItemButton key={entity.id} selected={selectedId === entity.id} onClick={() => void selectEntity(entity)} sx={{ borderRadius: 2, mb: 0.5 }}><ListItemText primary={entity.name} secondary={`${entity.entityTypeName || 'Uncategorized'} · ${entity.canonicalName || 'No canonical name'}`} /></ListItemButton>)}</List>}</CardContent></Card>
      {detailsLoading ? <Box sx={{ display: 'grid', placeItems: 'center', py: 10 }}><CircularProgress /></Box> : details ? <Stack spacing={3}>
        <Card><CardContent><Stack direction={{ xs: 'column', sm: 'row' }} sx={{ justifyContent: 'space-between', gap: 2 }}><Box><Typography variant="h5" sx={{ fontWeight: 700, overflowWrap: 'anywhere' }}>{details.entity.name}</Typography><Typography color="text.secondary" sx={{ mt: 0.5 }}>Canonical name: {details.entity.canonicalName || '—'}</Typography></Box><Chip label={details.entity.entityTypeName || 'Uncategorized'} color="primary" variant="outlined" /></Stack><Divider sx={{ my: 2 }} /><Typography variant="body2" color="text.secondary">Entity ID: {details.entity.id}</Typography></CardContent></Card>
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', lg: '1fr 1fr' }, gap: 2 }}><RelationshipList title="Outgoing relationships" relationships={details.outgoingRelationships} entities={details.connectedEntities} selectedId={details.entity.id} /><RelationshipList title="Incoming relationships" relationships={details.incomingRelationships} entities={details.connectedEntities} selectedId={details.entity.id} /></Box>
        <Card><CardContent><Typography variant="h6" gutterBottom>Interactive subgraph</Typography><Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>Drag entities to arrange them, then use the controls to zoom or pan. Select a node to inspect it.</Typography>{subgraph ? <GraphVisualization subgraph={subgraph} selectedEntityId={details.entity.id} onEntitySelect={handleGraphEntitySelect} /> : <Box sx={{ display: 'grid', placeItems: 'center', py: 8 }}><CircularProgress /></Box>}</CardContent></Card>
        <Card><CardContent><Typography variant="h6" gutterBottom>Depth 1 subgraph</Typography><Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>Structured neighboring entities and relationships for future graph visualization.</Typography>{!subgraph || subgraph.entities.length === 0 ? <Typography color="text.secondary">No connected entities were returned.</Typography> : <Stack spacing={2}><Box><Typography sx={{ fontWeight: 700, mb: 1 }}>Entities ({subgraph.entities.length})</Typography><Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>{subgraph.entities.map((entity) => <Chip key={entity.id} label={`${entity.name} · ${entity.entityTypeName || 'Unknown type'}`} variant={entity.id === details.entity.id ? 'filled' : 'outlined'} />)}</Stack></Box><Divider /><Typography sx={{ fontWeight: 700 }}>Relationships ({subgraph.relationships.length})</Typography>{subgraph.relationships.length === 0 ? <Typography variant="body2" color="text.secondary">No relationships were returned.</Typography> : <TableContainer><Table size="small"><TableHead><TableRow><TableCell>Source</TableCell><TableCell>Predicate</TableCell><TableCell>Target</TableCell><TableCell>Confidence</TableCell></TableRow></TableHead><TableBody>{subgraph.relationships.map((relationship) => <TableRow key={relationship.id}><TableCell sx={{ overflowWrap: 'anywhere' }}>{subgraphNames.get(relationship.sourceEntityId) ?? relationship.sourceEntityId}</TableCell><TableCell sx={{ overflowWrap: 'anywhere' }}>{relationship.predicate}</TableCell><TableCell sx={{ overflowWrap: 'anywhere' }}>{subgraphNames.get(relationship.targetEntityId) ?? relationship.targetEntityId}</TableCell><TableCell>{confidenceLabel(relationship.confidence)}</TableCell></TableRow>)}</TableBody></Table></TableContainer>}</Stack>}</CardContent></Card>
      </Stack> : <Card><CardContent sx={{ py: 8, textAlign: 'center' }}><Typography variant="h6" gutterBottom>Select an entity</Typography><Typography color="text.secondary">Choose an entity from the search results to inspect its details and subgraph.</Typography></CardContent></Card>}
    </Box>
  </Stack>
}
