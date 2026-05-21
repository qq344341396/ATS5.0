namespace ATS5.Application.DataQuery
{
    public sealed class DataQueryCriteria
    {
        public DataQueryCriteria(ulong startLogGuid, ulong endLogGuid, string barcode, ushort channel, string flowName)
        {
            StartLogGuid = startLogGuid;
            EndLogGuid = endLogGuid;
            Barcode = barcode;
            Channel = channel;
            FlowName = flowName;
        }

        public ulong StartLogGuid { get; }

        public ulong EndLogGuid { get; }

        public string Barcode { get; }

        public ushort Channel { get; }

        public string FlowName { get; }
    }
}
