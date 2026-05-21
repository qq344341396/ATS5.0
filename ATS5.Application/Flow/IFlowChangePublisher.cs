namespace ATS5.Application.Flow
{
    public interface IFlowChangePublisher
    {
        void PublishSaved(string flowName);
    }
}
