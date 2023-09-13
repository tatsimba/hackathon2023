namespace ColorCodeHackathon2023.Services;

using System.Diagnostics;
using System.Text.Json;
using Model.VisionPython;

public interface IVisionPythonService
{
    Task<(List<string>, bool)> AnalyzeDenseCaptions(IFormFile image, string imagePath, string bodyPart);
}

public class VisionPythonService : IVisionPythonService
{
    //private const string DenseCaptionPath = "https://colorecode-python.azurewebsites.net/captions";
    private const string DenseCaptionPath = "https://colorecode-python.azurewebsites.net/captions/{0}";

    public async Task<(List<string>, bool)> AnalyzeDenseCaptions(IFormFile image, string imagePath, string bodyPart)
    {
        var imageTempFile = Path.GetTempFileName();
        File.Copy(imagePath, imageTempFile, true);
        Console.WriteLine($"Using for vision python: body part {bodyPart} and imageTempFile {imageTempFile}");
        var fileStream = new FileStream(imageTempFile, FileMode.Open);

        var httpClient = new HttpClient();
        using var formData = new MultipartFormDataContent
        {
            { new StreamContent(fileStream), "image", imageTempFile }
        };

        var watch = new Stopwatch();
        watch.Start();
        using var httpResponse = await httpClient.PostAsync(string.Format(DenseCaptionPath, bodyPart), formData);
        watch.Stop();
        Console.WriteLine($"Vision python took {watch.ElapsedMilliseconds} ms");

        var responseContent = httpResponse.Content is not null
        ? await httpResponse.Content.ReadAsStringAsync()
        : string.Empty;

        if (!httpResponse?.IsSuccessStatusCode ?? true)
        {
            Console.WriteLine($"Failed to query vision python - {responseContent}");
            return (new List<string>() { $"Failed to query vision python - {responseContent}" }, false);

        }

        var promptFilterResult = JsonSerializer.Deserialize<VisionPythonResponse>(responseContent);

        if (promptFilterResult?.Captions == null) return (new List<string>(), false);

        Console.WriteLine(" Python Dense Captions:");
        foreach (var caption in promptFilterResult.Captions)
        {
            Console.WriteLine($"{caption}");
        }

        return (promptFilterResult.Captions.Where(s => s.StartsWith("in 4 word") is false).ToList(), true);
    }
}
