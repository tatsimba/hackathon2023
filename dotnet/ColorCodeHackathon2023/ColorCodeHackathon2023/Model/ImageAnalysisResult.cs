namespace ColorCodeHackathon2023.Model;

using System.Text.Json.Serialization;

public class ImageAnalysisResult
{
    [JsonPropertyName("result")]
    public string Result { get; set; }

    [JsonPropertyName("denseCaptions")]
    public List<string> DenseCaptions { get; set; }
}
