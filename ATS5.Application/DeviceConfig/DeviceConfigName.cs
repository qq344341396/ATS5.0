using System;

namespace ATS5.Application.DeviceConfig
{
    public sealed class DeviceConfigName
    {
        public DeviceConfigName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Device config name is required.", nameof(name));
            }

            Name = name;
        }

        public string Name { get; }
    }
}
