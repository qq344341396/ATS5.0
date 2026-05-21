namespace ATS5.Application.Flow
{
    public sealed class FlowScriptValidationResult
    {
        private FlowScriptValidationResult(bool isSuccess, string message, bool isUserMessageComplete)
        {
            IsSuccess = isSuccess;
            Message = message;
            IsUserMessageComplete = isUserMessageComplete;
        }

        public bool IsSuccess { get; }

        public string Message { get; }

        public bool IsUserMessageComplete { get; }

        public static FlowScriptValidationResult Success()
        {
            return new FlowScriptValidationResult(true, string.Empty, false);
        }

        public static FlowScriptValidationResult Fail(string message)
        {
            return new FlowScriptValidationResult(false, message, false);
        }

        public static FlowScriptValidationResult FailWithCompleteMessage(string message)
        {
            return new FlowScriptValidationResult(false, message, true);
        }
    }
}
