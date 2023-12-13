namespace Assistant.Web;

public interface IProblemDetailsConverter
{
    int Order { get; }

    bool IsEnabled { get; }

    ProblemDetails Convert(Stream stream);
}