import { useEffect, useMemo } from 'react'
import { MapContainer, TileLayer, Polyline, useMap } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'
import type { RouteLine } from './types'
import HeatLayer from './HeatLayer'

export type MapMode = 'routes' | 'heat'

// Automatically fits the map so every route is visible.
function FitBounds({ routes }: { routes: RouteLine[] }) {
  const map = useMap()

  useEffect(() => {
    if (routes.length === 0) return

    const allPoints = routes.flatMap((route) => route.points)
    if (allPoints.length === 0) return

    map.fitBounds(allPoints, { padding: [30, 30] })
  }, [map, routes])

  return null
}

interface Props {
  routes: RouteLine[]
  mode: MapMode
}

export default function RouteMap({ routes, mode }: Props) {
  // Collect every route's points into one list for the heatmap.
  const allPoints = useMemo(() => routes.flatMap((route) => route.points), [routes])

  return (
    <MapContainer
      center={[41.01, 28.98]} // Istanbul; fitBounds takes over once routes load
      zoom={11}
      style={{ height: '100%', width: '100%' }}
    >
      {/* Map tiles. OpenStreetMap is free, but attribution is required. */}
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        url="https://tile.openstreetmap.org/{z}/{x}/{y}.png"
      />

      {mode === 'routes' &&
        routes.map((route) => (
          <Polyline
            key={route.id}
            positions={route.points}
            pathOptions={{ color: '#e11d48', weight: 3, opacity: 0.65 }}
          />
        ))}

      {mode === 'heat' && <HeatLayer points={allPoints} />}

      <FitBounds routes={routes} />
    </MapContainer>
  )
}
