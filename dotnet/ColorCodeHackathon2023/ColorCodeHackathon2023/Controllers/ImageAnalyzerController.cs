namespace ColorCodeHackathon2023.Controllers;

using Services;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
    [Authorize(AuthenticationSchemes = "ApiKey")]
    [Route("analyze")]
    public async Task<ActionResult<string>> AnalyzeImage([FromForm] IFormFile image)
    {

        if (!(Request.HasFormContentType && Request.Form.Files.Any()))
        {
            return new UnsupportedMediaTypeResult();
        }

        var result = await _imageAnalysisService.AnalyzeImageAsync(image);
        return Ok(result);
    }
}
