import { useEffect, useState } from 'react'
import {
  applyEdgeChanges,
  applyNodeChanges,
  Background,
  Controls,
  MarkerType,
  MiniMap,
  ReactFlow,
  type Edge,
  type EdgeChange,
  type Node,
  type NodeChange,
} from '@xyflow/react'
import '@xyflow/react/dist/style.css'
import { Box, Typography } from '@mui/material'
import type { GraphSubgraphResponse } from '../../services/graphQueryApi'

type GraphNodeData = { label: string; entityType: string }

function buildNodes(subgraph: GraphSubgraphResponse): Node<GraphNodeData>[] {
  return subgraph.entities.map((entity, index) => ({
    id: entity.id,
    position: { x: (index % 3) * 250, y: Math.floor(index / 3) * 150 },
    data: { label: entity.name, entityType: entity.entityTypeName || 'Entity' },
    style: { background: '#ffffff', border: '2px solid #2f5bea', borderRadius: 12, color: '#182230', padding: 12, width: 190, boxShadow: '0 4px 14px rgba(28, 45, 78, 0.12)' },
  }))
}

function buildEdges(subgraph: GraphSubgraphResponse): Edge[] {
  const entityIds = new Set(subgraph.entities.map((entity) => entity.id))
  return subgraph.relationships.filter((relationship) => entityIds.has(relationship.sourceEntityId) && entityIds.has(relationship.targetEntityId)).map((relationship) => ({
    id: relationship.id,
    source: relationship.sourceEntityId,
    target: relationship.targetEntityId,
    label: relationship.predicate,
    markerEnd: { type: MarkerType.ArrowClosed },
    style: { stroke: '#687386', strokeWidth: 1.5 },
    labelStyle: { fill: '#4b5565', fontSize: 11, fontWeight: 600 },
    labelBgStyle: { fill: '#f5f7fb', fillOpacity: 0.9 },
  }))
}

export function GraphVisualization({ subgraph, selectedEntityId, onEntitySelect }: { subgraph: GraphSubgraphResponse; selectedEntityId: string; onEntitySelect: (entityId: string) => void }) {
  const [nodes, setNodes] = useState<Node<GraphNodeData>[]>([])
  const [edges, setEdges] = useState<Edge[]>([])

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setNodes(buildNodes(subgraph).map((node) => node.id === selectedEntityId ? { ...node, style: { ...node.style, border: '2px solid #0f9d8a', background: '#eefbf8' } } : node))
      setEdges(buildEdges(subgraph))
    }, 0)
    return () => window.clearTimeout(timer)
  }, [subgraph, selectedEntityId])

  const handleNodesChange = (changes: NodeChange[]) => setNodes((current) => applyNodeChanges(changes, current) as Node<GraphNodeData>[])
  const handleEdgesChange = (changes: EdgeChange[]) => setEdges((current) => applyEdgeChanges(changes, current))

  if (subgraph.entities.length === 0) return <Box sx={{ height: 360, display: 'grid', placeItems: 'center', bgcolor: '#f5f7fb', borderRadius: 2 }}><Typography color="text.secondary">No graph data is available for this entity.</Typography></Box>

  return <Box sx={{ height: { xs: 420, md: 520 }, border: 1, borderColor: 'divider', borderRadius: 2, overflow: 'hidden', bgcolor: '#f5f7fb' }}><ReactFlow nodes={nodes} edges={edges} onNodesChange={handleNodesChange} onEdgesChange={handleEdgesChange} onNodeClick={(_, node) => onEntitySelect(node.id)} fitView minZoom={0.3} maxZoom={2}><MiniMap pannable zoomable nodeColor="#2f5bea" /><Controls showInteractive /><Background gap={24} size={1} color="#dce2ed" /></ReactFlow></Box>
}
