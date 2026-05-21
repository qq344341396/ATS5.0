using System.Collections.Generic;

namespace ATS5.Application.DataQuery
{
    public sealed class ManualMesUploadResult
    {
        public ManualMesUploadResult(bool isSuccess, IReadOnlyList<string> messages)
        {
            IsSuccess = isSuccess;
            Messages = messages;
        }

        public bool IsSuccess { get; }

        public IReadOnlyList<string> Messages { get; }
    }
}
