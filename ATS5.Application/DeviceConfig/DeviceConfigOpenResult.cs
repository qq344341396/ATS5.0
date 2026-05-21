using System;
using System.Collections.Generic;

namespace ATS5.Application.DeviceConfig
{
    public sealed class DeviceConfigOpenResult
    {
        private DeviceConfigOpenResult(
            bool isSuccess,
            string configName,
            IReadOnlyList<DeviceConfigItem> devices,
            string originalJson,
            string message)
        {
            IsSuccess = isSuccess;
            ConfigName = configName;
            Devices = devices;
            OriginalJson = originalJson;
            Message = message;
        }

        public bool IsSuccess { get; }

        public string ConfigName { get; }

        public IReadOnlyList<DeviceConfigItem> Devices { get; }

        public string OriginalJson { get; }

        public string Message { get; }

        public static DeviceConfigOpenResult Success(
            string configName,
            IReadOnlyList<DeviceConfigItem> devices,
            string originalJson)
        {
            return new DeviceConfigOpenResult(
                true,
                configName ?? string.Empty,
                devices ?? Array.Empty<DeviceConfigItem>(),
                originalJson ?? string.Empty,
                string.Empty);
        }

        public static DeviceConfigOpenResult Failure(string message)
        {
            return new DeviceConfigOpenResult(
                false,
                string.Empty,
                Array.Empty<DeviceConfigItem>(),
                string.Empty,
                message ?? string.Empty);
        }
    }
}
