using System;
using System.Collections.Generic;
using System.Linq;
using ATS5.Application.DeviceConfig;
using ATS5.Application.Logging;
using ATS5.Modules.Device.ViewModels;
using ATS5.Wpf.Core;
using Xunit;

namespace ATS5.Tests.DeviceConfig
{
    public sealed class DeviceManagerViewModelTests
    {
        [Fact]
        public void DefaultMode_KeepsLegacyCommandStates()
        {
            var viewModel = CreateViewModel();

            Assert.True(viewModel.NewCommand.CanExecute());
            Assert.True(viewModel.OpenCommand.CanExecute());
            Assert.False(viewModel.SaveCommand.CanExecute());
            Assert.False(viewModel.SaveAsCommand.CanExecute());
            Assert.False(viewModel.CancelCommand.CanExecute());
            Assert.False(viewModel.MoveUpCommand.CanExecute());
            Assert.False(viewModel.MoveDownCommand.CanExecute());
            Assert.False(viewModel.MoveTopCommand.CanExecute());
            Assert.False(viewModel.DeleteCommand.CanExecute());
            Assert.True(viewModel.IsGridReadOnly);
        }

        [Fact]
        public void NewThenCancel_ClearsConfigNameAndDeviceRows()
        {
            var dialogService = new FakeDeviceConfigDialogService { ConfirmResult = true };
            var viewModel = CreateViewModel(dialogService: dialogService);

            viewModel.NewCommand.Execute();
            viewModel.DeviceConfigs.Add(CreateDevice("_bms", true));
            viewModel.CancelCommand.Execute();

            Assert.Equal(string.Empty, viewModel.ConfigName);
            Assert.Empty(viewModel.DeviceConfigs);
            Assert.True(viewModel.IsGridReadOnly);
        }

        [Fact]
        public void Open_LoadsConfigNameTitleRowsAndSelectedInitRemark()
        {
            var repository = new FakeDeviceConfigRepository();
            var dialogService = new FakeDeviceConfigDialogService { SelectedConfigName = "测试" };
            var viewModel = CreateViewModel(repository, dialogService);

            viewModel.OpenCommand.Execute();

            Assert.Equal("测试", viewModel.ConfigName);
            Assert.Equal("【设备配置：测试】", viewModel.Title);
            Assert.Single(viewModel.DeviceConfigs);
            Assert.Equal("【参数1】设备索引号\r\n", viewModel.InitRemark);
        }

        [Fact]
        public void AddDeviceFromLibrary_UsesLegacyDefaultValues()
        {
            var viewModel = CreateViewModel();
            var libraryItem = new DeviceLibraryItem
            {
                DevType = "BMS",
                DevName = "周立功【ZLG_USBCANII】",
                ClassName = "CAN_BMS.Bms_ZLG_USBCANII",
                InitParExample = "0,0,500",
                InitParRemark = "【参数1】设备索引号\r\n"
            };

            viewModel.NewCommand.Execute();
            viewModel.SelectedLibraryItem = libraryItem;
            viewModel.AddDeviceFromLibraryCommand.Execute();

            var device = Assert.Single(viewModel.DeviceConfigs);
            Assert.Equal("BMS", device.DevType);
            Assert.Equal("周立功【ZLG_USBCANII】", device.DevName);
            Assert.Equal("_bms_zlg_usbcanii", device.DevCode);
            Assert.Equal("0,0,500", device.InitPars);
            Assert.Equal("【参数1】设备索引号\r\n", device.InitRemark);
            Assert.Equal("CAN_BMS.Bms_ZLG_USBCANII", device.DevClasss);
            Assert.True(device.IsEnable);
        }

        [Fact]
        public void DeviceLibrarySearchLeaf_IncludesParentCategoryAndMatchingLeaf()
        {
            var viewModel = CreateViewModel(new FakeDeviceConfigRepository(CreateDeviceLibrary()));

            viewModel.SearchText = "周立功";

            var category = Assert.Single(viewModel.DeviceLibraryNodes);
            Assert.Equal("BMS", category.Name);
            var leaf = Assert.Single(category.Children);
            Assert.Equal("周立功【ZLG_USBCANII】", leaf.Name);
            Assert.False(category.CanAddDevice);
            Assert.True(leaf.CanAddDevice);
        }

        [Fact]
        public void DeviceLibrarySearchCategory_IncludesAllChildren()
        {
            var viewModel = CreateViewModel(new FakeDeviceConfigRepository(CreateDeviceLibrary()));

            viewModel.SearchText = "BMS";

            var category = Assert.Single(viewModel.DeviceLibraryNodes);
            Assert.Equal("BMS", category.Name);
            Assert.Equal(new[] { "周立功【ZLG_USBCANII】", "峰岹【FT_CAN】" }, category.Children.Select(child => child.Name));
        }

        [Fact]
        public void DeviceLibrarySearchEmpty_RestoresFullTree()
        {
            var viewModel = CreateViewModel(new FakeDeviceConfigRepository(CreateDeviceLibrary()));

            viewModel.SearchText = "峰岹";
            Assert.Single(viewModel.DeviceLibraryNodes);

            viewModel.SearchText = string.Empty;

            Assert.Equal(new[] { "BMS", "IO板卡" }, viewModel.DeviceLibraryNodes.Select(node => node.Name));
            Assert.Equal(2, viewModel.DeviceLibraryNodes.First().Children.Count);
        }

