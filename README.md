# StravaHeatmap

Self-hosted Strava activity visualizer. Connect your Strava account, import your
activities and see them on a real map: individual routes and a heatmap of
everywhere you have been.

Everything runs on your own machine. Your Strava data stays in your own SQLite
database, and no third party sees it.

## Features

- **Strava OAuth** — connect your account in a couple of clicks, no password sharing
- **Automatic token refresh** — keeps working after Strava's short-lived access tokens expire
- **Local storage** — activities are saved in SQLite, so the map loads without calling Strava
- **Route view** — every activity with GPS data drawn on OpenStreetMap
- **Heatmap view** — density map that shows where you go most often
- **One-click sync** — pull the latest activities from Strava

## Screenshot

<img width="2012" height="1663" alt="image" src="https://github.com/user-attachments/assets/e6fac38d-4ee3-47cd-9412-5ed4197a0818" />

## Requirements

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js 22](https://nodejs.org/)
- A Strava account

## Setup

### 1. Create a Strava API application

Go to <https://www.strava.com/settings/api> and create an application.

| Field | Value |
| --- | --- |
| Application Name | StravaHeatmap |
| Category | whichever fits best |
| Authorization Callback Domain | `localhost` |

After saving, the page shows a **Client ID** and a **Client Secret**.

> Strava limits new API applications to a single athlete. This project is built
> exactly for that model: your own instance, your own data, your own credentials.

### 2. Store your credentials

Credentials never go into the repository. .NET user secrets keeps them in your
user profile instead:

```bash
dotnet user-secrets init --project src/StravaHeatmap.Api
dotnet user-secrets set "Strava:ClientId" "YOUR_CLIENT_ID" --project src/StravaHeatmap.Api
dotnet user-secrets set "Strava:ClientSecret" "YOUR_CLIENT_SECRET" --project src/StravaHeatmap.Api
```

### 3. Run the backend

```bash
dotnet run --project src/StravaHeatmap.Api
```

The API listens on <https://localhost:7050>.

### 4. Run the frontend

In a second terminal:

```bash
cd client
npm install
npm run dev
```

The app opens on <http://localhost:5173>. Vite proxies `/api` to the backend, so
there is no CORS configuration to worry about.

### 5. Connect Strava and import activities

Open <https://localhost:7050/api/strava/connect> and authorize the application.
Then press **Sync with Strava** in the UI to import your activities.

## Docker

Docker builds the frontend and the API into a single image and serves both from
the same origin.

```bash
cp .env.example .env
# edit .env and fill in your Strava credentials
docker compose up --build
```

The app is then available on <http://localhost:8080>.

Remember to set the **Authorization Callback Domain** in your Strava application
to match the host you use. The default redirect URI is
`http://localhost:8080/api/strava/callback`.

## Project structure

```
src/StravaHeatmap.Api/    ASP.NET Core Web API, EF Core, Strava integration
  Controllers/            HTTP endpoints
  Data/                   DbContext and migrations
  Dtos/                   Shapes of the data sent to the frontend
  Models/                 Database entities
  Options/                Strongly typed configuration
  Services/               Strava client, token refresh, sync logic
tests/                    Automated tests
client/                   React + TypeScript frontend (Vite)
```

## How it works

The backend talks to the Strava API and stores activities in a local SQLite
database. Strava access tokens expire every six hours, so every API call first
checks the expiry and refreshes the token when needed.

The frontend fetches activities from `/api/activities`, decodes Strava's encoded
polylines into coordinates and draws them with Leaflet on OpenStreetMap tiles.
The heatmap uses the same coordinates, projected into a density layer.

## Roadmap

- [x] Project skeleton
- [x] Strava OAuth
- [x] Activity import
- [x] Polyline decoding
- [x] Route map
- [x] Heatmap
- [ ] Filters (date range, sport type)
- [ ] Statistics panel
- [ ] Automated tests for the sync logic

## License

MIT — see [LICENSE](LICENSE).
