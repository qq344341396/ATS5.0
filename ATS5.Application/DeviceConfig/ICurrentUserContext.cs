namespace ATS5.Application.DeviceConfig
{
    public interface ICurrentUserContext
    {
        string RoleName { get; }

        string UserName { get; }
    }
}
