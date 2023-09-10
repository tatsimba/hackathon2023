namespace ColorCodeHackathon2023.Controllers;

using Services;
using System.IO;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("image")]
public class ImageAnalyzerController : ControllerBase
{
    private readonly IImageAnalysisService _imageAnalysisService;
    private readonly ILogger<ImageAnalyzerController> _logger;
    
    public ImageAnalyzerController(IImageAnalysisService imageAnalysisService, ILogger<ImageAnalyzerController> logger)
    {
        _imageAnalysisService = imageAnalysisService;
        _logger = logger;
    }

    [HttpPost]
    [Route("analyze")]
    public async Task<ActionResult<string>> AnalyzeImage([FromForm] IFormFile image)
    {

        if (!(Request.HasFormContentType && Request.Form.Files.Any()))
        {
            return new UnsupportedMediaTypeResult();
        }

        //using var imageSource = VisionSource.FromUrl(
        //  new Uri("https://plus.unsplash.com/premium_photo-1673210886161-bfcc40f54d1f?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxzZWFyY2h8MXx8cGVyc29uJTIwc3RhbmRpbmd8ZW58MHx8MHx8&w=1000&q=80"));

        var tempFile = Path.GetTempFileName();
        Console.WriteLine("Using " + tempFile);
        await image.OpenReadStream().CopyToAsync(new FileStream(tempFile, FileMode.Open));
        var result = await _imageAnalysisService.AnalyzeImageAsync(tempFile);
        return Ok(result);
    }
}
