namespace ColorCodeHackathon2023.Services;

using System.Linq;
using System.Text;
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
        return new ImageAnalysisResult {Prompt = prompt, Success = matchingColorResult.Item2, Result = matchingColorResult.Item1, DenseCaptions = denseCaptionsAll };
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
            .Concat(densePythonCaptionsUpperClothingTask.Result)
            .Concat(densePythonCaptionsUpperShoesTask.Result)
            .Concat(densePythonCaptionsUpperPantsTask.Result)
            .Distinct().ToList();
        return denseCaptionsAll;
    }

    private static string CreatePrompt(string denseCaptions)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Given the captions: \"{denseCaptions}\"");
        builder.AppendLine($"Note what the person in the captions is wearing out of the following garments list: \"{Garments}\".");
        builder.AppendLine("We would like to assess the following:");
        CreateMatchingColorPrompt(builder);
        CreateWeatherPrompt(builder);
        CreateGarmentColorPrompt(builder);
        builder.AppendLine("Provide the response in a json format using the following properties: resultGarmentsColors, resultWearing, matchingWearing, nonMatchingGarmentsWearing, resultWeather, matchingWeather, nonMatchingGarmentsWeather");
        return builder.ToString();
    }
    private static void CreateMatchingColorPrompt(StringBuilder builder)
    {
        builder.AppendLine($"Assessment 1 - Does the clothes which the person is wearing fit well in terms of color?");
        builder.AppendLine("\"resultWearing\": Should explain in 1 short and concise sentence result of assessment 1.");
        builder.AppendLine("\"matchingWearing\": Should contain either \"true\" or \"false\" bool value according to assessment 1.");
        builder.AppendLine($"\"nonMatchingGarmentsWearing\": Should contain the garments that are not matching in assessment 1.");
    }

    private static void CreateWeatherPrompt(StringBuilder builder)
    {
        builder.AppendLine("Assessment 2 - Does the clothes which the person is wearing matches a sunny day?");
        builder.AppendLine("\"resultWeather\": Should explain in 1 short and concise sentence result of assessment 2.");
        builder.AppendLine("\"matchingWeather\": Should contain either \"true\" or \"false\" bool value according to assessment 2.");
        builder.AppendLine($"\"nonMatchingGarmentsWeather\": should contain the garments that are not matching in assessment 2.");
    }

    private static void CreateGarmentColorPrompt(StringBuilder builder)
    {
        builder.AppendLine($"Assessment 3 - Summarize what are the color of the garments that the person is wearing?");
        builder.AppendLine("\"resultGarmentsColors\": Result of assessment 3 in the schema of { \"garment name \": \"garment color\"}.");
        builder.AppendLine("Not mentioned or undefined or unknown garments should not be included in the response");
    }
}
