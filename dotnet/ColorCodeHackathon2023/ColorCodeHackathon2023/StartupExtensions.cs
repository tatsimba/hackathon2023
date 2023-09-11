namespace ColorCodeHackathon2023;

using Auth;
using Services;
using Settings;

public static class StartupExtensions
{
    public static void ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureAISettings>(configuration.GetSection(AzureAISettings.Key));
        services.Configure<OpenAISettings>(configuration.GetSection(OpenAISettings.Key));
    }

    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IImageAnalysisService, ImageAnalysisService>();
        services.AddSingleton<IGptService, GptService>();
        services.AddSingleton<IVisionService, VisionService>();
    }

    public static void ConfigureAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication("ApiKey")
            .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationSchemeHandler>(
                "ApiKey",
                opts => opts.ApiKey = configuration.GetValue<string>("ApiKey")
            );
    }

    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .Build();
            });
        });
        return services;
    }

}
