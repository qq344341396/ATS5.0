using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ATS5.Application.DeviceConfig;
using ATS5.Modules.Device.ViewModels;
using ATS5.Wpf.Core;

namespace ATS5.Modules.Device.Views
{
    public sealed class DeviceConfigDialogService : IDeviceConfigDialogService
    {
        private readonly IMessageDialogService _messageDialogService;

        public DeviceConfigDialogService(IMessageDialogService messageDialogService)
        {
            _messageDialogService = messageDialogService ?? throw new ArgumentNullException(nameof(messageDialogService));
        }

        public string? SelectConfigName(IReadOnlyList<DeviceConfigName> configNames)
        {
            var dialog = new DeviceConfigSelectWindow(configNames);
            AssignOwner(dialog);
            return dialog.ShowDialog() == true ? dialog.SelectedConfigName : null;
        }

        public string? RequestSaveConfigName(IReadOnlyList<DeviceConfigName> configNames)
        {
            var dialog = new DeviceConfigSaveWindow(configNames, _messageDialogService);
            AssignOwner(dialog);
            return dialog.ShowDialog() == true ? dialog.ConfigName : null;
        }

        public bool ConfirmDeleteDevice()
        {
            return _messageDialogService.Confirm("确定要删除该配置？");
        }

        public bool ConfirmCancelEdit()
        {
            return _messageDialogService.Confirm("确定要取消编辑？");
        }

        private sealed class DeviceConfigSelectWindow : Window
        {
            private readonly ListBox _configListBox;

            public DeviceConfigSelectWindow(IReadOnlyList<DeviceConfigName> configNames)
            {
                Title = "设备配置清单";
                Width = 360;
                Height = 420;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ResizeMode = ResizeMode.NoResize;
                SelectedConfigName = null;

                _configListBox = CreateConfigListBox(configNames);
                _configListBox.MouseDoubleClick += (_, _) => ConfirmSelection();

                var okButton = new Button { Content = "确定", Width = 72, Height = 28, Margin = new Thickness(4) };
                okButton.Click += (_, _) => ConfirmSelection();
                var cancelButton = new Button { Content = "取消", Width = 72, Height = 28, Margin = new Thickness(4) };
                cancelButton.Click += (_, _) => DialogResult = false;

                Content = CreateDialogLayout(_configListBox, okButton, cancelButton);
            }

            public string? SelectedConfigName { get; private set; }

            private void ConfirmSelection()
            {
                var selectedName = _configListBox.SelectedItem as DeviceConfigName;
                if (selectedName == null)
                {
                    return;
                }

                SelectedConfigName = selectedName.Name;
                DialogResult = true;
            }
        }

        private sealed class DeviceConfigSaveWindow : Window
        {
            private readonly IReadOnlyList<DeviceConfigName> _configNames;
            private readonly IMessageDialogService _messageDialogService;
            private readonly ListBox _configListBox;
            private readonly TextBox _configNameTextBox;

            public DeviceConfigSaveWindow(
                IReadOnlyList<DeviceConfigName> configNames,
                IMessageDialogService messageDialogService)
            {
                _configNames = configNames;
                _messageDialogService = messageDialogService;
                Title = "设备配置";
                Width = 420;
                Height = 460;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ResizeMode = ResizeMode.NoResize;
                ConfigName = null;

                _configNameTextBox = new TextBox { MinWidth = 220, Margin = new Thickness(6, 4, 6, 4) };
                _configListBox = CreateConfigListBox(configNames);
                _configListBox.SelectionChanged += (_, _) =>
                {
                    var selectedName = _configListBox.SelectedItem as DeviceConfigName;
                    if (selectedName != null)
                    {
                        _configNameTextBox.Text = selectedName.Name;
                    }
                };

                var okButton = new Button { Content = "确定", Width = 72, Height = 28, Margin = new Thickness(4) };
                okButton.Click += (_, _) => ConfirmSaveName();
                var cancelButton = new Button { Content = "取消", Width = 72, Height = 28, Margin = new Thickness(4) };
                cancelButton.Click += (_, _) => DialogResult = false;

                Content = CreateSaveLayout(_configNameTextBox, _configListBox, okButton, cancelButton);
            }

            public string? ConfigName { get; private set; }

            private void ConfirmSaveName()
            {
                var configName = _configNameTextBox.Text;
                if (configName.Length == 0)
                {
                    _messageDialogService.ShowWarning("设备配置类别不能为空");
                    _configNameTextBox.Focus();
                    return;
                }

                if (!DeviceConfigNameValidator.IsValidLeafName(configName))
                {
                    _messageDialogService.ShowWarning(DeviceConfigNameValidator.InvalidConfigNameMessage);
                    _configNameTextBox.Focus();
                    return;
                }

                if (_configNames.Any(name => name.Name == configName) && !_messageDialogService.Confirm("确定要覆盖原文件？"))
                {
                    _configNameTextBox.Focus();
                    return;
                }

                ConfigName = configName;
                DialogResult = true;
            }
        }

        private static ListBox CreateConfigListBox(IReadOnlyList<DeviceConfigName> configNames)
        {
            return new ListBox
            {
                DisplayMemberPath = nameof(DeviceConfigName.Name),
                ItemsSource = configNames,
                Margin = new Thickness(8)
            };
        }

        private static DockPanel CreateDialogLayout(ListBox configListBox, Button okButton, Button cancelButton)
        {
            var root = new DockPanel { LastChildFill = true, Margin = new Thickness(8) };
            var title = new TextBlock { Text = "FileName", FontWeight = FontWeights.SemiBold, Margin = new Thickness(8, 4, 8, 0) };
            DockPanel.SetDock(title, Dock.Top);
            root.Children.Add(title);
            AddButtonBar(root, okButton, cancelButton);
            root.Children.Add(configListBox);
            return root;
        }

        private static DockPanel CreateSaveLayout(TextBox configNameTextBox, ListBox configListBox, Button okButton, Button cancelButton)
        {
            var root = new DockPanel { LastChildFill = true, Margin = new Thickness(8) };
            var inputPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(8, 4, 8, 4) };
            inputPanel.Children.Add(new TextBlock { Text = "设备配置类别", VerticalAlignment = VerticalAlignment.Center });
            inputPanel.Children.Add(configNameTextBox);
            DockPanel.SetDock(inputPanel, Dock.Top);
            root.Children.Add(inputPanel);

            var title = new TextBlock { Text = "FileName", FontWeight = FontWeights.SemiBold, Margin = new Thickness(8, 4, 8, 0) };
            DockPanel.SetDock(title, Dock.Top);
            root.Children.Add(title);
            AddButtonBar(root, okButton, cancelButton);
            root.Children.Add(configListBox);
            return root;
        }

        private static void AddButtonBar(DockPanel root, Button okButton, Button cancelButton)
        {
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(8)
            };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            DockPanel.SetDock(buttonPanel, Dock.Bottom);
            root.Children.Add(buttonPanel);
        }

        private static void AssignOwner(Window dialog)
        {
            var owner = ResolveOwner();
            if (owner != null && !ReferenceEquals(owner, dialog))
            {
                dialog.Owner = owner;
            }
        }

        private static Window? ResolveOwner()
        {
            return System.Windows.Application.Current?.Windows
                .OfType<Window>()
                .FirstOrDefault(window => window.IsActive) ??
                System.Windows.Application.Current?.MainWindow;
        }
    }
}
