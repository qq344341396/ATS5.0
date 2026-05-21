using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ATS5.Application.Logging;

namespace ATS5.Application.DeviceConfig
{
    public sealed class DeviceConfigService
    {
        private const string EmptyDeviceConfigMessage = "无设备配置信息，无法保存";
        private const string SaveSuccessMessage = "保存成功";

        private readonly IDeviceConfigRepository _repository;
        private readonly ILogService _logService;
        private readonly ICurrentUserContext _currentUserContext;

        public DeviceConfigService(
            IDeviceConfigRepository repository,
            ILogService logService,
            ICurrentUserContext currentUserContext)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
        }

        public IReadOnlyList<DeviceConfigName> GetConfigNames()
        {
            return _repository.GetConfigNames();
        }

        public IReadOnlyList<DeviceLibraryItem> GetDeviceLibrary()
        {
            return _repository.GetDeviceLibrary();
        }

        public DeviceConfigOpenResult Open(string configName)
        {
            if (!DeviceConfigNameValidator.IsValidLeafName(configName))
            {
                return DeviceConfigOpenResult.Failure(DeviceConfigNameValidator.InvalidConfigNameMessage);
            }

            return _repository.Open(configName);
        }

        public string Serialize(IReadOnlyList<DeviceConfigItem> devices)
        {
            return _repository.Serialize(devices);
        }

        public DeviceConfigOperationResult Save(
            string configName,
            IReadOnlyList<DeviceConfigItem> devices,
            DeviceConfigSaveMode saveMode,
            string originalJson)
        {
            var validation = Validate(devices);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            var nameValidation = ValidateConfigName(configName);
            if (!nameValidation.IsSuccess)
            {
                return nameValidation;
            }

            _repository.Write(configName, devices);
            if (saveMode == DeviceConfigSaveMode.Overwrite && _repository.Serialize(devices) != (originalJson ?? string.Empty))
            {
                _logService.Operate($"[{_currentUserContext.RoleName}:{_currentUserContext.UserName}]设备配置{configName}.dev修改，并保存覆盖");
            }

            return DeviceConfigOperationResult.Success(SaveSuccessMessage);
        }

        public DeviceConfigOperationResult SaveAs(
            string oldConfigName,
            string newConfigName,
            IReadOnlyList<DeviceConfigItem> devices)
        {
            var validation = Validate(devices);
            if (!validation.IsSuccess)
            {
                return validation;
            }

            var nameValidation = ValidateConfigName(newConfigName);
            if (!nameValidation.IsSuccess)
            {
                return nameValidation;
            }

            _repository.Write(newConfigName, devices);
            _logService.Operate($"[{_currentUserContext.RoleName}:{_currentUserContext.UserName}]设备配置另存为，原配置：{oldConfigName}.dev, 新配置：{newConfigName}.dev");
            return DeviceConfigOperationResult.Success(SaveSuccessMessage);
        }

        private static DeviceConfigOperationResult ValidateConfigName(string configName)
        {
            return DeviceConfigNameValidator.IsValidLeafName(configName)
                ? DeviceConfigOperationResult.Success(string.Empty)
                : DeviceConfigOperationResult.Failure(DeviceConfigNameValidator.InvalidConfigNameMessage);
        }

        private static DeviceConfigOperationResult Validate(IReadOnlyList<DeviceConfigItem> devices)
        {
            if (devices == null || devices.Count == 0)
            {
                return DeviceConfigOperationResult.Failure(EmptyDeviceConfigMessage);
            }

            var duplicateCodes = devices
                .Where(device => device.IsEnable)
                .GroupBy(device => device.DevCode)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateCodes.Count > 0)
            {
                var builder = new StringBuilder();
                foreach (var code in duplicateCodes)
                {
                    builder.AppendFormat("存在多个相同设备编码【{0}】\r\n", code);
                }

                builder.Append("请检查修改！");
                return DeviceConfigOperationResult.Failure(builder.ToString());
            }

            return DeviceConfigOperationResult.Success(string.Empty);
        }
    }
}
