using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using ATS5.Application.Authority;
using ATS5.Infrastructure.LegacyAdapters.Authority;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using Xunit;

namespace ATS5.Tests.Authority
{
    public sealed class LegacyAuthorityRepositoryTests
    {
        [Fact]
        public void Repository_ReadsAndWritesRoleAndUserData_FromTemporarySqliteRuntime()
        {
            using (var runtime = TemporaryAuthorityRuntime.Create())
            {
                var repository = CreateRepository(runtime.RuntimeRoot);

                var roleInsertResult = repository.InsertRole(new AuthorityRole
                {
                    RoleName = "工艺",
                    Remark = "流程",
                    Powers = "[\"1003\",\"1005\"]"
                });
                var role = repository.GetRoles().Single(item => item.RoleName == "工艺");
                var userInsertResult = repository.InsertUser(new AuthorityUser
                {
                    RID = role.ID,
                    UserName = "alice",
                    PW = "p@ss"
                });

                var users = repository.GetUsers(role.ID);

                Assert.Equal(1, roleInsertResult);
                Assert.Equal(1, userInsertResult);
                Assert.Equal("[\"1003\",\"1005\"]", role.Powers);
                Assert.Equal("流程", role.Remark);
                Assert.Equal("alice", users.Single().UserName);
                Assert.Equal("p@ss", users.Single().PW);
            }
        }

        [Fact]
        public void DeleteRole_DeletesUsersForThatRole_FromTemporarySqliteRuntime()
        {
            using (var runtime = TemporaryAuthorityRuntime.Create())
            {
                var repository = CreateRepository(runtime.RuntimeRoot);
                var firstRoleId = InsertRoleAndUser(repository, "管理员", "admin");
                var secondRoleId = InsertRoleAndUser(repository, "操作员", "operator");

                var result = repository.DeleteRole(firstRoleId);

                Assert.True(result > 0);
                Assert.DoesNotContain(repository.GetRoles(), role => role.ID == firstRoleId);
                Assert.Empty(repository.GetUsers(firstRoleId));
                Assert.Single(repository.GetUsers(secondRoleId));
            }
        }

        private static int InsertRoleAndUser(IAuthorityRepository repository, string roleName, string userName)
        {
            repository.InsertRole(new AuthorityRole { RoleName = roleName, Powers = "[]", Remark = string.Empty });
            var roleId = repository.GetRoles().Single(role => role.RoleName == roleName).ID;
            repository.InsertUser(new AuthorityUser { RID = roleId, UserName = userName, PW = "pw" });
            return roleId;
        }

        private static LegacyAuthorityRepository CreateRepository(string runtimeRoot)
        {
            return new LegacyAuthorityRepository(
                new LegacyRuntimeContext(
                    new DebugRuntimePathProvider(runtimeRoot)));
        }

        private sealed class TemporaryAuthorityRuntime : IDisposable
        {
            private TemporaryAuthorityRuntime(string runtimeRoot)
            {
                RuntimeRoot = runtimeRoot;
            }

            public string RuntimeRoot { get; }

            public static TemporaryAuthorityRuntime Create()
            {
                var root = Path.Combine(Path.GetTempPath(), "ATS5.Authority." + Guid.NewGuid().ToString("N"));
                var appData = Path.Combine(root, "AppData");
                Directory.CreateDirectory(appData);
                CreateDatabase(Path.Combine(appData, "ats.db"));
                return new TemporaryAuthorityRuntime(root);
            }

            public void Dispose()
            {
                SQLiteConnection.ClearAllPools();
                if (Directory.Exists(RuntimeRoot))
                {
                    DeleteDirectoryWithRetry(RuntimeRoot);
                }
            }

            private static void DeleteDirectoryWithRetry(string directory)
            {
                for (var attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
                        Directory.Delete(directory, true);
                        return;
                    }
                    catch (IOException) when (attempt < 4)
                    {
                        SQLiteConnection.ClearAllPools();
                        Thread.Sleep(100);
                    }
                }
            }

            private static void CreateDatabase(string databasePath)
            {
                SQLiteConnection.CreateFile(databasePath);
                using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText =
                            "create table Role(ID integer primary key autoincrement, RoleName text, Powers text, Remark text);" +
                            "create table User(ID integer primary key autoincrement, RID integer, UserName text, PW text);";
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
