namespace ColorCodeHackathon2023;

using Services;
using Settings;

public static class StartupExtensions
{
    public static void ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureAISettings>(configuration.GetSection(AzureAISettings.Key));
    }

    public static void ConfigureImageAnalysisService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IImageAnalysisService, ImageAnalysisService>();
    }
}
