namespace ATS5.Application.Authority
{
    public sealed class AuthorityOperationResult
    {
        private AuthorityOperationResult(bool isSuccess, string message, string confirmationMessage)
        {
            IsSuccess = isSuccess;
            Message = message;
            ConfirmationMessage = confirmationMessage;
        }

        public bool IsSuccess { get; }

        public string Message { get; }

        public string ConfirmationMessage { get; }

        public static AuthorityOperationResult Success()
        {
            return new AuthorityOperationResult(true, string.Empty, string.Empty);
        }

        public static AuthorityOperationResult Failed(string message)
        {
            return new AuthorityOperationResult(false, message, string.Empty);
        }

        public static AuthorityOperationResult Cancelled(string confirmationMessage)
        {
            return new AuthorityOperationResult(false, string.Empty, confirmationMessage);
        }
    }
}
