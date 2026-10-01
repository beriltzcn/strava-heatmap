namespace StravaHeatmap.Api.Services;

// Our own exception type, so Strava failures can be told apart from other errors.
public class StravaAuthException : Exception
{
    public StravaAuthException(string message) : base(message)
    {
    }
}
