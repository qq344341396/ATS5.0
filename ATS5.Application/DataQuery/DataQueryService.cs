using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ATS5.Application.DataQuery
{
    public sealed class DataQueryService
    {
        private readonly ITestRecordRepository _testRecordRepository;

        public DataQueryService(ITestRecordRepository testRecordRepository)
        {
            _testRecordRepository = testRecordRepository ?? throw new ArgumentNullException(nameof(testRecordRepository));
        }

        public Task<DataQueryResult> QueryAsync(DataQueryFilter filter, int page, int pageSize, CancellationToken cancellationToken)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var validation = filter.Validate();
            if (!validation.IsValid)
            {
                return Task.FromResult(new DataQueryResult(false, validation.Message, 0, Array.Empty<TestRecord>()));
            }

            var criteria = filter.ToLegacyCriteria();
            var totalCount = _testRecordRepository.GetTotalCount(criteria);
            if (totalCount <= 0)
            {
                return Task.FromResult(new DataQueryResult(false, "未查询到相关数据", 0, Array.Empty<TestRecord>()));
            }

            var records = _testRecordRepository.GetRecords(criteria, page, pageSize);
            return Task.FromResult(new DataQueryResult(true, string.Empty, totalCount, records));
        }

        public Task<IReadOnlyList<TestDetailRecord>> GetDetailsAsync(TestRecord record, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_testRecordRepository.GetDetails(record));
        }
    }
}
