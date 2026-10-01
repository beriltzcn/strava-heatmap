import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ActivitiesResponse, ActivitySummary } from './types'
import { decodePolyline } from './polyline'
import RouteMap from './RouteMap'
import type { MapMode } from './RouteMap'

function App() {
  const [activities, setActivities] = useState<ActivitySummary[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [mode, setMode] = useState<MapMode>('routes')

  const [syncing, setSyncing] = useState(false)
  const [message, setMessage] = useState<string | null>(null)

  // Sadece rotasi olanlari istiyoruz; rotasizlari haritada cizemeyiz.
  const loadActivities = useCallback(async () => {
    const res = await fetch('/api/activities?onlyWithRoute=true&limit=200')
    if (!res.ok) throw new Error(`Backend error: HTTP ${res.status}`)
    const data: ActivitiesResponse = await res.json()
    setActivities(data.activities)
  }, [])

  // Bilesen ekrana geldiginde bir kez calisir.
  useEffect(() => {
    loadActivities()
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false))
  }, [loadActivities])

  // Strava'dan yeni aktiviteleri cekip veritabanina kaydeder.
  async function handleSync() {
    setSyncing(true)
    setMessage(null)

    try {
      const res = await fetch('/api/strava/sync?maxPages=5', { method: 'POST' })

      if (!res.ok) {
        const body = await res.json().catch(() => null)
        throw new Error(body?.message ?? `HTTP ${res.status}`)
      }

      const data = await res.json()
      setMessage(`${data.added} added, ${data.updated} updated`)

      // Yeni veri geldiyse haritayi tazele.
      await loadActivities()
    } catch (err) {
      setMessage(`Error: ${(err as Error).message}`)
    } finally {
      setSyncing(false)
    }
  }

  // Polylinelari bir kez cozup hazir tutuyoruz.
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

  if (loading) return <p style={{ padding: 24 }}>Loading...</p>
  if (error) return <p style={{ padding: 24, color: '#b91c1c' }}>Error: {error}</p>

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100vh' }}>
      <header
        style={{
          padding: '12px 20px',
          borderBottom: '1px solid #e5e7eb',
          display: 'flex',
          alignItems: 'center',
          gap: 12,
          flexWrap: 'wrap',
        }}
      >
        <strong>StravaHeatmap</strong>
        <span style={{ color: '#6b7280' }}>{routes.length} routes</span>

        {message && <span style={{ color: '#2563eb' }}>{message}</span>}

        <div style={{ marginLeft: 'auto', display: 'flex', gap: 8 }}>
          <button
            onClick={handleSync}
            disabled={syncing}
            style={{ padding: '6px 14px', cursor: 'pointer' }}
          >
            {syncing ? 'Syncing...' : 'Sync with Strava'}
          </button>

          <button
            onClick={() => setMode('routes')}
            disabled={mode === 'routes'}
            style={{ padding: '6px 14px', cursor: 'pointer' }}
          >
            Routes
          </button>

          <button
            onClick={() => setMode('heat')}
            disabled={mode === 'heat'}
            style={{ padding: '6px 14px', cursor: 'pointer' }}
          >
            Heatmap
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
