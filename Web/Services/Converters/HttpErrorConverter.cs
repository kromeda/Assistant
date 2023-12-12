using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Web.Http;

namespace Assistant.Web;

internal class HttpErrorConverter : IProblemDetailsConverter
{
    public ProblemDetails Convert(Stream stream)
    {
        if (Json.TryDeserialize(stream, out HttpError error) &&
            !string.IsNullOrEmpty(error.Message))
        {
            var problemDetails = new ProblemDetails
            {
                Title = error.Message,
                Detail = error.MessageDetail
            };

            if (error?.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in error)
                {
                    problemDetails.Extensions.Add(kvp.Key, kvp.Value);
                }
            }
        }

        stream.Position = 0;
        return null;
    }
}