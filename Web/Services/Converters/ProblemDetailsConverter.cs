using Microsoft.AspNetCore.Mvc;

namespace Assistant.Web;

internal class ProblemDetailsConverter : IProblemDetailsConverter
{
    public ProblemDetails Convert(Stream stream)
    {
        if (Json.TryDeserialize(stream, out ProblemDetails problemDetails) &&
            !string.IsNullOrWhiteSpace(problemDetails.Title))
        {
            return problemDetails;
        }

        stream.Position = 0;
        return null;
    }
}