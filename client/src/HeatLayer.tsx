import { useEffect } from 'react'
import { useMap } from 'react-leaflet'
import * as L from 'leaflet'
import 'leaflet.heat'

// leaflet.heat, Leaflet'in L nesnesine "heatLayer" fonksiyonu ekliyor ama
// TypeScript bunu bilmiyor. Tip tanimi olmadigi icin kucuk bir cast yapiyoruz.
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
      radius: 14, // her noktanin etrafinda ne kadar yayilsin (piksel)
      blur: 18, // kenar yumusakligi
      maxZoom: 17, // bu yakınlastirmadan sonra noktalar daha fazla buyumesin
      minOpacity: 0.15,
      // Az ugradigin yer mavi, cok ugradigin yer kirmizi.
      gradient: {
        0.2: '#3b82f6',
        0.4: '#22c55e',
        0.6: '#eab308',
        0.8: '#f97316',
        1.0: '#dc2626',
      },
    })

    layer.addTo(map)

    // Bilesen ekrandan kalkarsa katmani da temizle; yoksa harita uzerinde kalir.
    return () => {
      map.removeLayer(layer)
    }
  }, [map, points])

  return null
}
