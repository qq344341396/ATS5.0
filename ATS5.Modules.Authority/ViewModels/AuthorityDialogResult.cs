namespace ATS5.Modules.Authority.ViewModels
{
    public sealed class AuthorityDialogResult<T>
    {
        public AuthorityDialogResult(bool isOk, T value)
        {
            IsOk = isOk;
            Value = value;
        }

        public bool IsOk { get; }

        public T Value { get; }
    }
}
