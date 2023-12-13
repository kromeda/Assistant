using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Assistant.Web
{
    public interface IProblemDetailsConverter
    {
        int Order { get; }

        bool IsEnabled { get; }

        ProblemDetails Convert(Stream stream);
    }
}