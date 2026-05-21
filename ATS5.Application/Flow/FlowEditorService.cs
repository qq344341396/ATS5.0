using System;

namespace ATS5.Application.Flow
{
    public sealed class FlowEditorService
    {
        private readonly IFlowRepository _flowRepository;
        private readonly IFlowScriptValidator _flowScriptValidator;
        private readonly IOperationLogger _operationLogger;
        private readonly IFlowChangePublisher _flowChangePublisher;

        public FlowEditorService(
            IFlowRepository flowRepository,
            IFlowScriptValidator flowScriptValidator,
            IOperationLogger operationLogger,
            IFlowChangePublisher flowChangePublisher)
        {
            _flowRepository = flowRepository ?? throw new ArgumentNullException(nameof(flowRepository));
            _flowScriptValidator = flowScriptValidator ?? throw new ArgumentNullException(nameof(flowScriptValidator));
            _operationLogger = operationLogger ?? throw new ArgumentNullException(nameof(operationLogger));
            _flowChangePublisher = flowChangePublisher ?? throw new ArgumentNullException(nameof(flowChangePublisher));
        }

        public FlowOpenResult Open(string flowName)
        {
            return _flowRepository.Open(flowName);
        }

        public FlowOperationResult CheckAllScripts(ProcessFlowDefinition flow, string flowName)
        {
            var validation = _flowScriptValidator.CheckAll(flow, flowName);
            if (!validation.IsSuccess && validation.IsUserMessageComplete)
            {
                return FlowOperationResult.Fail(validation.Message);
            }

            return validation.IsSuccess
                ? FlowOperationResult.Success("脚本检验通过")
                : FlowOperationResult.Fail($"脚本检验失败：\r\n{validation.Message}");
        }

        public FlowOperationResult Save(ProcessFlowDefinition flow, string flowName, FlowSaveMode saveMode)
        {
            return FlowOperationResult.Fail("样板阶段只允许保存测试副本");
        }

        public FlowOperationResult SaveSampleCopy(ProcessFlowDefinition flow, string sourceFlowName)
        {
            var copyName = _flowRepository.CreateSampleCopyName(sourceFlowName);
            if (!IsSampleCopyName(sourceFlowName, copyName))
            {
                return FlowOperationResult.Fail("样板阶段只允许保存测试副本");
            }

            var result = SaveCopy(flow, sourceFlowName, copyName);
            return result.IsSuccess
                ? FlowOperationResult.Success($"保存成功：{copyName}.fw")
                : result;
        }

        private FlowOperationResult SaveCopy(ProcessFlowDefinition flow, string flowName)
        {
            return SaveCopy(flow, flowName, flowName);
        }

        private FlowOperationResult SaveCopy(ProcessFlowDefinition flow, string validationFlowName, string writeFlowName)
        {
            if (flow == null)
            {
                throw new ArgumentNullException(nameof(flow));
            }

            if (flow.Projects.Count == 0)
            {
                return FlowOperationResult.Fail("无测试项目信息，无需保存");
            }

            if (string.IsNullOrEmpty(flow.DevCfgName))
            {
                return FlowOperationResult.Fail("请先关联设备配置，再执行此操作");
            }

            if (!_flowRepository.ExistsDeviceConfig(flow.DevCfgName))
            {
                return FlowOperationResult.Fail("设备配置文件不存在，请检查！");
            }

            if (!_flowRepository.DeviceConfigHasContent(flow.DevCfgName))
            {
                return FlowOperationResult.Fail("设备配置文件异常，请检查！");
            }

            var validationResult = CheckAllScripts(flow, validationFlowName);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            _flowRepository.WriteFlow(writeFlowName, flow);
            _flowChangePublisher.PublishSaved(writeFlowName);
            return FlowOperationResult.Success("保存成功");
        }

        private static bool IsSampleCopyName(string sourceFlowName, string copyName)
        {
            return !string.IsNullOrEmpty(sourceFlowName)
                && !string.Equals(sourceFlowName, copyName, StringComparison.Ordinal)
                && copyName.StartsWith($"{sourceFlowName}-WpfSample-", StringComparison.Ordinal);
        }
    }
}
