using System.IO;
using System.Linq;
using ATS5.Infrastructure.LegacyAdapters.DeviceConfig;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using Xunit;

namespace ATS5.Tests.DeviceConfig
{
    [Trait("Category", "LegacyIntegration")]
    [Trait("Category", "DeviceConfigParity")]
    public sealed class LegacyDeviceConfigRepositoryTests
    {
        private const string LegacyRuntimeRoot = @"D:\CODE\ATE\01_code\ATS\bin\Debug";

        [Fact]
        public void Open_ReadsRealDebugDeviceConfigSample()
        {
            var repository = CreateRepository(LegacyRuntimeRoot);

            var result = repository.Open("测试");

            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal("测试", result.ConfigName);
            Assert.NotEmpty(result.Devices);
            var device = result.Devices.First();
            Assert.Equal("BMS", device.DevType);
            Assert.Equal("周立功【ZLG_USBCANII】", device.DevName);
            Assert.Equal("_bms_zlg_usbcanii", device.DevCode);
            Assert.Equal("CAN_BMS.Bms_ZLG_USBCANII", device.DevClasss);
        }

        [Fact]
        public void Write_UsesUtf8BomJsonCompatibleWithLegacyFields()
        {
            var tempRoot = CreateTempRuntimeRoot();
            var repository = CreateRepository(tempRoot);
            var source = CreateRepository(LegacyRuntimeRoot).Open("测试");
            Assert.True(source.IsSuccess, source.Message);

            repository.Write("测试副本", source.Devices);
            var writtenPath = Path.Combine(tempRoot, "SysCache", "DevCfg", "测试副本.dev");
            var bytes = File.ReadAllBytes(writtenPath);
            var roundTrip = repository.Open("测试副本");

            Assert.True(bytes.Length > 3);
            Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3).ToArray());
            Assert.True(roundTrip.IsSuccess, roundTrip.Message);
            Assert.Equal(source.Devices.Count, roundTrip.Devices.Count);
            Assert.Equal(source.Devices.First().DevCode, roundTrip.Devices.First().DevCode);
            Assert.Contains("\"DevType\"", File.ReadAllText(writtenPath));
            Assert.Contains("\"IsGlobal\"", File.ReadAllText(writtenPath));
        }

        [Fact]
        public void Open_ReturnsLegacyDamagedMessage_ForEmptyDeviceConfig()
        {
            var repository = CreateRepository(LegacyRuntimeRoot);

            var result = repository.Open("DeviceA");

            Assert.False(result.IsSuccess);
            Assert.Equal("设备配置文件已损坏，请重新选择！", result.Message);
        }

        [Fact]
        public void Write_RejectsPathTraversalConfigName()
        {
            var tempRoot = CreateTempRuntimeRoot();
            var repository = CreateRepository(tempRoot);
            var source = CreateRepository(LegacyRuntimeRoot).Open("测试");
            Assert.True(source.IsSuccess, source.Message);

            Assert.Throws<System.ArgumentException>(() => repository.Write(@"..\逃逸", source.Devices));
            Assert.False(File.Exists(Path.Combine(tempRoot, "逃逸.dev")));
        }

        private static LegacyDeviceConfigRepository CreateRepository(string runtimeRoot)
        {
            return new LegacyDeviceConfigRepository(new LegacyRuntimeContext(new DebugRuntimePathProvider(runtimeRoot)));
        }

        private static string CreateTempRuntimeRoot()
        {
            var root = Path.Combine(Path.GetTempPath(), "ATS5-WP07-" + Path.GetRandomFileName());
            Directory.CreateDirectory(Path.Combine(root, "SysCache", "DevCfg"));
            Directory.CreateDirectory(Path.Combine(root, "SysCache", "Devices"));
            return root;
        }
    }
}
