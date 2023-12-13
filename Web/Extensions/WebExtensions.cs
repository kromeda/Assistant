using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Web;

public static class WebExtensions
{
    public static IServiceCollection AddAssistExceptions(this IServiceCollection services)
    {
        return services
            .AddScoped<IProblemDetailsConverter, ProblemDetailsConverter>()
            .AddScoped<IProblemDetailsConverter, DsupErrorModelConverter>()
            .AddScoped<IProblemDetailsConverter, HttpErrorConverter>();
    }

    public static IApplicationBuilder UseAssistExceptionHandling(this IApplicationBuilder application)
    {
        return application
            .UseMiddleware<ExceptionMiddleware>();
    }
}