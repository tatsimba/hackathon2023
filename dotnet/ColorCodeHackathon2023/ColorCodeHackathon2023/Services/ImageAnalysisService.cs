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
        var result = await _gptService.RunPromptAsync(CreateFirstStepDescribePrompt(string.Join(Environment.NewLine, denseCaptions)));
        return new ImageAnalysisResult {Result = result, DenseCaptions = denseCaptions};
    }

    public string CreateFirstStepDescribePrompt(string denseCaptions)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Given the captions: \"{denseCaptions}\"");
        builder.AppendLine("Summarize what the person is wearing out of the following items: pants, shirt, shoes, dress, hat and belt"); 
        builder.AppendLine("Format the output as json using the following scheme: { \"item name \": \"item color\"}. ");
        return builder.ToString();
    }
}
