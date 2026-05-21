namespace ATS5.Application.Flow
{
    public interface IFlowRepository
    {
        FlowOpenResult Open(string flowName);

        bool ExistsDeviceConfig(string deviceConfigName);

        bool DeviceConfigHasContent(string deviceConfigName);

        string Serialize(ProcessFlowDefinition flow);

        void WriteFlow(string flowName, ProcessFlowDefinition flow);

        void WriteBackup(string flowName, string json);

        string CreateSampleCopyName(string sourceFlowName);
    }
}
