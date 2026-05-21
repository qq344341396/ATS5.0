namespace ATS5.Application.DeviceConfig
{
    public sealed class DeviceConfigOperationResult
    {
        private DeviceConfigOperationResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public bool IsSuccess { get; }

        public string Message { get; }

        public static DeviceConfigOperationResult Success(string message)
        {
            return new DeviceConfigOperationResult(true, message);
        }

        public static DeviceConfigOperationResult Failure(string message)
        {
            return new DeviceConfigOperationResult(false, message);
        }
    }
}
