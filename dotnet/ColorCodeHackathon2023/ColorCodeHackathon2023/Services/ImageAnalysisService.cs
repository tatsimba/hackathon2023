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
        builder.AppendLine($"Image description:\"{denseCaptions}\"");
        builder.AppendLine("Summarize in 1 sentence what the person is wearing. Specify only out of the following items: ");
        builder.AppendLine("pants\n\nshirt\n\nshoes\n\ndress\n\nhat\n\nbelt");
        builder.AppendLine("Format the output as json using the following scheme: { \"pants\": \"pants color\"}. Do not specify cloths that the person is not wearing.");
        return builder.ToString();
    }
}
