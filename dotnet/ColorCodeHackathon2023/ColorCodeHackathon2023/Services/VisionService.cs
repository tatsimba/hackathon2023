namespace ColorCodeHackathon2023.Services;

using Azure.AI.Vision.Common;
using Azure.AI.Vision.ImageAnalysis;
using Azure;
using Microsoft.Extensions.Options;
using Settings;
using System.Text;

public interface IVisionService
{
    List<string> AnalyzeDenseCaptions(string imageFile);
}

public class VisionService : IVisionService
{
    private static readonly ImageAnalysisOptions ImageAnalysisOptions = new()
    {
        Features = ImageAnalysisFeature.Caption
                   | ImageAnalysisFeature.DenseCaptions,
        Language = "en",
        GenderNeutralCaption = false
    };

    private readonly VisionServiceOptions _visionServiceOptions;

    public VisionService(IOptions<AzureAISettings> azureAiSettings)
    {
        var endpoint = azureAiSettings.Value.Endpoint;
        var apiKey = azureAiSettings.Value.ApiKey;
        _visionServiceOptions = new VisionServiceOptions(endpoint, new AzureKeyCredential(apiKey));
    }

    public List<string> AnalyzeDenseCaptions(string imageFile)
    {
        var visionSource = VisionSource.FromFile(imageFile);
        using var analyzer = new ImageAnalyzer(_visionServiceOptions, visionSource, ImageAnalysisOptions);
        var imageAnalysisResult = analyzer.Analyze();
        var result = new List<string>();

        if (imageAnalysisResult.Reason == ImageAnalysisResultReason.Analyzed)
        {
            if (imageAnalysisResult.Caption != null)
            {
                Console.WriteLine(" Caption:");
                Console.WriteLine($"   \"{imageAnalysisResult.Caption.Content}\", Confidence {imageAnalysisResult.Caption.Confidence:0.0000}");
            }

            if (imageAnalysisResult.DenseCaptions != null)
            {
                Console.WriteLine(" Dense Captions:");
                foreach (var caption in imageAnalysisResult.DenseCaptions)
                {
                    Console.WriteLine($"   \"{caption.Content}\", Bounding box {caption.BoundingBox}, Confidence {caption.Confidence:0.0000}");
                    result.Add(caption.Content);
                }
            }
        }
        else
        {
            var errorDetails = ImageAnalysisErrorDetails.FromResult(imageAnalysisResult);
            throw new Exception($"$Analysis failed, Error reason : {errorDetails.Reason}, Error code : {errorDetails.ErrorCode}, Error message: {errorDetails.Message}");
        }

        return result;
    }
}
