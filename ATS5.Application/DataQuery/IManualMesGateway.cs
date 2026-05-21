using System.Collections.Generic;

namespace ATS5.Application.DataQuery
{
    public interface IManualMesGateway
    {
        MesCallResult LoginMes();

        MesCallResult SendChannelBarcode(ushort channel, IReadOnlyList<string> barcodes);

        MesCallResult StationCheck(string barcode);

        MesCallResult UploadData(ulong logGuid, ushort channel);
    }
}
