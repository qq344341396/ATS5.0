using System;
using System.Collections.ObjectModel;
using System.Linq;
using ATS5.Application.Flow;
using Prism.Commands;
using Prism.Mvvm;

namespace ATS5.Modules.Flow.ViewModels
{
    public sealed class FlowEditorViewModel : BindableBase
    {
        private readonly FlowEditorService _flowEditorService;
        private string _flowName = "测试1";
        private string _devConfigName = string.Empty;
        private string _message = string.Empty;
        private ProcessProjectDefinition? _selectedProject;
        private ProcessFlowDefinition? _currentFlow;

        public FlowEditorViewModel(FlowEditorService flowEditorService)
        {
            _flowEditorService = flowEditorService ?? throw new ArgumentNullException(nameof(flowEditorService));
            OpenCommand = new DelegateCommand(Open);
            CheckScriptsCommand = new DelegateCommand(CheckScripts, () => CurrentFlow != null).ObservesProperty(() => CurrentFlow);
            SaveCommand = new DelegateCommand(Save, () => CurrentFlow != null).ObservesProperty(() => CurrentFlow);
        }

        public string FlowName
        {
            get => _flowName;
            set => SetProperty(ref _flowName, value);
        }

        public string DevConfigName
        {
            get => _devConfigName;
            set => SetProperty(ref _devConfigName, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public ProcessFlowDefinition? CurrentFlow
        {
            get => _currentFlow;
            set => SetProperty(ref _currentFlow, value);
        }

        public ProcessProjectDefinition? SelectedProject
        {
            get => _selectedProject;
            set => SetProperty(ref _selectedProject, value);
        }

        public ObservableCollection<ProcessProjectDefinition> Projects { get; } = new ObservableCollection<ProcessProjectDefinition>();

        public DelegateCommand OpenCommand { get; }

        public DelegateCommand CheckScriptsCommand { get; }

        public DelegateCommand SaveCommand { get; }

        private void Open()
        {
            var result = _flowEditorService.Open(FlowName);
            if (!result.IsSuccess)
            {
                Message = result.Message;
                return;
            }

            CurrentFlow = result.Flow;
            DevConfigName = result.Flow.DevCfgName;
            Projects.Clear();
            foreach (var project in result.Flow.Projects)
            {
                Projects.Add(project);
            }

            SelectedProject = Projects.FirstOrDefault();
            Message = string.Empty;
        }

        private void CheckScripts()
        {
            if (CurrentFlow == null)
            {
                return;
            }

            var currentFlow = CurrentFlow;
            var result = _flowEditorService.CheckAllScripts(currentFlow, FlowName);
            Message = result.Message;
        }

        private void Save()
        {
            if (CurrentFlow == null)
            {
                return;
            }

            CurrentFlow.DevCfgName = DevConfigName;
            var currentFlow = CurrentFlow;
            var result = _flowEditorService.SaveSampleCopy(currentFlow, FlowName);
            Message = result.Message;
        }
    }
}
