namespace ColorCodeHackathon2023.Model;

using System.Text.Json.Serialization;

public class ImageAnalysisResult
{
    [JsonPropertyName("garmentColorResult")]
    public string GarmentColorResult { get; set; }

    [JsonPropertyName("matchingColorResult")]
    public string MatchingColorResult { get; set; }

    [JsonPropertyName("weatherResult")]
    public string WeatherResult { get; set; }

    [JsonPropertyName("denseCaptions")]
    public List<string> DenseCaptions { get; set; }
}
