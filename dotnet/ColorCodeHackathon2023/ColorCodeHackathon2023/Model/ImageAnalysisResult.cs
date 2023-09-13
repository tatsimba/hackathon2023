namespace ColorCodeHackathon2023.Model;

using System.Text.Json.Serialization;

public class ImageAnalysisResult
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }

    [JsonPropertyName("denseCaptions")]
    public List<string> DenseCaptions { get; set; }

    [JsonPropertyName("result")]
    public string Result { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;
}
