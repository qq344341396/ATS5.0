using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS5.Application.DeviceConfig;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATSCommon;
using ATSCore;
using ATSModel;

namespace ATS5.Infrastructure.LegacyAdapters.DeviceConfig
{
    public sealed class LegacyDeviceConfigRepository : IDeviceConfigRepository
    {
        private const string DamagedDeviceConfigMessage = "设备配置文件已损坏，请重新选择！";

        private readonly LegacyRuntimeContext _runtimeContext;

        public LegacyDeviceConfigRepository(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        }

        public IReadOnlyList<DeviceConfigName> GetConfigNames()
        {
            return _runtimeContext.Execute(
                () =>
                {
                    var directory = new DirectoryInfo(SysCache.PathDevCfg);
                    if (!directory.Exists)
                    {
                        return Array.Empty<DeviceConfigName>();
                    }

                    return directory
                        .GetFiles("*.dev")
                        .Select(file => new DeviceConfigName(Path.GetFileNameWithoutExtension(file.Name)))
                        .Cast<DeviceConfigName>()
                        .ToArray();
                });
        }

        public DeviceConfigOpenResult Open(string configName)
        {
            if (!DeviceConfigNameValidator.IsValidLeafName(configName))
            {
                return DeviceConfigOpenResult.Failure(DeviceConfigNameValidator.InvalidConfigNameMessage);
            }

            return _runtimeContext.Execute(
                () =>
                {
                    var filePath = $"{SysCache.PathDevCfg}{configName}.dev";
                    if (File.Exists(filePath) && new FileInfo(filePath).Length == 0)
                    {
                        return DeviceConfigOpenResult.Failure(DamagedDeviceConfigMessage);
                    }

                    var json = JsonHelper.GetJsonFile(filePath);
                    var devices = JsonHelper.ToList<Device>(json);
                    if (devices == null)
                    {
                        return DeviceConfigOpenResult.Failure(DamagedDeviceConfigMessage);
                    }

                    return DeviceConfigOpenResult.Success(configName, devices.Select(MapDevice).ToArray(), json);
                });
        }

        public IReadOnlyList<DeviceLibraryItem> GetDeviceLibrary()
        {
            return _runtimeContext.Execute(() => DeviceCore.GetDevices().Select(MapLibraryItem).ToArray());
        }

        public string Serialize(IReadOnlyList<DeviceConfigItem> devices)
        {
            return JsonHelper.ObjToJson(devices.Select(MapDevice).ToArray());
        }

        public void Write(string configName, IReadOnlyList<DeviceConfigItem> devices)
        {
            DeviceConfigNameValidator.ThrowIfInvalidLeafName(configName);

            _runtimeContext.Execute(() => JsonHelper.WriteJsonFile(devices.Select(MapDevice).ToArray(), $"{SysCache.PathDevCfg}{configName}.dev"));
        }

        private static DeviceConfigItem MapDevice(Device device)
        {
            return new DeviceConfigItem
            {
                DevType = device.DevType,
                DevName = device.DevName,
                DevCode = device.DevCode,
                InitPars = device.InitPars,
                InitRemark = device.InitRemark,
                DevClasss = device.DevClasss,
                IsEnable = device.IsEnable,
                Remark = device.Remark,
                IsGlobal = device.IsGlobal
            };
        }

        private static Device MapDevice(DeviceConfigItem device)
        {
            return new Device
            {
                DevType = device.DevType,
                DevName = device.DevName,
                DevCode = device.DevCode,
                InitPars = device.InitPars,
                InitRemark = device.InitRemark,
                DevClasss = device.DevClasss,
                IsEnable = device.IsEnable,
                Remark = device.Remark,
                IsGlobal = device.IsGlobal
            };
        }

        private static DeviceLibraryItem MapLibraryItem(DeviceClass deviceClass)
        {
            return new DeviceLibraryItem
            {
                Id = deviceClass.ID,
                DevType = deviceClass.DevType,
                DevName = deviceClass.DevName,
                ClassName = deviceClass.ClassName,
                InitParExample = deviceClass.InitParExample,
                InitParRemark = deviceClass.InitParRemark
            };
        }
    }
}
