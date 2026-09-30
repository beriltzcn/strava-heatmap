namespace StravaHeatmap.Api.Services;

// Strava ile ilgili hatalari diger hatalardan ayirmak icin kendi tipimiz.
public class StravaAuthException : Exception
{
    public StravaAuthException(string message) : base(message)
    {
    }
}
