// Decodes Strava's "encoded polyline" format into coordinates.
//
// The idea: Strava stores route points as consecutive differences (deltas),
// and packs each number into 5-bit chunks so the whole route fits in one string.
// The loop below unpacks that string into [latitude, longitude] pairs.
//
// The format was defined by Google and is used by many map services.
export function decodePolyline(encoded: string): [number, number][] {
  const points: [number, number][] = []

  let index = 0
  let lat = 0
  let lng = 0

  while (index < encoded.length) {
    // 1) Read the latitude delta
    let result = 0
    let shift = 0
    let byte: number

    do {
      byte = encoded.charCodeAt(index++) - 63
      result |= (byte & 0x1f) << shift
      shift += 5
    } while (byte >= 0x20)

    // The lowest bit is the sign; the remaining bits hold the magnitude.
    lat += result & 1 ? ~(result >> 1) : result >> 1

    // 2) Read the longitude delta
    result = 0
    shift = 0

    do {
      byte = encoded.charCodeAt(index++) - 63
      result |= (byte & 0x1f) << shift
      shift += 5
    } while (byte >= 0x20)

    lng += result & 1 ? ~(result >> 1) : result >> 1

    // Strava keeps five decimal places of precision: 1e5 = 100000
    points.push([lat / 1e5, lng / 1e5])
  }

  return points
}
