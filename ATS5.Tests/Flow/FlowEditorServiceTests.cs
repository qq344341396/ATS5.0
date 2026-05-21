using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS5.Application.Flow;
using Xunit;

namespace ATS5.Tests.Flow
{
    public sealed class FlowEditorServiceTests
    {
        [Fact]
        public void Open_LoadsFlowWithoutWritingFiles()
        {
            var repository = new FakeFlowRepository();
            var service = new FlowEditorService(repository, new FakeFlowScriptValidator(), new FakeOperationLogger(), new FakeFlowChangePublisher());

            var result = service.Open("Sample");

            Assert.True(result.IsSuccess);
            Assert.Equal("Sample", result.FlowName);
            Assert.Equal("DeviceA", result.Flow.DevCfgName);
            Assert.Empty(repository.Writes);
        }

        [Fact]
        public void SaveSampleCopy_ReturnsLegacyWarning_WhenFlowHasNoProjects()
        {
            var service = new FlowEditorService(new FakeFlowRepository(), new FakeFlowScriptValidator(), new FakeOperationLogger(), new FakeFlowChangePublisher());
            var flow = new ProcessFlowDefinition { DevCfgName = "DeviceA" };

            var result = service.SaveSampleCopy(flow, "Sample");

            Assert.False(result.IsSuccess);
            Assert.Equal("无测试项目信息，无需保存", result.Message);
        }

        [Fact]
        public void SaveSampleCopy_ReturnsLegacyWarning_WhenDeviceConfigMissing()
        {
            var repository = new FakeFlowRepository { DeviceConfigExists = false };
            var service = new FlowEditorService(repository, new FakeFlowScriptValidator(), new FakeOperationLogger(), new FakeFlowChangePublisher());

            var result = service.SaveSampleCopy(CreateFlow(), "Sample");

            Assert.False(result.IsSuccess);
            Assert.Equal("设备配置文件不存在，请检查！", result.Message);
        }

        [Fact]
        public void SaveSampleCopy_ReturnsLegacyWarning_WhenDeviceConfigNameMissing()
        {
            var repository = new FakeFlowRepository();
            var service = new FlowEditorService(repository, new FakeFlowScriptValidator(), new FakeOperationLogger(), new FakeFlowChangePublisher());
            var flow = CreateFlow();
            flow.DevCfgName = string.Empty;

            var result = service.SaveSampleCopy(flow, "Sample");

            Assert.False(result.IsSuccess);
            Assert.Equal("请先关联设备配置，再执行此操作", result.Message);
            Assert.Empty(repository.Writes);
        }

        [Fact]
        public void SaveSampleCopy_ReturnsLegacyWarning_WhenDeviceConfigEmpty()
        {
            var repository = new FakeFlowRepository { IsDeviceConfigContentValid = false };
            var service = new FlowEditorService(repository, new FakeFlowScriptValidator(), new FakeOperationLogger(), new FakeFlowChangePublisher());

            var result = service.SaveSampleCopy(CreateFlow(), "Sample");

            Assert.False(result.IsSuccess);
            Assert.Equal("设备配置文件异常，请检查！", result.Message);
            Assert.Empty(repository.Writes);
        }

        [Fact]
        public void Save_RejectsOverwriteMode_DuringSampleStage()
        {
            var repository = new FakeFlowRepository { ExistingJson = "{\"old\":true}" };
            var validator = new FakeFlowScriptValidator();
            var logger = new FakeOperationLogger();
            var publisher = new FakeFlowChangePublisher();
            var service = new FlowEditorService(repository, validator, logger, publisher);
            var flow = CreateFlow();

            var result = service.Save(flow, "Sample", FlowSaveMode.Overwrite);

            Assert.False(result.IsSuccess);
            Assert.Equal("样板阶段只允许保存测试副本", result.Message);
            Assert.Empty(validator.CheckedFlowNames);
            Assert.Empty(repository.Writes);
            Assert.Empty(logger.Messages);
            Assert.Empty(publisher.Messages);
        }

