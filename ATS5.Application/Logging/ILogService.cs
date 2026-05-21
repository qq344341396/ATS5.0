using System;

namespace ATS5.Application.Logging
{
    public interface ILogService
    {
        void Init();

        void Info(string message);

        void Error(string message, Exception exception);

        void Test(string message, string key = "");

        void Mes(string message);

        void Operate(string message);

        void CanTool(string message);
    }
}
