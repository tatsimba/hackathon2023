namespace ColorCodeHackathon2023.Model.VisionPython;

using System.Text.Json.Serialization;

public class VisionPythonResponse
{
    [JsonPropertyName("captions")]
    public List<string> Captions { get; set; }
}
