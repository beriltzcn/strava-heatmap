// Backend'in /api/activities ucundan donen verinin sekli.

export interface ActivitySummary {
  id: number
  name: string | null
  sportType: string | null
  startDate: string
  distanceKm: number
  movingTimeMinutes: number
  elevationGainMeters: number
  hasRoute: boolean
  polyline: string | null
}

export interface ActivitiesResponse {
  count: number
  activities: ActivitySummary[]
}

// Haritada cizilecek tek bir rota.
export interface RouteLine {
  id: number
  points: [number, number][]
}
