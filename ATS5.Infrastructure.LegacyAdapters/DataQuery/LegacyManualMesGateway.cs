using System.Collections.Generic;
using ATS5.Application.DataQuery;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATSCore;

namespace ATS5.Infrastructure.LegacyAdapters.DataQuery
{
    public sealed class LegacyManualMesGateway : IManualMesGateway
    {
        private readonly LegacyRuntimeContext _runtimeContext;

        public LegacyManualMesGateway(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext;
        }

        public MesCallResult LoginMes()
        {
            return _runtimeContext.Execute(() => SysCache.LoginMes == null ? MesCallResult.Skipped() : ToResult(SysCache.LoginMes()));
        }

        public MesCallResult SendChannelBarcode(ushort channel, IReadOnlyList<string> barcodes)
        {
            return _runtimeContext.Execute(() => SysCache.SendChannelBarcode == null ? MesCallResult.Skipped() : ToResult(SysCache.SendChannelBarcode(channel, new List<string>(barcodes))));
        }

        public MesCallResult StationCheck(string barcode)
        {
            return _runtimeContext.Execute(() => SysCache.StationCheck == null ? MesCallResult.Skipped() : ToResult(SysCache.StationCheck(barcode)));
        }

        public MesCallResult UploadData(ulong logGuid, ushort channel)
        {
            return _runtimeContext.Execute(() => SysCache.UploadData == null ? MesCallResult.Skipped() : ToResult(SysCache.UploadData(logGuid, channel)));
        }

        private static MesCallResult ToResult(ATSModel.MesRes result)
        {
            return result.Status ? MesCallResult.Success(result.Msg ?? string.Empty) : MesCallResult.Fail(result.Msg ?? string.Empty);
        }
    }
}
