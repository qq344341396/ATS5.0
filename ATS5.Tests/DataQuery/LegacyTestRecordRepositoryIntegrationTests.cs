using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ATS5.Application.DataQuery;
using ATS5.Infrastructure.LegacyAdapters.DataQuery;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using Xunit;

namespace ATS5.Tests.DataQuery
{
    [Trait("Category", "LegacyIntegration")]
    [Trait("Category", "DataQueryParity")]
    public sealed class LegacyTestRecordRepositoryIntegrationTests
    {
        private const int IndexPage = 1;
        private const int IndexPageSize = 200;
        private const int DetailSearchPageSize = 2000;
        private const string LegacyRuntimeRootEnvironmentVariable = "ATS5_LEGACY_RUNTIME_ROOT";
        private const string DefaultSourceRuntimeRoot = @"D:\CODE\ATE\01_code\ATS\bin\Debug";
        private const string DetailProjectName = "VoltageProject";
        private const string DetailTestName = "VoltageCheck";
        private const string DetailVarName = "Voltage";
        private const string DetailDataType = "double";
        private const string DetailMaxLimit = "5.2";
        private const string DetailMinLimit = "4.8";
        private const string DetailComparisonOperator = ">=";
        private const string DetailPassResult = "PASS";
        private const string DetailTestValue = "5.01";
        private const string DetailUnit = "V";
        private const double DetailTestTime = 1.25;
        private const string FailedProjectName = "CommunicationProject";
        private const string FailedTestName = "HandshakeCheck";
        private const string FailedVarName = "Handshake";
        private const string FailedDataType = "bool";
        private const string FailedTestValue = "False";
        private const string FailedComparisonOperator = "==";
        private const string FailedResult = "FAIL";
        private const double FailedTestTime = 2.5;
        private static readonly DataQueryCriteria RealSqliteCriteria = new DataQueryCriteria(20251201000000UL, 20260430235959UL, string.Empty, 0, string.Empty);
        private static readonly DataQueryCriteria DetailSeedCriteria = new DataQueryCriteria(20260210000000UL, 20260211235959UL, string.Empty, 0, string.Empty);
        private static readonly string SourceRuntimeRoot = ResolveSourceRuntimeRoot();
        private static readonly string SourceDatabasePath = Path.Combine(SourceRuntimeRoot, "AppData", "ats.db");

        [Fact]
        public void Constructor_RejectsNullRuntimeContext()
        {
            Assert.Throws<ArgumentNullException>(() => new LegacyTestRecordRepository(null!));
        }

        [Fact]
        public void GetTotalCount_RejectsNullCriteria()
        {
            var repository = CreateRepository(SourceRuntimeRoot);

            Assert.Throws<ArgumentNullException>(() => repository.GetTotalCount(null!));
        }

        [Fact]
        public void GetRecords_RejectsNullCriteria()
        {
            var repository = CreateRepository(SourceRuntimeRoot);

            Assert.Throws<ArgumentNullException>(() => repository.GetRecords(null!, 1, 200));
        }

        [Theory]
        [InlineData(0, 200)]
        [InlineData(-1, 200)]
        [InlineData(1, 0)]
        [InlineData(1, -1)]
        public void GetRecords_RejectsInvalidPagination(int page, int pageSize)
        {
            var repository = CreateRepository(SourceRuntimeRoot);

            Assert.Throws<ArgumentOutOfRangeException>(() => repository.GetRecords(RealSqliteCriteria, page, pageSize));
        }

        [Fact]
        public void GetDetails_RejectsNullRecord()
        {
            var repository = CreateRepository(SourceRuntimeRoot);

            Assert.Throws<ArgumentNullException>(() => repository.GetDetails(null!));
        }

        [Fact]
        public void UpdateIndexInfo_RejectsNullRecord()
        {
            var repository = CreateRepository(SourceRuntimeRoot);

            Assert.Throws<ArgumentNullException>(() => repository.UpdateIndexInfo(null!));
        }

        [Fact]
        public void GetTotalCount_AndGetRecords_ReturnConsistentRealSqlitePage()
        {
            using (var runtime = CopiedDebugRuntime.Create())
            {
                var repository = CreateRepository(runtime.RuntimeRoot);

                var totalCount = repository.GetTotalCount(RealSqliteCriteria);
                var records = repository.GetRecords(RealSqliteCriteria, IndexPage, IndexPageSize);

                Assert.True(totalCount > 0, "真实 Debug ats.db 没有可查询的 IndexInfo 完成记录，不能证明兼容。");
                Assert.InRange(records.Count, 1, IndexPageSize);
                Assert.True(totalCount >= records.Count);
                Assert.All(records, record =>
                {
                    Assert.InRange(record.LogGuid, RealSqliteCriteria.StartLogGuid, RealSqliteCriteria.EndLogGuid);
                    Assert.False(string.IsNullOrEmpty(record.TestResult));
                });
                Assert.Equal(GetFileSha256(SourceDatabasePath), runtime.SourceDatabaseSha256);
            }
        }

