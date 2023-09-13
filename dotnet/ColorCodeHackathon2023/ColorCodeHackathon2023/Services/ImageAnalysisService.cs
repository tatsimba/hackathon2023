namespace ColorCodeHackathon2023.Services;

using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Model;

public interface IImageAnalysisService
{
    Task<ImageAnalysisResult> AnalyzeImageAsync(IFormFile image, bool useHardCodedWeatherResult = false);
}

public class GarmentInfo
{
    [JsonPropertyName("resultGarmentsColors")]
    public Dictionary<string, string> ResultGarmentsColors { get; set; }

    [JsonPropertyName("resultWearing")]
    public string ResultWearing { get; set; }

    [JsonPropertyName("matchingWearing")]
    public bool MatchingWearing { get; set; }

    [JsonPropertyName("nonMatchingGarmentsWearing")]
    public List<string> NonMatchingGarmentsWearing { get; set; }

    [JsonPropertyName("resultWeather")]
    public string ResultWeather { get; set; }

    [JsonPropertyName("matchingWeather")]
    public bool MatchingWeather { get; set; }

    [JsonPropertyName("nonMatchingGarmentsWeather")]
    public List<string> NonMatchingGarmentsWeather { get; set; }
}

public class ImageAnalysisService : IImageAnalysisService
{
    private readonly IVisionService _visionService;
    private readonly IVisionPythonService _visionPythonService;
    private readonly IGptService _gptService;
    private const string Garments = "pants, shorts, shirt, shoes, dress, coat, jacket and scarf";

    public ImageAnalysisService(IVisionService visionService, IGptService gptService, IVisionPythonService visionPythonService)
    {
        _visionService = visionService;
        _gptService = gptService;
        _visionPythonService = visionPythonService;
    }

    public async Task<ImageAnalysisResult> AnalyzeImageAsync(IFormFile image, bool useHardCodedWeatherResult)
    {
        // 1) get the dense captions
        var denseCaptionsAll = await GetDenseCaptions(image);

        var winterItems = new List<string> { "jacket", "scarf", "coat" };
        var denseCaptionsInOneSentence = string.Join(", ", denseCaptionsAll.Select(x => x.ToLower()));
        var winterItemsInCaptions = winterItems.Where(x => denseCaptionsInOneSentence.Contains(x)).ToList();
        var weatherMatchingSummary = "";
        if (winterItemsInCaptions.Any())
        {
            if (winterItemsInCaptions.Count == 1)
            {
                weatherMatchingSummary = $"You aren't dressed well for a sunny weather, I would suggest removing the {winterItemsInCaptions.Last()}";
            }
            else
            {
                weatherMatchingSummary = $"You aren't dressed well for a sunny weather, I would suggest removing the {string.Join(", ", winterItemsInCaptions.SkipLast(1))} and the {winterItemsInCaptions.Last()}";
            }
        }
        else
        {
            weatherMatchingSummary = "You are dressed well for a sunny weather";
        }

        //2) analyze with GPT
        var prompt = CreatePrompt(string.Join(", ", denseCaptionsAll));
        var matchingColorResult = await _gptService.RunPromptAsync(prompt);
        Console.WriteLine("GPT result: " + matchingColorResult.Item1);
        var success = matchingColorResult.Item2;
        GarmentInfo? result = null;
        if (success)
        {
            result = JsonSerializer.Deserialize<GarmentInfo>(matchingColorResult.Item1, new JsonSerializerOptions { AllowTrailingCommas = true });
            if (useHardCodedWeatherResult)
            {
                result.MatchingWeather = !winterItemsInCaptions.Any();
                result.ResultWeather = weatherMatchingSummary;
                result.NonMatchingGarmentsWeather = winterItemsInCaptions;
            }
        }

        return new ImageAnalysisResult { Prompt = prompt, Success = success, Result = JsonSerializer.Serialize(result), DenseCaptions = denseCaptionsAll };
    }

    private async Task<List<string>> GetDenseCaptions(IFormFile image)
    {
        var imageTempFile = Path.GetTempFileName();
        Console.WriteLine("Using " + imageTempFile);
        await image.OpenReadStream().CopyToAsync(new FileStream(imageTempFile, FileMode.Open));

        var denseCaptionsTask = _visionService.AnalyzeDenseCaptions(imageTempFile);
        var densePythonCaptionsUpperClothingTask =
            _visionPythonService.AnalyzeDenseCaptions(image, imageTempFile, "upper_clothing");
        var densePythonCaptionsUpperShoesTask = _visionPythonService.AnalyzeDenseCaptions(image, imageTempFile, "shoes");
        var densePythonCaptionsUpperPantsTask = _visionPythonService.AnalyzeDenseCaptions(image, imageTempFile, "pants");
        Task.WaitAll(denseCaptionsTask, densePythonCaptionsUpperClothingTask, densePythonCaptionsUpperShoesTask,
            densePythonCaptionsUpperPantsTask);
        var denseCaptionsAll = denseCaptionsTask.Result
            .Concat(densePythonCaptionsUpperClothingTask.Result.Item2 ? densePythonCaptionsUpperClothingTask.Result.Item1 : new List<string>())
            .Concat(densePythonCaptionsUpperShoesTask.Result.Item2 ? densePythonCaptionsUpperShoesTask.Result.Item1 : new List<string>())
            .Concat(densePythonCaptionsUpperPantsTask.Result.Item2 ? densePythonCaptionsUpperPantsTask.Result.Item1 : new List<string>())
            .Distinct().ToList();
        return denseCaptionsAll;
    }

    private static string CreatePrompt(string denseCaptions)
    {
        return @$"
Given the captions: ""{denseCaptions}"".
Focus on what the person in the captions is wearing and their colors out of the following garments list: ""{Garments}"".
Provide a json response according to the following format:
{{
    resultGarmentsColors: ""key value pairs of the garments and their colors, don't include garments without a color"",
    resultWearing: ""Describe what the person is wearing and make sure to include the colors of the garments"",
    matchingWearing: ""Boolean value indicating whether or not the color combination of the garments is commonly wore by most people"",
    nonMatchingGarmentsWearing: ""List of garments that don't match if matchingWearing is false"",
    resultWeather: ""Describe whether the person is dressed for a hot weather?"",
    matchingWeather: ""True whether the person is dressed for a hot weather, else false"",
    nonMatchingGarmentsWeather: ""List of garments that the person is dressed which doesn't fit a hot weather""
}}
Give me only the json without any additional output.
";
    }
}
