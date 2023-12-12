namespace Assistant.Web;

public abstract class HttpServiceBase
{
    private readonly IEnumerable<IProblemDetailsConverter> _converters;
    protected readonly HttpClient HttpClient;

    protected HttpServiceBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
        _converters = GetDefaultConverters();
    }

    protected HttpServiceBase(HttpClient httpClient, IEnumerable<IProblemDetailsConverter> converters)
    {
        HttpClient = httpClient;
        _converters = converters;
    }

    protected Task EnsureSuccessStatusCode(HttpResponseMessage response)
    {
        return EnsureSuccessStatusCode(response, CancellationToken.None);
    }

    protected async Task EnsureSuccessStatusCode(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var problemDetails = Convert(stream);
            problemDetails.Status = (int)response.StatusCode;

            throw new ProblemDetailsException(problemDetails);
        }
    }

    protected Task<T> GetAs<T>(string route)
    {
        return GetAs<T>(route, CancellationToken.None);
    }

    protected async Task<T> GetAs<T>(string route, CancellationToken cancellationToken)
    {
        var response = await HttpClient.GetAsync(route, cancellationToken);
        await EnsureSuccessStatusCode(response, cancellationToken);

        return await response.Content.ReadAsAsync<T>(cancellationToken);
    }

    private ProblemDetails Convert(Stream stream)
    {
        foreach (var converter in _converters)
        {
            if (converter.Convert(stream) is { } problemDetails)
            {
                return problemDetails;
            }
        }

        using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        var message = reader.ReadToEnd();

        return new ProblemDetails
        {
            Title = ErrorTexts.RelatedSerivce,
            Detail = message
        };
    }

    private IEnumerable<IProblemDetailsConverter> GetDefaultConverters()
    {
        var converterTypes = GetType().Assembly.DefinedTypes
            .Where(x => typeof(IProblemDetailsConverter).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

        return converterTypes
            .Select(Activator.CreateInstance)
            .Cast<IProblemDetailsConverter>();
    }
}
