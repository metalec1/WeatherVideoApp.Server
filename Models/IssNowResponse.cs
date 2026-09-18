using System.Text.Json.Serialization;

namespace WeatherVideoApp.Server.Models;

public record IssPosition(
    [property: JsonPropertyName("latitude")] string Latitude,
    [property: JsonPropertyName("longitude")] string Longitude
);

public record IssNowResponse(
    [property: JsonPropertyName("timestamp")] long Timestamp,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("iss_position")] IssPosition IssPosition
);