        [Fact]
        public void Save_RejectsSaveAsSourceName_DuringSampleStage()
        {
            var repository = new FakeFlowRepository { SampleCopyName = "Sample-WpfSample-20260521123045" };
            var validator = new FakeFlowScriptValidator();
            var publisher = new FakeFlowChangePublisher();
            var service = new FlowEditorService(repository, validator, new FakeOperationLogger(), publisher);

            var result = service.Save(CreateFlow(), "Sample", FlowSaveMode.SaveAs);

            Assert.False(result.IsSuccess);
            Assert.Equal("样板阶段只允许保存测试副本", result.Message);
            Assert.Empty(validator.CheckedFlowNames);
            Assert.Empty(repository.Writes);
            Assert.Empty(publisher.Messages);
        }

        [Fact]
        public void SaveSampleCopy_ValidatesSourceFlowAndWritesTimestampedWpfSampleCopy()
        {
            var repository = new FakeFlowRepository { SampleCopyName = "Sample-WpfSample-20260521123045" };
            var validator = new FakeFlowScriptValidator();
            var publisher = new FakeFlowChangePublisher();
            var service = new FlowEditorService(repository, validator, new FakeOperationLogger(), publisher);

            var result = service.SaveSampleCopy(CreateFlow(), "Sample");

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { "Sample" }, validator.CheckedFlowNames);
            Assert.Equal(new[] { "flow:Sample-WpfSample-20260521123045" }, repository.Writes.Select(x => x.Kind + ":" + x.Name));
            Assert.Equal("Sample-WpfSample-20260521123045@UcFlow", publisher.Messages.Single());
            Assert.Equal("保存成功：Sample-WpfSample-20260521123045.fw", result.Message);
        }

        [Fact]
        public void SaveSampleCopy_DoesNotWrite_WhenScriptValidationFails()
        {
            var repository = new FakeFlowRepository { SampleCopyName = "Sample-WpfSample-20260521123045" };
            var validator = new FakeFlowScriptValidator { Result = FlowScriptValidationResult.Fail("测试项目【P1】：编译错误") };
            var publisher = new FakeFlowChangePublisher();
            var service = new FlowEditorService(repository, validator, new FakeOperationLogger(), publisher);

            var result = service.SaveSampleCopy(CreateFlow(), "Sample");

            Assert.False(result.IsSuccess);
            Assert.Equal("脚本检验失败：\r\n测试项目【P1】：编译错误", result.Message);
            Assert.Equal(new[] { "Sample" }, validator.CheckedFlowNames);
            Assert.Empty(repository.Writes);
            Assert.Empty(publisher.Messages);
        }

        [Fact]
        public void SaveSampleCopy_DoesNotWrite_WhenDeterministicScriptGateFails()
        {
            var repository = new FakeFlowRepository { SampleCopyName = "Sample-WpfSample-20260521123045" };
            var validator = new FakeFlowScriptValidator
            {
                Result = FlowScriptValidationResult.FailWithCompleteMessage(
                    "测试项【P1】中输出项【Out1】的【底层命名】与临时变量【Temp1】变量重复,请检查!")
            };
            var publisher = new FakeFlowChangePublisher();
            var service = new FlowEditorService(repository, validator, new FakeOperationLogger(), publisher);

            var result = service.SaveSampleCopy(CreateFlow(), "Sample");

            Assert.False(result.IsSuccess);
            Assert.Equal("测试项【P1】中输出项【Out1】的【底层命名】与临时变量【Temp1】变量重复,请检查!", result.Message);
            Assert.Equal(new[] { "Sample" }, validator.CheckedFlowNames);
            Assert.Empty(repository.Writes);
            Assert.Empty(publisher.Messages);
        }

        [Fact]
        public void SaveSampleCopy_DoesNotOverwriteSource_WhenRepositoryReturnsSourceName()
        {
            var repository = new FakeFlowRepository { SampleCopyName = "Sample" };
            var validator = new FakeFlowScriptValidator();
            var service = new FlowEditorService(repository, validator, new FakeOperationLogger(), new FakeFlowChangePublisher());

            var result = service.SaveSampleCopy(CreateFlow(), "Sample");

            Assert.False(result.IsSuccess);
            Assert.Equal("样板阶段只允许保存测试副本", result.Message);
            Assert.Empty(validator.CheckedFlowNames);
            Assert.Empty(repository.Writes);
        }

