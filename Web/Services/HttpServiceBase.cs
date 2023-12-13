namespace Assistant.Web;

public abstract class HttpServiceBase
{
    private static readonly ProblemDetailTypes _problemDetailTypes = new ProblemDetailTypes();
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
        _converters = converters
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.Order);
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
            var statusCode = (int)response.StatusCode;
            problemDetails.Status = statusCode;
            problemDetails.Type = _problemDetailTypes[statusCode];

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

        return await Json.ReadContent<T>(response.Content, cancellationToken);
    }

    protected Task PostAsJson<T>(string route, T contentValue)
    {
        return PostAsJson(route, contentValue, CancellationToken.None);
    }

    protected async Task PostAsJson<T>(string route, T contentValue, CancellationToken cancellationToken)
    {
        var response = await HttpClient.PostAsync(route, Json.GetContent(contentValue), cancellationToken);
        await EnsureSuccessStatusCode(response, cancellationToken);
    }

    protected Task PutAsJson<T>(string route, T contentValue)
    {
        return PutAsJson(route, contentValue, CancellationToken.None);
    }

    protected async Task PutAsJson<T>(string route, T contentValue, CancellationToken cancellationToken)
    {
        var response = await HttpClient.PutAsync(route, Json.GetContent(contentValue), cancellationToken);
        await EnsureSuccessStatusCode(response, cancellationToken);
    }

    protected Task DeleteAsJson<T>(string route, T contentValue)
    {
        return DeleteAsJson(route, contentValue, CancellationToken.None);
    }

    protected async Task DeleteAsJson<T>(string route, T contentValue, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, route);
        request.Content = Json.GetContent(contentValue);

        var response = await HttpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessStatusCode(response, cancellationToken);
    }

    protected Task Delete<T>(string route)
    {
        return Delete(route, CancellationToken.None);
    }

    protected async Task Delete(string route, CancellationToken cancellationToken)
    {
        var response = await HttpClient.DeleteAsync(route, cancellationToken);
        await EnsureSuccessStatusCode(response, cancellationToken);
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

    private static IEnumerable<IProblemDetailsConverter> GetDefaultConverters()
    {
        var assembly = typeof(HttpServiceBase).Assembly;
        var converterTypes = assembly.DefinedTypes
            .Where(x => typeof(IProblemDetailsConverter).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

        return converterTypes
            .Select(Activator.CreateInstance)
            .Cast<IProblemDetailsConverter>()
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.Order);
    }
}