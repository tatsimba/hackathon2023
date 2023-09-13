namespace ColorCodeHackathon2023.Services;

using System.Linq;
using System.Text.Json;
using Model;

public interface IImageAnalysisService
{
    Task<ImageAnalysisResult> AnalyzeImageAsync(IFormFile image);
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

    public async Task<ImageAnalysisResult> AnalyzeImageAsync(IFormFile image)
    {
        // 1) get the dense captions
        var denseCaptionsAll = await GetDenseCaptions(image);

        //2) analyze with GPT
        var prompt = CreatePrompt(string.Join(", ", denseCaptionsAll));
        var matchingColorResult = await _gptService.RunPromptAsync(prompt);
        Console.WriteLine("GPT result: " + matchingColorResult.Item1);
        var success = matchingColorResult.Item2;
        var jsonForResult = "{}";
        if (success)
        {
            jsonForResult = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(matchingColorResult.Item1, new JsonSerializerOptions { AllowTrailingCommas = true }));
        }
        
        return new ImageAnalysisResult {Prompt = prompt, Success = success, Result = jsonForResult, DenseCaptions = denseCaptionsAll };
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
Jacket or coat or scarf fits cold weather.
However, Shorts and others fits hot weather.
Given the captions: ""{denseCaptions}"".
Focus on what the person in the captions is wearing and their colors out of the following garments list: ""{Garments}"".
Provide a json response according to the following format:
{{
    resultGarmentsColors: ""key value pairs of the garments and their colors, don't include garments without a color"",
    resultWearing: ""Describe what the person is wearing and make sure to include the colors of the garments"",
    matchingWearing: ""Boolean value indicating whether or not the color combination of the garments is commonly wore by most people"",
    nonMatchingGarmentsWearing: ""List of garments that don't match if matchingWearing is false"",
    resultWeather: ""Assess whether the person is dressed for a hot weather"",
    matchingWeather: ""Boolean value indicating whether the garments fit a hot weather."",
    nonMatchingGarmentsWeather: ""List of garments that don't match a very hot weather""
}}
Give me only the json without any additional output.
";
    }
}
