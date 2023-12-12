using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text;

namespace Assistant.Web;

public abstract class HttpServiceBase
{
    private readonly IEnumerable<IProblemDetailsConverter> _converters;

    protected HttpServiceBase(IEnumerable<IProblemDetailsConverter> converters)
    {
        _converters = converters;
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
}
