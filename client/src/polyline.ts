// Strava'nin gonderdigi "encoded polyline" bicimini koordinatlara cevirir.
//
// Mantik sudur: Strava, rota noktalarini ardisik farklar (delta) olarak
// ve her sayiyi 5 bitlik parcalara bolerek tek bir metne sigdirir.
// Asagidaki dongu bu metni cozup [enlem, boylam] ciftleri uretir.
//
// Bicim Google tarafindan tanimlanmistir ve bir cok harita servisi kullanir.
export function decodePolyline(encoded: string): [number, number][] {
  const points: [number, number][] = []

  let index = 0
  let lat = 0
  let lng = 0

  while (index < encoded.length) {
    // 1) Enlem farkini oku
    let result = 0
    let shift = 0
    let byte: number

    do {
      byte = encoded.charCodeAt(index++) - 63
      result |= (byte & 0x1f) << shift
      shift += 5
    } while (byte >= 0x20)

    // En dusuk bit isaret bitidir; kalan bitler buyuklugu verir.
    lat += result & 1 ? ~(result >> 1) : result >> 1

    // 2) Boylam farkini oku
    result = 0
    shift = 0

    do {
      byte = encoded.charCodeAt(index++) - 63
      result |= (byte & 0x1f) << shift
      shift += 5
    } while (byte >= 0x20)

    lng += result & 1 ? ~(result >> 1) : result >> 1

    // Strava 5 ondalik basamak hassasiyet kullanir: 1e5 = 100000
    points.push([lat / 1e5, lng / 1e5])
  }

  return points
}