        [Fact]
        public void GetDetails_MapsLegacyGzipDetailFields_FromSeededRuntimeCopy()
        {
            using (var runtime = CopiedDebugRuntime.Create())
            {
                var repository = CreateRepository(runtime.RuntimeRoot);

                var record = repository.GetRecords(DetailSeedCriteria, IndexPage, DetailSearchPageSize).FirstOrDefault();

                Assert.NotNull(record);
                WriteLegacyDetailFile(runtime.RuntimeRoot, record!);

                var details = repository.GetDetails(record!);
                var first = details.Single(item => item.ProjectIndex == 1);
                var second = details.Single(item => item.ProjectIndex == 2);

                Assert.Equal(2, details.Count);
                Assert.Equal(DetailProjectName, first.ProjectName);
                Assert.Equal(DetailTestName, first.TestName);
                Assert.Equal(DetailVarName, first.VarName);
                Assert.Equal(DetailDataType, first.DataType);
                Assert.Equal(DetailMaxLimit, first.MaxLimit);
                Assert.Equal(DetailMinLimit, first.MinLimit);
                Assert.Equal(DetailComparisonOperator, first.ComparisonOperator);
                Assert.Equal(DetailPassResult, first.TestResult);
                Assert.Equal(DetailPassResult, first.ProjectTestResult);
                Assert.Equal(DetailTestValue, first.TestValue);
                Assert.Equal(DetailUnit, first.Unit);
                Assert.Equal(DetailTestTime, first.TestTime);

                Assert.Equal(FailedProjectName, second.ProjectName);
                Assert.Equal(FailedTestName, second.TestName);
                Assert.Equal(FailedResult, second.TestResult);
                Assert.Equal(FailedResult, second.ProjectTestResult);
                Assert.Equal(GetFileSha256(SourceDatabasePath), runtime.SourceDatabaseSha256);
            }
        }

        private static LegacyTestRecordRepository CreateRepository(string runtimeRoot)
        {
            return new LegacyTestRecordRepository(
                new LegacyRuntimeContext(
                    new DebugRuntimePathProvider(runtimeRoot)));
        }

        private static string ResolveSourceRuntimeRoot()
        {
            var configuredRoot = Environment.GetEnvironmentVariable(LegacyRuntimeRootEnvironmentVariable);
            return string.IsNullOrWhiteSpace(configuredRoot)
                ? DefaultSourceRuntimeRoot
                : Path.GetFullPath(configuredRoot);
        }

        private static void WriteLegacyDetailFile(string runtimeRoot, TestRecord record)
        {
            var testTime = ParseLogGuidTime(record.LogGuid);
            var path = Path.Combine(
                runtimeRoot,
                "AppData",
                "TestData",
                testTime.Year.ToString(),
                testTime.Month.ToString(),
                testTime.Day.ToString(),
                $"{record.LogGuid}-CH{record.Channel}.json");

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            var json = CreateLegacyDetailJson(record);
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 81920))
            using (var compressStream = new GZipStream(fileStream, CompressionLevel.Optimal))
            using (var streamWriter = new StreamWriter(compressStream, Encoding.UTF8))
            {
                streamWriter.Write(json);
            }

