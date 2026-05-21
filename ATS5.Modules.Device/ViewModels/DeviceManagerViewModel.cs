using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ATS5.Application.DeviceConfig;
using ATS5.Wpf.Core;
using Prism.Commands;
using Prism.Mvvm;

namespace ATS5.Modules.Device.ViewModels
{
    public sealed class DeviceManagerViewModel : BindableBase
    {
        private readonly DeviceConfigService _deviceConfigService;
        private readonly IMessageDialogService _messageDialogService;
        private readonly IDeviceConfigDialogService _deviceConfigDialogService;
        private readonly List<DeviceLibraryItem> _allDeviceLibrary = new List<DeviceLibraryItem>();
        private DeviceConfigMode _currentMode;
        private string _configName = string.Empty;
        private string _initRemark = string.Empty;
        private string _originalJson = string.Empty;
        private string _searchText = string.Empty;
        private DeviceConfigItem? _selectedDeviceConfig;
        private DeviceLibraryItem? _selectedLibraryItem;

        public DeviceManagerViewModel(
            DeviceConfigService deviceConfigService,
            IMessageDialogService messageDialogService,
            IDeviceConfigDialogService deviceConfigDialogService)
        {
            _deviceConfigService = deviceConfigService ?? throw new ArgumentNullException(nameof(deviceConfigService));
            _messageDialogService = messageDialogService ?? throw new ArgumentNullException(nameof(messageDialogService));
            _deviceConfigDialogService = deviceConfigDialogService ?? throw new ArgumentNullException(nameof(deviceConfigDialogService));
            NewCommand = new DelegateCommand(New);
            EditCommand = new DelegateCommand(Edit, () => _currentMode == DeviceConfigMode.Default && !string.IsNullOrEmpty(ConfigName));
            OpenCommand = new DelegateCommand(Open, () => _currentMode == DeviceConfigMode.Default);
            SaveCommand = new DelegateCommand(Save, () => _currentMode != DeviceConfigMode.Default);
            SaveAsCommand = new DelegateCommand(SaveAs, () => _currentMode == DeviceConfigMode.Edit);
            CancelCommand = new DelegateCommand(Cancel, () => _currentMode != DeviceConfigMode.Default);
            AddDeviceFromLibraryCommand = new DelegateCommand(AddDeviceFromLibrary, () => _currentMode != DeviceConfigMode.Default && SelectedLibraryItem != null && !SelectedLibraryItem.IsCategory);
            MoveUpCommand = new DelegateCommand(MoveUp, CanMoveSelectedDevice);
            MoveDownCommand = new DelegateCommand(MoveDown, CanMoveSelectedDevice);
            MoveTopCommand = new DelegateCommand(MoveTop, CanMoveSelectedDevice);
            DeleteCommand = new DelegateCommand(Delete, CanMoveSelectedDevice);
            LoadDeviceLibrary();
            RefreshCommandStates();
        }

        public string ConfigName
        {
            get => _configName;
            set
            {
                if (SetProperty(ref _configName, value ?? string.Empty))
                {
                    RaisePropertyChanged(nameof(Title));
                    RefreshCommandStates();
                }
            }
        }

        public string InitRemark
        {
            get => _initRemark;
            set => SetProperty(ref _initRemark, value ?? string.Empty);
        }

        public string Title => string.IsNullOrEmpty(ConfigName) ? string.Empty : $"【设备配置：{ConfigName}】";

