namespace ColorCodeHackathon2023.Services;


public interface IImageAnalysisService
{
    Task<string> AnalyzeImageAsync(string imageFile);
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

    public async Task<string> AnalyzeImageAsync(string imageFile)
    {
        // 1) get the dense captions
        var denseCaptions = _visionService.AnalyzeDenseCaptions(imageFile);
        //2) analyze with GPT
        var promptWearingSummary = $"Please summarize in one sentence what is this person wearing? \"{denseCaptions}\"";
        var result = await _gptService.RunPromptAsync(promptWearingSummary);
        return result;
    }
}
