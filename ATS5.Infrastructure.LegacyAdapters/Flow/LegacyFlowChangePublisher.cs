using ATS5.Application.Flow;
using ATSCommon;
using ATSCore;

namespace ATS5.Infrastructure.LegacyAdapters.Flow
{
    public sealed class LegacyFlowChangePublisher : IFlowChangePublisher
    {
        public void PublishSaved(string flowName)
        {
            MessageHelper.Publish<ProductTestCore>(nameof(ProductTestCore), $"{flowName}@UcFlow");
        }
    }
}