            Assert.True(File.Exists(path), $"未生成 legacy 明细文件：{path}");
            Assert.True(new FileInfo(path).Length > 3, $"legacy 明细文件不可读：{path}");
        }

        private static DateTime ParseLogGuidTime(ulong logGuid)
        {
            return DateTime.ParseExact(logGuid.ToString("D14"), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }

        private static string CreateLegacyDetailJson(TestRecord record)
        {
            return "{" +
                "\"LstTestData\":[" +
                "{" +
                $"\"LogGuid\":{record.LogGuid}," +
                $"\"Channel\":{record.Channel}," +
                "\"ProjectIndex\":1," +
                $"\"TestName\":\"{DetailTestName}\"," +
                $"\"VarName\":\"{DetailVarName}\"," +
                $"\"DataType\":\"{DetailDataType}\"," +
                $"\"Unit\":\"{DetailUnit}\"," +
                $"\"MinLimit\":\"{DetailMinLimit}\"," +
                $"\"MaxLimit\":\"{DetailMaxLimit}\"," +
                $"\"TestValue\":\"{DetailTestValue}\"," +
                $"\"TestResult\":\"{DetailPassResult}\"," +
                $"\"ComparisonOperator\":\"{DetailComparisonOperator}\"" +
                "}," +
                "{" +
                $"\"LogGuid\":{record.LogGuid}," +
                $"\"Channel\":{record.Channel}," +
                "\"ProjectIndex\":2," +
                $"\"TestName\":\"{FailedTestName}\"," +
                $"\"VarName\":\"{FailedVarName}\"," +
                $"\"DataType\":\"{FailedDataType}\"," +
                "\"Unit\":\"\"," +
                "\"MinLimit\":\"\"," +
                "\"MaxLimit\":\"\"," +
                $"\"TestValue\":\"{FailedTestValue}\"," +
                $"\"TestResult\":\"{FailedResult}\"," +
                $"\"ComparisonOperator\":\"{FailedComparisonOperator}\"" +
                "}" +
                "]," +
                "\"LstTestProject\":[" +
                "{" +
                $"\"LogGuid\":{record.LogGuid}," +
                $"\"Channel\":{record.Channel}," +
                "\"ProjectIndex\":1," +
                $"\"ProjectName\":\"{DetailProjectName}\"," +
                $"\"TestTime\":{DetailTestTime.ToString(CultureInfo.InvariantCulture)}," +
                $"\"ProjectTestResult\":\"{DetailPassResult}\"" +
                "}," +
                "{" +
                $"\"LogGuid\":{record.LogGuid}," +
                $"\"Channel\":{record.Channel}," +
                "\"ProjectIndex\":2," +
                $"\"ProjectName\":\"{FailedProjectName}\"," +
                $"\"TestTime\":{FailedTestTime.ToString(CultureInfo.InvariantCulture)}," +
                $"\"ProjectTestResult\":\"{FailedResult}\"" +
                "}" +
                "]" +
                "}";
        }

        private static string GetFileSha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha256 = SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty);
            }
        }

        private sealed class CopiedDebugRuntime : IDisposable
        {
            private CopiedDebugRuntime(string runtimeRoot, string sourceDatabaseSha256)
            {
                RuntimeRoot = runtimeRoot;
                SourceDatabaseSha256 = sourceDatabaseSha256;
            }

            public string RuntimeRoot { get; }

            public string SourceDatabaseSha256 { get; }

            public static CopiedDebugRuntime Create()
            {
                Assert.True(File.Exists(SourceDatabasePath), $"真实 Debug ats.db 不存在：{SourceDatabasePath}");

                var sourceHash = GetFileSha256(SourceDatabasePath);
                var root = Path.Combine(Path.GetTempPath(), "ATS5.DataQuery." + Guid.NewGuid().ToString("N"));
                try
                {
                    CopyDirectory(SourceRuntimeRoot, root);
                    CopySqliteInterop(root);
                }
                catch
                {
                    DeleteRuntimeRoot(root);
                    throw;
                }

                return new CopiedDebugRuntime(root, sourceHash);
            }

            public void Dispose()
            {
                ClearSqlitePools();
                ResetAttributes(RuntimeRoot);

                try
                {
                    DeleteRuntimeRoot(RuntimeRoot);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    throw new InvalidOperationException($"未能清理测试运行目录：{RuntimeRoot}", ex);
                }
            }

            private static void DeleteRuntimeRoot(string runtimeRoot)
            {
                if (Directory.Exists(runtimeRoot))
                {
                    ResetAttributes(runtimeRoot);
                    Directory.Delete(runtimeRoot, true);
                }
            }

            private static void CopyDirectory(string sourceDirectory, string targetDirectory)
            {
                Directory.CreateDirectory(targetDirectory);

                foreach (var sourceFile in Directory.EnumerateFiles(sourceDirectory))
                {
                    var fileName = Path.GetFileName(sourceFile);
                    var targetFile = Path.Combine(targetDirectory, fileName);
                    if (!File.Exists(targetFile))
                    {
                        File.Copy(sourceFile, targetFile, false);
                    }
                }

                foreach (var sourceSubDirectory in Directory.EnumerateDirectories(sourceDirectory))
                {
                    var directoryName = Path.GetFileName(sourceSubDirectory);
                    CopyDirectory(sourceSubDirectory, Path.Combine(targetDirectory, directoryName));
                }
            }

            private static void CopySqliteInterop(string runtimeRoot)
            {
                var sqliteDirectory = Path.Combine(runtimeRoot, "AppDll", "sqlite");
                var testBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                CopyDirectoryIfMissing(Path.Combine(sqliteDirectory, "x64"), Path.Combine(testBaseDirectory, "x64"));
                CopyDirectoryIfMissing(Path.Combine(sqliteDirectory, "x86"), Path.Combine(testBaseDirectory, "x86"));
            }

            private static void ResetAttributes(string directory)
            {
                if (!Directory.Exists(directory))
                {
                    return;
                }

                foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                }

                foreach (var subDirectory in Directory.EnumerateDirectories(directory, "*", SearchOption.AllDirectories))
                {
                    File.SetAttributes(subDirectory, FileAttributes.Directory);
                }

                File.SetAttributes(directory, FileAttributes.Directory);
            }

            private static void CopyDirectoryIfMissing(string sourceDirectory, string targetDirectory)
            {
                if (Directory.Exists(targetDirectory))
                {
                    return;
                }

                CopyDirectory(sourceDirectory, targetDirectory);
            }

            private static void ClearSqlitePools()
            {
                var sqliteConnectionType = Type.GetType("System.Data.SQLite.SQLiteConnection, System.Data.SQLite");
                var clearAllPools = sqliteConnectionType?.GetMethod("ClearAllPools", BindingFlags.Public | BindingFlags.Static);
                clearAllPools?.Invoke(null, null);
            }
        }
    }
}
