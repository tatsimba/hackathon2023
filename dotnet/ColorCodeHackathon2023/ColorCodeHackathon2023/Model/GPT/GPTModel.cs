using System.Text.Json.Serialization;

namespace ColorCodeHackathon2023.Model.GPT;

public class ModelPrompt
{
    [JsonPropertyName("prompt")]
    public string? Prompt
    {
        get;
        set;
    }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens
    {
        get;
        set;
    }

    [JsonPropertyName("temperature")]
    public double Temperature
    {
        get;
        set;
    }

    [JsonPropertyName("top_p")]
    public int TopP
    {
        get;
        set;
    }

    [JsonPropertyName("n")]
    public int N
    {
        get;
        set;
    }

    [JsonPropertyName("stream")]
    public bool Stream
    {
        get;
        set;
    }

    [JsonPropertyName("logprobs")]
    public object? LogProbs
    {
        get;
        set;
    }

    [JsonPropertyName("stop")]
    public string? Stop
    {
        get;
        set;
    }
};

public class Choice
{
    [JsonPropertyName("text")]
    public string? Text
    {
        get;
        set;
    }

    [JsonPropertyName("index")]
    public int Index
    {
        get;
        set;
    }

    [JsonPropertyName("logprobs")]
    public object? LogProbs
    {
        get;
        set;
    }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason
    {
        get;
        set;
    }
}

public class StreamResponse
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("object")]
    public string? Object
    {
        get;
        set;
    }

    [JsonPropertyName("created")]
    public int Created
    {
        get;
        set;
    }

    [JsonPropertyName("choices")]
    public List<Choice>? Choices
    {
        get;
        set;
    }

    [JsonPropertyName("model")]
    public string? Model
    {
        get;
        set;
    }
}