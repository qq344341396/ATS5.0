using System;
using System.Collections.Generic;
using System.Linq;
using ATS5.Application.Flow;
using ATS5.Modules.Flow.ViewModels;
using Xunit;

namespace ATS5.Tests.Flow
{
    public sealed class FlowEditorViewModelTests
    {
        [Fact]
        public void Commands_AreDisabled_WhenFlowIsNotOpened()
        {
            var repository = new FakeFlowRepository();
            var viewModel = CreateViewModel(repository);

            Assert.False(viewModel.SaveCommand.CanExecute());
            Assert.False(viewModel.CheckScriptsCommand.CanExecute());
            Assert.Empty(repository.Writes);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenFlowEditorServiceIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new FlowEditorViewModel(null!));

            Assert.Equal("flowEditorService", exception.ParamName);
        }

        [Fact]
        public void Open_LoadsProjectsSelectsFirstProjectAndShowsDeviceConfig()
        {
            var repository = new FakeFlowRepository();
            var viewModel = CreateViewModel(repository);

            viewModel.OpenCommand.Execute();
            var projects = ((IEnumerable<ProcessProjectDefinition>)viewModel.Projects).ToList();

            Assert.Same(repository.Flow, viewModel.CurrentFlow);
            Assert.Equal("DeviceA", viewModel.DevConfigName);
            Assert.Equal(new[] { "P1", "P2" }, projects.Select(project => project.ProjectName));
            Assert.Same(projects.First(), viewModel.SelectedProject);
            Assert.True(viewModel.SaveCommand.CanExecute());
            Assert.True(viewModel.CheckScriptsCommand.CanExecute());
        }

        [Fact]
        public void Save_WritesOnlySampleCopyAndKeepsSourceFlowFileUntouched()
        {
            var repository = new FakeFlowRepository();
            var validator = new FakeFlowScriptValidator();
            var viewModel = CreateViewModel(repository, validator);

            viewModel.FlowName = "Sample";
            viewModel.OpenCommand.Execute();
            viewModel.DevConfigName = "DeviceB";
            viewModel.SaveCommand.Execute();

            Assert.Equal("DeviceB", repository.Flow.DevCfgName);
            Assert.Equal(new[] { "Sample-WpfSample-20260521123045" }, repository.Writes);
            Assert.DoesNotContain("Sample", repository.Writes);
            Assert.Equal(new[] { "Sample" }, validator.CheckedFlowNames);
            Assert.Equal("保存成功：Sample-WpfSample-20260521123045.fw", viewModel.Message);
        }

        [Fact]
        public void OpenCommand_PropagatesUnexpectedExceptionAndDoesNotExposeSystemMessage()
        {
            var repository = new FakeFlowRepository
            {
                OpenException = new InvalidOperationException("repository failed")
            };
            var viewModel = CreateViewModel(repository);

            var exception = Assert.Throws<InvalidOperationException>(() => viewModel.OpenCommand.Execute());

            Assert.Equal("repository failed", exception.Message);
            Assert.Equal(string.Empty, viewModel.Message);
        }

        private static FlowEditorViewModel CreateViewModel(
            FakeFlowRepository repository,
            FakeFlowScriptValidator? validator = null)
        {
            var service = new FlowEditorService(
                repository,
                validator ?? new FakeFlowScriptValidator(),
                new FakeOperationLogger(),
                new FakeFlowChangePublisher());

            return new FlowEditorViewModel(service);
        }

        private sealed class FakeFlowRepository : IFlowRepository
        {
            public ProcessFlowDefinition Flow { get; } = CreateFlow();

            public List<string> Writes { get; } = new List<string>();

            public InvalidOperationException? OpenException { get; set; }

            public FlowOpenResult Open(string flowName)
            {
                if (OpenException != null)
                {
                    throw OpenException;
                }

                return FlowOpenResult.Success(flowName, Flow, "{\"flow\":true}");
            }

            public bool ExistsDeviceConfig(string deviceConfigName)
            {
                return true;
            }

            public bool DeviceConfigHasContent(string deviceConfigName)
            {
                return true;
            }

            public string Serialize(ProcessFlowDefinition flow)
            {
                return "{\"serialized\":true}";
            }

            public void WriteFlow(string flowName, ProcessFlowDefinition flow)
            {
                Writes.Add(flowName);
            }

            public void WriteBackup(string flowName, string json)
            {
                Writes.Add($"{flowName}.bak");
            }

            public string CreateSampleCopyName(string sourceFlowName)
            {
                return $"{sourceFlowName}-WpfSample-20260521123045";
            }

            private static ProcessFlowDefinition CreateFlow()
            {
                return new ProcessFlowDefinition
                {
                    DevCfgName = "DeviceA",
                    Projects =
                    {
                        new ProcessProjectDefinition
                        {
                            IsEnable = true,
                            ProjectName = "P1",
                            Script = "var p1 = 1;"
                        },
                        new ProcessProjectDefinition
                        {
                            IsEnable = true,
                            ProjectName = "P2",
                            Script = "var p2 = 2;"
                        }
                    }
                };
            }
        }

        private sealed class FakeFlowScriptValidator : IFlowScriptValidator
        {
            public List<string> CheckedFlowNames { get; } = new List<string>();

            public FlowScriptValidationResult CheckAll(ProcessFlowDefinition flow, string flowName)
            {
                CheckedFlowNames.Add(flowName);
                return FlowScriptValidationResult.Success();
            }
        }

        private sealed class FakeOperationLogger : IOperationLogger
        {
            public void Operate(string message)
            {
            }
        }

        private sealed class FakeFlowChangePublisher : IFlowChangePublisher
        {
            public void PublishSaved(string flowName)
            {
            }
        }
    }
}