        [Fact]
        public void RowCommands_PreserveWinFormsOrderingBehavior()
        {
            var dialogService = new FakeDeviceConfigDialogService { ConfirmResult = true };
            var viewModel = CreateViewModel(dialogService: dialogService);
            var first = CreateDevice("_first", true);
            var second = CreateDevice("_second", true);
            var third = CreateDevice("_third", true);

            viewModel.NewCommand.Execute();
            viewModel.DeviceConfigs.Add(first);
            viewModel.DeviceConfigs.Add(second);
            viewModel.DeviceConfigs.Add(third);
            viewModel.SelectedDeviceConfig = third;
            viewModel.MoveUpCommand.Execute();
            Assert.Equal(new[] { "_first", "_third", "_second" }, viewModel.DeviceConfigs.Select(device => device.DevCode));

            viewModel.MoveTopCommand.Execute();
            Assert.Equal(new[] { "_third", "_first", "_second" }, viewModel.DeviceConfigs.Select(device => device.DevCode));

            viewModel.MoveDownCommand.Execute();
            Assert.Equal(new[] { "_first", "_third", "_second" }, viewModel.DeviceConfigs.Select(device => device.DevCode));

            viewModel.DeleteCommand.Execute();
            Assert.Equal(new[] { "_first", "_second" }, viewModel.DeviceConfigs.Select(device => device.DevCode));
        }

        private static DeviceManagerViewModel CreateViewModel(
            FakeDeviceConfigRepository? repository = null,
            FakeDeviceConfigDialogService? dialogService = null)
        {
            var actualRepository = repository ?? new FakeDeviceConfigRepository();
            var service = new DeviceConfigService(actualRepository, new FakeLogService(), new FakeCurrentUserContext());
            return new DeviceManagerViewModel(
                service,
                new FakeMessageDialogService(),
                dialogService ?? new FakeDeviceConfigDialogService());
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

        private static IReadOnlyList<DeviceLibraryItem> CreateDeviceLibrary()
        {
            return new[]
            {
                new DeviceLibraryItem { Id = "BMS", DevName = "BMS" },
                new DeviceLibraryItem
                {
                    Id = "1",
                    DevType = "BMS",
                    DevName = "周立功【ZLG_USBCANII】",
                    ClassName = "CAN_BMS.Bms_ZLG_USBCANII",
                    InitParExample = "0,0,500",
                    InitParRemark = "【参数1】设备索引号\r\n"
                },
                new DeviceLibraryItem
                {
                    Id = "2",
                    DevType = "BMS",
                    DevName = "峰岹【FT_CAN】",
                    ClassName = "CAN_BMS.Bms_FT_CAN",
                    InitParExample = "0,1,500",
                    InitParRemark = "【参数1】设备索引号\r\n"
                },
                new DeviceLibraryItem { Id = "IO板卡", DevName = "IO板卡" },
                new DeviceLibraryItem
                {
                    Id = "3",
                    DevType = "IO板卡",
                    DevName = "自研[串口]",
                    ClassName = "PLC_LK3U.PLC",
                    InitParExample = "COM3,9600,1,1,1",
                    InitParRemark = "【参数1】端口号\r\n"
                }
            };
        }

        private sealed class FakeDeviceConfigRepository : IDeviceConfigRepository
        {
            private readonly IReadOnlyList<DeviceLibraryItem> _deviceLibrary;

            public FakeDeviceConfigRepository()
                : this(Array.Empty<DeviceLibraryItem>())
            {
            }

            public FakeDeviceConfigRepository(IReadOnlyList<DeviceLibraryItem> deviceLibrary)
            {
                _deviceLibrary = deviceLibrary;
            }

            public IReadOnlyList<DeviceConfigName> GetConfigNames()
            {
                return new[] { new DeviceConfigName("测试") };
            }

            public DeviceConfigOpenResult Open(string configName)
            {
                return DeviceConfigOpenResult.Success(configName, new[] { CreateDevice("_bms", true) }, "[{\"DevType\":\"BMS\"}]");
            }

            public IReadOnlyList<DeviceLibraryItem> GetDeviceLibrary()
            {
                return _deviceLibrary;
            }

            public string Serialize(IReadOnlyList<DeviceConfigItem> devices)
            {
                return "[{\"DevType\":\"BMS\"}]";
            }

            public void Write(string configName, IReadOnlyList<DeviceConfigItem> devices)
            {
            }
        }

        private sealed class FakeDeviceConfigDialogService : IDeviceConfigDialogService
        {
            public string SelectedConfigName { get; set; } = string.Empty;

            public string SaveConfigName { get; set; } = "测试副本";

            public bool ConfirmResult { get; set; } = true;

            public string? SelectConfigName(IReadOnlyList<DeviceConfigName> configNames)
            {
                return SelectedConfigName;
            }

            public string? RequestSaveConfigName(IReadOnlyList<DeviceConfigName> configNames)
            {
                return SaveConfigName;
            }

            public bool ConfirmDeleteDevice()
            {
                return ConfirmResult;
            }

            public bool ConfirmCancelEdit()
            {
                return ConfirmResult;
            }
        }

        private sealed class FakeMessageDialogService : IMessageDialogService
        {
            public bool Confirm(string message)
            {
                return true;
            }

            public void ShowInfo(string message)
            {
            }

            public void ShowWarning(string message)
            {
            }

            public void ShowError(string message)
            {
            }
        }

        private sealed class FakeLogService : ILogService
        {
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
