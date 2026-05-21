using System;
using System.Collections.Generic;
using System.Linq;

namespace ATS5.Modules.ProductTest.ViewModels
{
    /// <summary>
    /// Describes the legacy barcode dialog context for one product-test channel.
    /// </summary>
    public sealed class BarcodeDialogRequest
    {
        /// <summary>
        /// Initializes a new barcode dialog request.
        /// </summary>
        /// <param name="channelNumber">The one-based channel number.</param>
        /// <param name="flowName">The current flow name.</param>
        /// <param name="barcodeCount">The initial barcode count.</param>
        /// <param name="barcodeNullable">Whether empty barcodes are allowed.</param>
        /// <param name="availableFlowNames">The available legacy flow names.</param>
        public BarcodeDialogRequest(
            int channelNumber,
            string? flowName,
            int barcodeCount,
            bool barcodeNullable,
            IEnumerable<string> availableFlowNames)
        {
            ChannelNumber = channelNumber;
            FlowName = flowName;
            BarcodeCount = barcodeCount < 1 ? 1 : barcodeCount;
            BarcodeNullable = barcodeNullable;
            AvailableFlowNames = (availableFlowNames ?? Array.Empty<string>()).ToList();
        }

        public int ChannelNumber { get; }

        public string? FlowName { get; }

        public int BarcodeCount { get; }

        public bool BarcodeNullable { get; }

        public IReadOnlyList<string> AvailableFlowNames { get; }
    }
}
