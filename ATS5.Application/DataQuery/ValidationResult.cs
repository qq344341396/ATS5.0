namespace ATS5.Application.DataQuery
{
    public sealed class ValidationResult
    {
        private ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }

        public bool IsValid { get; }

        public string Message { get; }

        public static ValidationResult Success()
        {
            return new ValidationResult(true, string.Empty);
        }

        public static ValidationResult Fail(string message)
        {
            return new ValidationResult(false, message);
        }
    }
}
