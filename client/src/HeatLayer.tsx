import { useEffect } from 'react'
import { useMap } from 'react-leaflet'
import * as L from 'leaflet'
import 'leaflet.heat'

// leaflet.heat adds a "heatLayer" function to Leaflet's L object, but the
// package ships no type definitions. This small cast keeps TypeScript happy.
const createHeatLayer = (
  L as unknown as {
    heatLayer: (
      points: [number, number][],
      options: Record<string, unknown>,
    ) => L.Layer
  }
).heatLayer

interface Props {
  points: [number, number][]
}

export default function HeatLayer({ points }: Props) {
  const map = useMap()

  useEffect(() => {
    if (points.length === 0) return

    const layer = createHeatLayer(points, {
      radius: 14, // how far each point spreads, in pixels
      blur: 18, // edge softness
      maxZoom: 17, // stop growing the points beyond this zoom level
      minOpacity: 0.15,
      // Rarely visited places stay blue, frequently visited ones turn red.
      gradient: {
        0.2: '#3b82f6',
        0.4: '#22c55e',
        0.6: '#eab308',
        0.8: '#f97316',
        1.0: '#dc2626',
      },
    })

    layer.addTo(map)

    // Remove the layer when the component unmounts, otherwise it stays on the map.
    return () => {
      map.removeLayer(layer)
    }
  }, [map, points])

  return null
}
