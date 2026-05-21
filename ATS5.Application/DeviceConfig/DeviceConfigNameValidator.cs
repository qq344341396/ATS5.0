using System;
using System.IO;

namespace ATS5.Application.DeviceConfig
{
    public static class DeviceConfigNameValidator
    {
        public const string InvalidConfigNameMessage = "设备配置类别无效";

        public static bool IsValidLeafName(string configName)
        {
            if (string.IsNullOrEmpty(configName))
            {
                return false;
            }

            if (configName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return false;
            }

            return !configName.Contains(Path.DirectorySeparatorChar.ToString())
                && !configName.Contains(Path.AltDirectorySeparatorChar.ToString())
                && !string.Equals(configName, ".")
                && !string.Equals(configName, "..");
        }

        public static void ThrowIfInvalidLeafName(string configName)
        {
            if (!IsValidLeafName(configName))
            {
                throw new ArgumentException(InvalidConfigNameMessage, nameof(configName));
            }
        }
    }
}
