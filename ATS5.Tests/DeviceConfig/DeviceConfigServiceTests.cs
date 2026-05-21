using System;
using System.Collections.Generic;
using System.Linq;
using ATS5.Application.DeviceConfig;
using ATS5.Application.Logging;
using Xunit;

namespace ATS5.Tests.DeviceConfig
{
    public sealed class DeviceConfigServiceTests
    {
        [Fact]
        public void Save_ReturnsLegacyWarning_WhenEnabledDeviceCodesAreDuplicated()
        {
            var repository = new FakeDeviceConfigRepository();
            var service = CreateService(repository);
            var devices = new[]
            {
                CreateDevice("_bms", true),
                CreateDevice("_bms", true)
            };

            var result = service.Save("测试", devices, DeviceConfigSaveMode.Overwrite, originalJson: "[]");

            Assert.False(result.IsSuccess);
            Assert.Equal("存在多个相同设备编码【_bms】\r\n请检查修改！", result.Message);
            Assert.Empty(repository.Writes);
        }

        [Fact]
        public void Save_WritesConfig_WhenDeviceListIsValid()
        {
            var repository = new FakeDeviceConfigRepository();
            var service = CreateService(repository);

            var result = service.Save("测试副本", new[] { CreateDevice("_bms", true) }, DeviceConfigSaveMode.New, originalJson: string.Empty);

            Assert.True(result.IsSuccess);
            Assert.Equal("保存成功", result.Message);
            Assert.Equal(new[] { "测试副本" }, repository.Writes);
        }

        [Fact]
        public void Save_LogsOverwriteOnlyWhenJsonChanges()
        {
            var repository = new FakeDeviceConfigRepository
            {
                SerializedJson = "[{\"changed\":true}]"
            };
            var logService = new FakeLogService();
            var service = CreateService(repository, logService);

            var result = service.Save("测试", new[] { CreateDevice("_bms", true) }, DeviceConfigSaveMode.Overwrite, originalJson: "[]");

            Assert.True(result.IsSuccess);
            Assert.Equal("[管理员:admin]设备配置测试.dev修改，并保存覆盖", logService.OperateMessages.Single());
        }

        [Fact]
        public void SaveAs_WritesNewNameAndLogsLegacyMessage()
        {
            var repository = new FakeDeviceConfigRepository();
            var logService = new FakeLogService();
            var service = CreateService(repository, logService);

            var result = service.SaveAs("原配置", "新配置", new[] { CreateDevice("_bms", true) });

            Assert.True(result.IsSuccess);
            Assert.Equal("保存成功", result.Message);
            Assert.Equal(new[] { "新配置" }, repository.Writes);
            Assert.Equal("[管理员:admin]设备配置另存为，原配置：原配置.dev, 新配置：新配置.dev", logService.OperateMessages.Single());
        }

        [Fact]
        public void Save_RejectsPathTraversalConfigName_BeforeRepositoryWrite()
        {
            var repository = new FakeDeviceConfigRepository();
            var service = CreateService(repository);

            var result = service.Save(@"..\逃逸", new[] { CreateDevice("_bms", true) }, DeviceConfigSaveMode.New, originalJson: string.Empty);

            Assert.False(result.IsSuccess);
            Assert.Equal("设备配置类别无效", result.Message);
            Assert.Empty(repository.Writes);
        }

        [Fact]
        public void SaveAs_RejectsInvalidLeafConfigName_BeforeRepositoryWrite()
        {
            var repository = new FakeDeviceConfigRepository();
            var service = CreateService(repository);

            var result = service.SaveAs("原配置", "bad:name", new[] { CreateDevice("_bms", true) });

            Assert.False(result.IsSuccess);
            Assert.Equal("设备配置类别无效", result.Message);
            Assert.Empty(repository.Writes);
        }

        private static DeviceConfigService CreateService(
            FakeDeviceConfigRepository repository,
            FakeLogService? logService = null)
        {
            return new DeviceConfigService(repository, logService ?? new FakeLogService(), new FakeCurrentUserContext());
        }

        private static DeviceConfigItem CreateDevice(string devCode, bool isEnable)
        {
            return new DeviceConfigItem
            {
                DevType = "BMS",
                DevName = "周立功【ZLG_USBCANII】",
                DevCode = devCode,
                InitPars = "0,0,500",
                InitRemark = "【参数1】设备索引号\r\n",
                DevClasss = "CAN_BMS.Bms_ZLG_USBCANII",
                IsEnable = isEnable,
                Remark = null,
                IsGlobal = false
            };
        }

        private sealed class FakeDeviceConfigRepository : IDeviceConfigRepository
        {
            public string SerializedJson { get; set; } = "[{\"DevType\":\"BMS\"}]";

            public List<string> Writes { get; } = new List<string>();

            public IReadOnlyList<DeviceConfigName> GetConfigNames()
            {
                return Array.Empty<DeviceConfigName>();
            }

            public DeviceConfigOpenResult Open(string configName)
            {
                return DeviceConfigOpenResult.Success(configName, Array.Empty<DeviceConfigItem>(), "[]");
            }

            public IReadOnlyList<DeviceLibraryItem> GetDeviceLibrary()
            {
                return Array.Empty<DeviceLibraryItem>();
            }

            public string Serialize(IReadOnlyList<DeviceConfigItem> devices)
            {
                return SerializedJson;
            }

            public void Write(string configName, IReadOnlyList<DeviceConfigItem> devices)
            {
                Writes.Add(configName);
            }
        }

        private sealed class FakeLogService : ILogService
        {
            public List<string> OperateMessages { get; } = new List<string>();

            public void Init()
            {
            }

            public void Info(string message)
            {
            }

            public void Error(string message, Exception exception)
            {
            }

            public void Test(string message, string key = "")
            {
            }

            public void Mes(string message)
            {
            }

            public void Operate(string message)
            {
                OperateMessages.Add(message);
            }

            public void CanTool(string message)
            {
            }
        }

        private sealed class FakeCurrentUserContext : ICurrentUserContext
        {
            public string RoleName => "管理员";

            public string UserName => "admin";
        }
    }
}
