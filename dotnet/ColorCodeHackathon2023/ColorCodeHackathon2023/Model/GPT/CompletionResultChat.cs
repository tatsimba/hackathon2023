namespace ColorCodeHackathon2023.Model.GPT;

using System.Text.Json.Serialization;

public class CompletionResultChat
{
    [JsonPropertyName("object")]
    public string ObjectType { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("choices")]
    public List<ChoiceResult> Choices { get; set; }
}

public class ChoiceResult
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; }
}