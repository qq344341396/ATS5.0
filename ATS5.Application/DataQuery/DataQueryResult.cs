using System.Collections.Generic;

namespace ATS5.Application.DataQuery
{
    public sealed class DataQueryResult
    {
        public DataQueryResult(bool isSuccess, string message, int totalCount, IReadOnlyList<TestRecord> records)
        {
            IsSuccess = isSuccess;
            Message = message;
            TotalCount = totalCount;
            Records = records;
        }

        public bool IsSuccess { get; }

        public string Message { get; }

        public int TotalCount { get; }

        public IReadOnlyList<TestRecord> Records { get; }
    }
}
