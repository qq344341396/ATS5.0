namespace ATS5.Application.Flow
{
    public sealed class FlowOpenResult
    {
        private FlowOpenResult(bool isSuccess, string flowName, ProcessFlowDefinition flow, string originalJson, string message)
        {
            IsSuccess = isSuccess;
            FlowName = flowName;
            Flow = flow;
            OriginalJson = originalJson;
            Message = message;
        }

        public bool IsSuccess { get; }

        public string FlowName { get; }

        public ProcessFlowDefinition Flow { get; }

        public string OriginalJson { get; }

        public string Message { get; }

        public static FlowOpenResult Success(string flowName, ProcessFlowDefinition flow, string originalJson)
        {
            return new FlowOpenResult(true, flowName, flow, originalJson, string.Empty);
        }

        public static FlowOpenResult Fail(string message)
        {
            return new FlowOpenResult(false, string.Empty, new ProcessFlowDefinition(), string.Empty, message);
        }
    }
}
