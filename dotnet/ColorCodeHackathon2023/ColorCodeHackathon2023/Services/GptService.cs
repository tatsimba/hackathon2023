namespace ColorCodeHackathon2023.Services;

using Microsoft.Identity.Client.Extensions.Msal;
using System.Text.Json;
using Model.GPT;
using System.Text;
using Microsoft.Identity.Client;

public interface IGptService
{
    Task<string> RunPrompt(string prompt, bool useSteam = false, string modelType = "dev-text-davinci-003");
}

public class GptService : IGptService
{
    private const string Endpoint = "https://fe-26.qas.bing.net/completions";

    private static readonly IEnumerable<string> Scopes = new List<string>() {
        "api://68df66a4-cad9-4bfd-872b-c6ddde00d6b2/access"
    };

    private static readonly IPublicClientApplication App = PublicClientApplicationBuilder.Create("68df66a4-cad9-4bfd-872b-c6ddde00d6b2")
        .WithAuthority("https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47")
        .Build();

    public async Task<string> RunPrompt(string prompt, bool useSteam = false, string modelType = "dev-text-davinci-003")
    {
        var cacheHelper = await CreateCacheHelperAsync().ConfigureAwait(false);
        cacheHelper.RegisterCache(App.UserTokenCache);

        var requestData = JsonSerializer.Serialize(new ModelPrompt
        {
            Prompt = prompt,
            MaxTokens = 4000,
            Temperature = 1,
            TopP = 1,
            N = 5,
            Stream = useSteam,
            LogProbs = null,
            Stop = useSteam ? "\r\n" : "\n"
        });

        // Available models are listed here: https://msasg.visualstudio.com/QAS/_wiki/wikis/QAS.wiki/134728/Getting-Started-with-Substrate-LLM-API?anchor=available-models
        return useSteam ? await SendStreamRequest(modelType, requestData) : await SendRequest(modelType, requestData);
    }

    static async Task<string> GetToken()
    {

        var accounts = await App.GetAccountsAsync();
        AuthenticationResult? result = null;
        if (accounts.Any())
        {
            var chosen = accounts.First();

            try
            {
                result = await App.AcquireTokenSilent(Scopes, chosen).ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // cannot get a token silently, so redirect the user to be challenged 
            }
        }
        if (result == null)
        {
            result = await App.AcquireTokenWithDeviceCode(Scopes,
                deviceCodeResult => {
                    // This will print the message on the console which tells the user where to go sign-in using
                    // a separate browser and the code to enter once they sign in.
                    // The AcquireTokenWithDeviceCode() method will poll the server after firing this
                    // device code callback to look for the successful login of the user via that browser.
                    // This background polling (whose interval and timeout data is also provided as fields in the
                    // deviceCodeCallback class) will occur until:
                    // * The user has successfully logged in via browser and entered the proper code
                    // * The timeout specified by the server for the lifetime of this code (typically ~15 minutes) has been reached
                    // * The developing application calls the Cancel() method on a CancellationToken sent into the method.
                    //   If this occurs, an OperationCanceledException will be thrown (see catch below for more details).
                    Console.WriteLine(deviceCodeResult.Message);
                    return Task.FromResult(0);
                }).ExecuteAsync();

        }
        return (result.AccessToken);
    }

    static async Task<string> SendRequest(string modelType, string requestData)
    {
        var token = await GetToken();
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Content = new StringContent(requestData, Encoding.UTF8, "application/json");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-ModelType", modelType);

        var httpResponse = await httpClient.SendAsync(request);

        return (await httpResponse.Content.ReadAsStringAsync());
    }

    static async Task<string> SendStreamRequest(string modelType, string requestData)
    {
        var token = await GetToken();
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Content = new StringContent(requestData, Encoding.UTF8, "application/json");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-ModelType", modelType);

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
                    return result.Choices[0].Text ?? string.Empty;
                }
            }
        }

        return string.Empty;
    }


    private static async Task<MsalCacheHelper> CreateCacheHelperAsync()
    {
        StorageCreationProperties storageProperties;

        try
        {
            storageProperties = new StorageCreationPropertiesBuilder(
                    ".llmapi-token-cache.txt",
                    ".")
                .WithLinuxKeyring(
                    "com.microsoft.substrate.llmapi",
                    MsalCacheHelper.LinuxKeyRingDefaultCollection,
                    "MSAL token cache for LLM API",
                    new KeyValuePair<string, string>("Version", "1"),
                    new KeyValuePair<string, string>("ProductGroup", "LLMAPI"))
                .WithMacKeyChain(
                    "llmapi_msal_service",
                    "llmapi_msla_account")
                .Build();

            var cacheHelper = await MsalCacheHelper.CreateAsync(
                storageProperties).ConfigureAwait(false);

            cacheHelper.VerifyPersistence();
            return cacheHelper;

        }
        catch (MsalCachePersistenceException e)
        {
            Console.WriteLine($"WARNING! Unable to encrypt tokens at rest." +
                              $" Saving tokens in plaintext at {Path.Combine(".", ".llmapi-token-cache.txt")} ! Please protect this directory or delete the file after use");
            Console.WriteLine($"Encryption exception: " + e);

            storageProperties =
                new StorageCreationPropertiesBuilder(
                        ".llmapi-token-cache.txt" + ".plaintext", // do not use the same file name so as not to overwrite the encypted version
                        ".")
                    .WithUnprotectedFile()
                    .Build();

            var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);
            cacheHelper.VerifyPersistence();

            return cacheHelper;
        }
    }
}
