namespace ColorCodeHackathon2023.Services;

using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Options;
using Model.GPT;
using Settings;

public interface IGptService
{
    Task<(string, bool)> RunPromptAsync(string prompt, bool useSteam = false);
}

public class GptService : IGptService
{
    private const string CompletionsUriPath = "{0}/openai/deployments/gpt-35-turbo-16k/chat/completions?api-version=2023-08-01-preview";

    private readonly string _endpoint;
    private readonly string _apiKey;

    public GptService(IOptions<OpenAISettings> openAiSettings)
    {
        _endpoint = openAiSettings.Value.Endpoint;
        _apiKey = openAiSettings.Value.ApiKey;
    }

    public async Task<(string, bool)> RunPromptAsync(string prompt, bool useSteam = false)
    {
        var requestData = JsonSerializer.Serialize(new ModelPrompt
        {
            Messages = { new Message { Content = prompt } },
            MaxTokens = 1000,
            Temperature = 1,
            TopP = 1,
            N = 1
        });

        // Available models are listed here: https://msasg.visualstudio.com/QAS/_wiki/wikis/QAS.wiki/134728/Getting-Started-with-Substrate-LLM-API?anchor=available-models
        return useSteam ? await SendStreamRequest(requestData) : await SendRequest(requestData);
    }

    private async Task<(string, bool)> SendRequest(string requestData)
    {
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, string.Format(CompletionsUriPath, _endpoint));
        request.Content = new StringContent(requestData, Encoding.UTF8, "application/json");
        request.Headers.Add("api-key", _apiKey);

        try
        {
            var httpResponse = await httpClient.SendAsync(request);
            var responseContent = httpResponse.Content is not null
                ? await httpResponse.Content.ReadAsStringAsync()
                : string.Empty;

            if (!httpResponse?.IsSuccessStatusCode ?? true) return ($"Failed to query GPT - {responseContent}", false);
            var promptFilterResult = JsonSerializer.Deserialize<CompletionResultChat>(responseContent);
            return (promptFilterResult?.Choices.FirstOrDefault()?.Message.Content ?? string.Empty, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task<(string, bool)> SendStreamRequest(string requestData)
    {
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, string.Format(CompletionsUriPath, _endpoint));
        request.Content = new StringContent(requestData, Encoding.UTF8, "application/json");
        request.Headers.Add("api-key", _apiKey);

        var httpResponse = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        var stream = await httpResponse.Content.ReadAsStreamAsync();
        TextReader textReader = new StreamReader(stream);

        while (await textReader.ReadLineAsync() is { } line)
        {
            if (line.StartsWith("data: "))
            {
                var lineData = line.Substring(6);
                if (string.Equals(lineData, "[DONE]"))
                {
                    break;
                }

                var result = JsonSerializer.Deserialize<StreamResponse>(line.Substring(6));

                if (result?.Choices?.Count > 0)
                {
                    Console.Write(result.Choices[0].Text);
                    return (result.Choices[0].Text ?? string.Empty, true);
                }
            }
        }

        return (string.Empty, true);
    }
}