        [Fact]
        public void CheckAllScripts_ReturnsLegacyFailureMessage_WhenCompilerFails()
        {
            var repository = new FakeFlowRepository();
            var validator = new FakeFlowScriptValidator { Result = FlowScriptValidationResult.Fail("测试项目【P1】：编译错误") };
            var service = new FlowEditorService(repository, validator, new FakeOperationLogger(), new FakeFlowChangePublisher());

            var result = service.CheckAllScripts(CreateFlow(), "Sample");

            Assert.False(result.IsSuccess);
            Assert.Equal("脚本检验失败：\r\n测试项目【P1】：编译错误", result.Message);
        }

        [Fact]
        public void CheckAllScripts_DoesNotWrapCompleteLegacyValidationMessages()
        {
            var repository = new FakeFlowRepository();
            var validator = new FakeFlowScriptValidator { Result = FlowScriptValidationResult.FailWithCompleteMessage("测试项名称:【P1】,重复,请检查!") };
            var service = new FlowEditorService(repository, validator, new FakeOperationLogger(), new FakeFlowChangePublisher());

            var result = service.CheckAllScripts(CreateFlow(), "Sample");

            Assert.False(result.IsSuccess);
            Assert.Equal("测试项名称:【P1】,重复,请检查!", result.Message);
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
                        Script = "var x = 1;"
                    }
                }
            };
        }

        private sealed class FakeFlowRepository : IFlowRepository
        {
            public bool DeviceConfigExists { get; set; } = true;

            public bool IsDeviceConfigContentValid { get; set; } = true;

            public string ExistingJson { get; set; } = "{\"new\":false}";

            public string SampleCopyName { get; set; } = "Sample-WpfSample-20260521123045";

            public List<WriteCall> Writes { get; } = new List<WriteCall>();

            public FlowOpenResult Open(string flowName)
            {
                return FlowOpenResult.Success(flowName, CreateFlow(), ExistingJson);
            }

            public bool ExistsDeviceConfig(string deviceConfigName)
            {
                return DeviceConfigExists;
            }

            public bool DeviceConfigHasContent(string deviceConfigName)
            {
                return IsDeviceConfigContentValid;
            }

            public string Serialize(ProcessFlowDefinition flow)
            {
                return "{\"serialized\":true}";
            }

            public void WriteFlow(string flowName, ProcessFlowDefinition flow)
            {
                Writes.Add(new WriteCall("flow", flowName));
            }

            public void WriteBackup(string flowName, string json)
            {
                Writes.Add(new WriteCall("backup", flowName));
            }

            public string CreateSampleCopyName(string sourceFlowName)
            {
                return SampleCopyName;
            }

            public readonly struct WriteCall
            {
                public WriteCall(string kind, string name)
                {
                    Kind = kind;
                    Name = name;
                }

                public string Kind { get; }

                public string Name { get; }
            }
        }

        private sealed class FakeFlowScriptValidator : IFlowScriptValidator
        {
            public List<string> CheckedFlowNames { get; } = new List<string>();

            public FlowScriptValidationResult Result { get; set; } = FlowScriptValidationResult.Success();

            public FlowScriptValidationResult CheckAll(ProcessFlowDefinition flow, string flowName)
            {
                CheckedFlowNames.Add(flowName);
                return Result;
            }
        }

        private sealed class FakeOperationLogger : IOperationLogger
        {
            public List<string> Messages { get; } = new List<string>();

            public void Operate(string message)
            {
                Messages.Add(message);
            }
        }

        private sealed class FakeFlowChangePublisher : IFlowChangePublisher
        {
            public List<string> Messages { get; } = new List<string>();

            public void PublishSaved(string flowName)
            {
                Messages.Add($"{flowName}@UcFlow");
            }
        }
    }
}
