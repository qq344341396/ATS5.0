using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS5.Application.DataQuery;
using ATSCommon;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATSCore.DBCore;
using ATSModel;
using ATSModel.DBModel;

namespace ATS5.Infrastructure.LegacyAdapters.DataQuery
{
    public sealed class LegacyTestRecordRepository : ITestRecordRepository
    {
        private readonly LegacyRuntimeContext _runtimeContext;

        public LegacyTestRecordRepository(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        }

        public int GetTotalCount(DataQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            return ExecuteDataQuery(() => IndexInfoCore.GetTotalCount(criteria.StartLogGuid, criteria.EndLogGuid, criteria.Barcode, criteria.Channel, criteria.FlowName));
        }

        public IReadOnlyList<TestRecord> GetRecords(DataQueryCriteria criteria, int page, int pageSize)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            if (page < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            return ExecuteDataQuery(() => IndexInfoCore.GetIndexInfo(criteria.StartLogGuid, criteria.EndLogGuid, criteria.Barcode, criteria.Channel, criteria.FlowName, page, pageSize)
                .Select(ToRecord)
                .ToList());
        }

        public IReadOnlyList<TestDetailRecord> GetDetails(TestRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            var indexInfo = ToIndexInfo(record);
            return ExecuteDataQuery(() => TestProjectDataCore.GetTestProjectDatas(indexInfo)
                .Select(ToDetailRecord)
                .ToList());
        }

        public void UpdateIndexInfo(TestRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            ExecuteDataQuery(() => IndexInfoCore.UpdateIndexInfo(ToIndexInfo(record)));
        }

        private T ExecuteDataQuery<T>(Func<T> action)
        {
            return _runtimeContext.Execute(
                () =>
                {
                    SQLiteHelper.ChangeConnection(_runtimeContext.GetLegacyPath(Path.Combine("AppData", "ats.db")));
                    return action();
                });
        }

        private void ExecuteDataQuery(Action action)
        {
            ExecuteDataQuery(
                () =>
                {
                    action();
                    return true;
                });
        }

        private static TestRecord ToRecord(IndexInfo source)
        {
            return new TestRecord
            {
                LogGuid = source.LogGuid,
                Channel = source.Channel,
                Barcode = source.Barcode ?? string.Empty,
                ProcessName = source.ProcessName ?? string.Empty,
                ProcessInfo = source.ProcessInfo ?? string.Empty,
                TestStartTime = source.TestStartTime,
                TestEndTime = source.TestEndTime,
                TakeTime = source.TakeTime,
                UploadMesStatus = source.UploadMesStatus,
                UploadFtpStatus = source.UploadFtpStatus,
                TestResult = source.TestResult ?? string.Empty,
                CreateTime = source.CreateTime,
                Reserve1 = source.Reserve1 ?? string.Empty,
                Reserve2 = source.Reserve2 ?? string.Empty
            };
        }

        private static IndexInfo ToIndexInfo(TestRecord source)
        {
            return new IndexInfo
            {
                LogGuid = source.LogGuid,
                Channel = source.Channel,
                Barcode = source.Barcode,
                ProcessName = source.ProcessName,
                ProcessInfo = source.ProcessInfo,
                TestStartTime = source.TestStartTime,
                TestEndTime = source.TestEndTime,
                TakeTime = source.TakeTime,
                UploadMesStatus = source.UploadMesStatus,
                UploadFtpStatus = source.UploadFtpStatus,
                TestResult = source.TestResult,
                CreateTime = source.CreateTime,
                Reserve1 = source.Reserve1,
                Reserve2 = source.Reserve2
            };
        }

        private static TestDetailRecord ToDetailRecord(TestProjectData source)
        {
            return new TestDetailRecord
            {
                TestName = source.TestName ?? string.Empty,
                VarName = source.VarName ?? string.Empty,
                DataType = source.DataType ?? string.Empty,
                MaxLimit = source.MaxLimit ?? string.Empty,
                MinLimit = source.MinLimit ?? string.Empty,
                ComparisonOperator = source.ComparisonOperator ?? string.Empty,
                ProjectIndex = source.ProjectIndex,
                TestResult = source.TestResult ?? string.Empty,
                ProjectTestResult = source.ProjectTestResult ?? string.Empty,
                TestValue = source.TestValue ?? string.Empty,
                Unit = source.Unit ?? string.Empty,
                ProjectName = source.ProjectName ?? string.Empty,
                TestTime = source.TestTime
            };
        }
    }
}
