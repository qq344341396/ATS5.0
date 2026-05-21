namespace ATS5.Application.Flow
{
    public sealed class FlowOperationResult
    {
        private FlowOperationResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public bool IsSuccess { get; }

        public string Message { get; }

        public static FlowOperationResult Success(string message)
        {
            return new FlowOperationResult(true, message);
        }

        public static FlowOperationResult Fail(string message)
        {
            return new FlowOperationResult(false, message);
        }
    }
}
