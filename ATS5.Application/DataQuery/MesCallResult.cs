namespace ATS5.Application.DataQuery
{
    public sealed class MesCallResult
    {
        private MesCallResult(bool status, string message, bool isHandled)
        {
            Status = status;
            Message = message;
            IsHandled = isHandled;
        }

        public bool Status { get; }

        public string Message { get; }

        public bool IsHandled { get; }

        public static MesCallResult Success(string message = "")
        {
            return new MesCallResult(true, message, true);
        }

        public static MesCallResult Skipped()
        {
            return new MesCallResult(true, string.Empty, false);
        }

        public static MesCallResult Fail(string message)
        {
            return new MesCallResult(false, message, true);
        }
    }
}
