namespace VacationExplorer
{
    public class SkyscannerError
    {
        public string statusText { get; set; }
        public DebugInformation debugInformation { get; set; }
    }

    public class DebugInformation
    {
        public ValidationError[] ValidationErrors { get; set; }
    }

    public class ValidationError
    {
        public string Message { get; set; }
    }
}
