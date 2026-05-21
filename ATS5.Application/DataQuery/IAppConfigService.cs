namespace ATS5.Application.DataQuery
{
    public interface IAppConfigService
    {
        string GetValue(string key);

        bool SetValue(string key, string value);
    }
}
