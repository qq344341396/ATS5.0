namespace ATS5.Application.Flow
{
    public interface IFlowScriptValidator
    {
        FlowScriptValidationResult CheckAll(ProcessFlowDefinition flow, string flowName);
    }
}
