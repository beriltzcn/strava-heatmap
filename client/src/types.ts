// Shape of the data returned by the backend's /api/activities endpoint.

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

// A single route ready to be drawn on the map.
export interface RouteLine {
  id: number
  points: [number, number][]
}
