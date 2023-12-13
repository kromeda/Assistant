namespace Assistant.Web
{
    internal static class ErrorTexts
    {
        internal const string Internal = "Внутренняя ошибка сервиса.";

        internal const string RelatedSerivce = "Возникла ошибка при обращении к сторонней системе.";

        internal const string TaskCanceled = "Время ожидания запроса истекло, либо запрос был отменен.";

        internal const string Validation = "Ошибка валидации.";

        internal const string RequestViolation = "Обнаружены нарушения требований к запросу.";
    }
}