        public bool IsGridReadOnly => _currentMode == DeviceConfigMode.Default;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value ?? string.Empty))
                {
                    RebuildDeviceLibraryNodes(_searchText);
                }
            }
        }

        public DeviceConfigItem? SelectedDeviceConfig
        {
            get => _selectedDeviceConfig;
            set
            {
                if (SetProperty(ref _selectedDeviceConfig, value))
                {
                    InitRemark = value?.InitRemark ?? string.Empty;
                    RefreshCommandStates();
                }
            }
        }

        public DeviceLibraryItem? SelectedLibraryItem
        {
            get => _selectedLibraryItem;
            set
            {
                if (SetProperty(ref _selectedLibraryItem, value))
                {
                    AddDeviceFromLibraryCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<DeviceConfigItem> DeviceConfigs { get; } = new ObservableCollection<DeviceConfigItem>();

        public ObservableCollection<DeviceLibraryItem> DeviceLibrary { get; } = new ObservableCollection<DeviceLibraryItem>();

        public ObservableCollection<DeviceLibraryNodeViewModel> DeviceLibraryNodes { get; } = new ObservableCollection<DeviceLibraryNodeViewModel>();

        public DelegateCommand NewCommand { get; }

        public DelegateCommand EditCommand { get; }

        public DelegateCommand OpenCommand { get; }

        public DelegateCommand SaveCommand { get; }

        public DelegateCommand SaveAsCommand { get; }

        public DelegateCommand CancelCommand { get; }

        public DelegateCommand AddDeviceFromLibraryCommand { get; }

        public DelegateCommand MoveUpCommand { get; }

        public DelegateCommand MoveDownCommand { get; }

        public DelegateCommand MoveTopCommand { get; }

        public DelegateCommand DeleteCommand { get; }

        private void New()
        {
            _currentMode = DeviceConfigMode.Add;
            ConfigName = string.Empty;
            _originalJson = string.Empty;
            DeviceConfigs.Clear();
            SelectedDeviceConfig = null;
            RaisePropertyChanged(nameof(IsGridReadOnly));
            RefreshCommandStates();
        }

        private void Edit()
        {
            _currentMode = DeviceConfigMode.Edit;
            RaisePropertyChanged(nameof(IsGridReadOnly));
            RefreshCommandStates();
        }

        private void Open()
        {
            var configName = _deviceConfigDialogService.SelectConfigName(_deviceConfigService.GetConfigNames());
            if (configName == null || configName.Length == 0)
            {
                return;
            }

            var selectedConfigName = configName ?? string.Empty;
            var result = _deviceConfigService.Open(selectedConfigName);
            if (!result.IsSuccess)
            {
                _messageDialogService.ShowWarning(result.Message);
                return;
            }

            LoadOpenedConfig(result);
        }

        private void Save()
        {
            if (_currentMode == DeviceConfigMode.Add)
            {
                var configName = _deviceConfigDialogService.RequestSaveConfigName(_deviceConfigService.GetConfigNames());
                if (configName == null || configName.Length == 0)
                {
                    return;
                }

                var newConfigName = configName ?? string.Empty;
                var result = _deviceConfigService.Save(newConfigName, DeviceConfigs.ToArray(), DeviceConfigSaveMode.New, string.Empty);
                CompleteSave(result, newConfigName);
                return;
            }

            var overwriteResult = _deviceConfigService.Save(ConfigName, DeviceConfigs.ToArray(), DeviceConfigSaveMode.Overwrite, _originalJson);
            CompleteSave(overwriteResult, ConfigName);
        }

        private void SaveAs()
        {
            var configName = _deviceConfigDialogService.RequestSaveConfigName(_deviceConfigService.GetConfigNames());
            if (configName == null || configName.Length == 0)
            {
                return;
            }

            var saveAsConfigName = configName ?? string.Empty;
            var result = _deviceConfigService.SaveAs(ConfigName, saveAsConfigName, DeviceConfigs.ToArray());
            CompleteSave(result, saveAsConfigName);
        }

        private void Cancel()
        {
            if (!_deviceConfigDialogService.ConfirmCancelEdit())
            {
                return;
            }

            if (_currentMode == DeviceConfigMode.Add)
            {
                ConfigName = string.Empty;
                DeviceConfigs.Clear();
                SelectedDeviceConfig = null;
            }
            else if (!string.IsNullOrEmpty(ConfigName))
            {
                var result = _deviceConfigService.Open(ConfigName);
                if (result.IsSuccess)
                {
                    LoadOpenedConfig(result);
                }
                else
                {
                    ConfigName = string.Empty;
                    DeviceConfigs.Clear();
                    SelectedDeviceConfig = null;
                }
            }

            _currentMode = DeviceConfigMode.Default;
            RaisePropertyChanged(nameof(IsGridReadOnly));
            RefreshCommandStates();
        }

        private void AddDeviceFromLibrary()
        {
            if (_currentMode == DeviceConfigMode.Default || SelectedLibraryItem == null || SelectedLibraryItem.IsCategory)
            {
                return;
            }

            var className = SelectedLibraryItem.ClassName ?? string.Empty;
            DeviceConfigs.Add(
                new DeviceConfigItem
                {
                    DevType = SelectedLibraryItem.DevType,
                    DevName = SelectedLibraryItem.DevName,
                    DevCode = $"_{className.Split('.').Last().ToLower()}",
                    InitPars = SelectedLibraryItem.InitParExample,
                    InitRemark = SelectedLibraryItem.InitParRemark,
                    DevClasss = className,
                    IsEnable = true
                });
        }

        private void MoveUp()
        {
            var index = GetSelectedIndex();
            if (index > 0)
            {
                DeviceConfigs.Move(index, index - 1);
                SelectedDeviceConfig = DeviceConfigs[index - 1];
            }
        }

        private void MoveDown()
        {
            var index = GetSelectedIndex();
            if (index >= 0 && index < DeviceConfigs.Count - 1)
            {
                DeviceConfigs.Move(index, index + 1);
                SelectedDeviceConfig = DeviceConfigs[index + 1];
            }
        }

        private void MoveTop()
        {
            var index = GetSelectedIndex();
            if (index > 0)
            {
                DeviceConfigs.Move(index, 0);
                SelectedDeviceConfig = DeviceConfigs[0];
            }
        }

        private void Delete()
        {
            var index = GetSelectedIndex();
            if (index < 0 || !_deviceConfigDialogService.ConfirmDeleteDevice())
            {
                return;
            }

            DeviceConfigs.RemoveAt(index);
            SelectedDeviceConfig = index < DeviceConfigs.Count ? DeviceConfigs[index] : DeviceConfigs.LastOrDefault();
        }

        private void LoadDeviceLibrary()
        {
            _allDeviceLibrary.Clear();
            DeviceLibrary.Clear();
            foreach (var item in _deviceConfigService.GetDeviceLibrary())
            {
                _allDeviceLibrary.Add(item);
                DeviceLibrary.Add(item);
            }

            RebuildDeviceLibraryNodes(SearchText);
        }

        private void LoadOpenedConfig(DeviceConfigOpenResult result)
        {
            ConfigName = result.ConfigName;
            _originalJson = result.OriginalJson;
            DeviceConfigs.Clear();
            foreach (var device in result.Devices)
            {
                DeviceConfigs.Add(device);
            }

            SelectedDeviceConfig = DeviceConfigs.FirstOrDefault();
            _currentMode = DeviceConfigMode.Default;
            RaisePropertyChanged(nameof(IsGridReadOnly));
            RefreshCommandStates();
        }

        private void CompleteSave(DeviceConfigOperationResult result, string configName)
        {
            if (!result.IsSuccess)
            {
                _messageDialogService.ShowWarning(result.Message);
                return;
            }

            _messageDialogService.ShowInfo(result.Message);
            ConfigName = configName;
            _originalJson = _deviceConfigService.Serialize(DeviceConfigs.ToArray());
            _currentMode = DeviceConfigMode.Default;
            RaisePropertyChanged(nameof(IsGridReadOnly));
            RefreshCommandStates();
        }

        private bool CanMoveSelectedDevice()
        {
            return _currentMode != DeviceConfigMode.Default && SelectedDeviceConfig != null;
        }

        private int GetSelectedIndex()
        {
            return SelectedDeviceConfig == null ? -1 : DeviceConfigs.IndexOf(SelectedDeviceConfig);
        }

        private void RefreshCommandStates()
        {
            EditCommand.RaiseCanExecuteChanged();
            OpenCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            SaveAsCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            AddDeviceFromLibraryCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
            MoveDownCommand.RaiseCanExecuteChanged();
            MoveTopCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }

        private void RebuildDeviceLibraryNodes(string searchText)
        {
            DeviceLibraryNodes.Clear();
            var filteredItems = FilterDeviceLibrary(searchText);
            var categoryNodes = new Dictionary<string, DeviceLibraryNodeViewModel>();
            foreach (var item in filteredItems.Where(item => item.IsCategory))
            {
                var categoryNode = new DeviceLibraryNodeViewModel(item);
                DeviceLibraryNodes.Add(categoryNode);
                if (item.Id != null && item.Id.Length > 0)
                {
                    categoryNodes[item.Id] = categoryNode;
                }
            }

            foreach (var item in filteredItems.Where(item => !item.IsCategory))
            {
                if (item.DevType == null || !categoryNodes.TryGetValue(item.DevType, out var categoryNode))
                {
                    continue;
                }

                categoryNode.Children.Add(new DeviceLibraryNodeViewModel(item));
            }
        }

        private IReadOnlyList<DeviceLibraryItem> FilterDeviceLibrary(string searchText)
        {
            var keyword = (searchText ?? string.Empty).Trim();
            if (keyword.Length == 0)
            {
                return _allDeviceLibrary.ToArray();
            }

            var filteredItems = new List<DeviceLibraryItem>();
            var matchedItems = _allDeviceLibrary
                .Where(item => (item.DevName ?? string.Empty).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();

            foreach (var item in matchedItems)
            {
                if (item.IsCategory)
                {
                    AddDistinct(filteredItems, item);
                    foreach (var child in _allDeviceLibrary.Where(child => child.DevType == item.Id))
                    {
                        AddDistinct(filteredItems, child);
                    }
                }
                else
                {
                    var category = _allDeviceLibrary.FirstOrDefault(categoryItem => categoryItem.IsCategory && categoryItem.Id == item.DevType);
                    if (category != null)
                    {
                        AddDistinct(filteredItems, category);
                    }

                    AddDistinct(filteredItems, item);
                }
            }

            return filteredItems;
        }

        private static void AddDistinct(List<DeviceLibraryItem> items, DeviceLibraryItem item)
        {
            if (!items.Contains(item))
            {
                items.Add(item);
            }
        }

        private enum DeviceConfigMode
        {
            Default,
            Add,
            Edit
        }
    }
}
