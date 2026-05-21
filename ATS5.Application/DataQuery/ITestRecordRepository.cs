using System.Collections.Generic;

namespace ATS5.Application.DataQuery
{
    public interface ITestRecordRepository
    {
        int GetTotalCount(DataQueryCriteria criteria);

        IReadOnlyList<TestRecord> GetRecords(DataQueryCriteria criteria, int page, int pageSize);

        IReadOnlyList<TestDetailRecord> GetDetails(TestRecord record);

        void UpdateIndexInfo(TestRecord record);
    }
}
