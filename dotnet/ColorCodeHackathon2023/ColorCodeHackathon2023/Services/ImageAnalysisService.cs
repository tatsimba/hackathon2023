namespace ColorCodeHackathon2023.Services;

using System.Text;
using Model;

public interface IImageAnalysisService
{
    Task<ImageAnalysisResult> AnalyzeImageAsync(string imageFile);
}

public class ImageAnalysisService : IImageAnalysisService
{
    private readonly IVisionService _visionService;
    private readonly IGptService _gptService;
    private static readonly string Garments = "pants, shirt, shoes, dress, hat, coat, jacket, scarf, shorts and belt";
    public ImageAnalysisService(IVisionService visionService, IGptService gptService)
    {
        _visionService = visionService;
        _gptService = gptService;
    }

    public async Task<ImageAnalysisResult> AnalyzeImageAsync(string imageFile)
    {
        // 1) get the dense captions
        var denseCaptions = _visionService.AnalyzeDenseCaptions(imageFile);
        //2) analyze with GPT
        var garmentColorTask = _gptService.RunPromptAsync(CreateGarmentColorPrompt(string.Join(Environment.NewLine, denseCaptions)));
        var weatherTask = _gptService.RunPromptAsync(CreateWeatherPrompt(string.Join(Environment.NewLine, denseCaptions)));
        var matchingColorTask = _gptService.RunPromptAsync(CreateMatchingColorPrompt(string.Join(Environment.NewLine, denseCaptions)));
        Task.WaitAll(garmentColorTask, weatherTask, matchingColorTask);
        return new ImageAnalysisResult {GarmentColorResult = garmentColorTask.Result, MatchingColorResult = matchingColorTask.Result, WeatherResult = weatherTask.Result, DenseCaptions = denseCaptions};
    }

    public string CreateGarmentColorPrompt(string denseCaptions)
    {
        //coat, 
        var builder = new StringBuilder();
        builder.AppendLine($"Given the captions: \"{denseCaptions}\"");
        builder.AppendLine("Note what the person in the image description is wearing.");
        builder.AppendLine($"Summarize what the person is wearing out of the following items: {Garments}"); 
        builder.AppendLine("Format the output as json using the following scheme: { \"garment name \": \"garment color\"}.");
        builder.AppendLine("In case the garment is not mentioned, don't add it to the response.");
        return builder.ToString();
    }

    public string CreateWeatherPrompt(string denseCaptions)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Given the captions: \"{denseCaptions}\"");
        builder.AppendLine("Note what the person in the image description is wearing.");
        builder.AppendLine("Does the clothes which the person is wearing matches a sunny day?");
        builder.AppendLine("Write the response in json format of tree fields:");
        builder.AppendLine("First field name is 'result' and explains in 1 short and concise sentence");
        builder.AppendLine("Second field name is 'matching' and contains a single word: 'matching' or 'non-matching'");
        builder.AppendLine($"Third filed name is 'non-matching-garments' and contains the garments that are not matching out of the following garments: {Garments}");
        return builder.ToString();
    }

    public string CreateMatchingColorPrompt(string denseCaptions)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Given the captions: \"{denseCaptions}\"");
        //builder.AppendLine("Act as an assistant to the color blind.");
        builder.AppendLine("Note what the person in the image description is wearing.");
        //builder.AppendLine("Make sure to pay extra attention to the style and color of the shirt, pants and shoes.");
        //builder.AppendLine("If any of the above items is missing then please ignore it.");
        builder.AppendLine("Does the clothes which the person is wearing fits well in terms of color?");
        builder.AppendLine("Write the response in json format of tree fields:");
        builder.AppendLine("First field name is 'result' and explains in 1 short and concise sentence");
        builder.AppendLine("Second field name is 'matching' and contains a single word: 'matching' or 'non-matching'");
        builder.AppendLine($"Third filed name is 'non-matching-garments' and contains the garments that are not matching out of the following garments: {Garments}");
        return builder.ToString();
    }
}
