import { useEffect, useMemo, useState } from 'react'
import type { ActivitiesResponse, ActivitySummary } from './types'
import { decodePolyline } from './polyline'
import RouteMap from './RouteMap'
import type { MapMode } from './RouteMap'

function App() {
  const [activities, setActivities] = useState<ActivitySummary[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [mode, setMode] = useState<MapMode>('routes')

  // Bilesen ekrana geldiginde bir kez calisir.
  // Sadece rotasi olanlari istiyoruz; rotasizlari haritada cizemeyiz.
  useEffect(() => {
    fetch('/api/activities?onlyWithRoute=true&limit=200')
      .then((res) => {
        if (!res.ok) throw new Error(`Backend hatasi: HTTP ${res.status}`)
        return res.json()
      })
      .then((data: ActivitiesResponse) => setActivities(data.activities))
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false))
  }, [])

  // Polylinelari bir kez cozup hazir tutuyoruz.
  // useMemo olmasa her yeniden cizimde bosa is yapilirdi.
  const routes = useMemo(
    () =>
      activities
        .filter((a) => a.polyline)
        .map((a) => ({
          id: a.id,
          points: decodePolyline(a.polyline!),
        })),
    [activities],
  )

  if (loading) return <p style={{ padding: 24 }}>Yukleniyor...</p>
  if (error) return <p style={{ padding: 24, color: '#b91c1c' }}>Hata: {error}</p>

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100vh' }}>
      <header
        style={{
          padding: '12px 20px',
          borderBottom: '1px solid #e5e7eb',
          display: 'flex',
          alignItems: 'center',
          gap: 12,
        }}
      >
        <strong>StravaHeatmap</strong>
        <span style={{ color: '#6b7280' }}>{routes.length} rota</span>

        <div style={{ marginLeft: 'auto', display: 'flex', gap: 8 }}>
          <button
            onClick={() => setMode('routes')}
            disabled={mode === 'routes'}
            style={{ padding: '6px 14px', cursor: 'pointer' }}
          >
            Rotalar
          </button>
          <button
            onClick={() => setMode('heat')}
            disabled={mode === 'heat'}
            style={{ padding: '6px 14px', cursor: 'pointer' }}
          >
            Isı haritası
          </button>
        </div>
      </header>

      <div style={{ flex: 1, minHeight: 0 }}>
        <RouteMap routes={routes} mode={mode} />
      </div>
    </div>
  )
}

export default App
