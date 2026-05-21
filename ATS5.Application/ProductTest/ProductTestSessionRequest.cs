using System;
using System.Collections.Generic;
using System.Linq;

namespace ATS5.Application.ProductTest
{
    /// <summary>
    /// Describes a legacy ProductTest start request without binding the application layer to WinForms state.
    /// </summary>
    public sealed class ProductTestSessionRequest
    {
        /// <summary>
        /// Gets the legacy test channel number.
        /// </summary>
        public ushort Channel { get; }

        /// <summary>
        /// Gets the flow name that was active before the current start request.
        /// </summary>
        public string PreviousFlowName { get; }

        /// <summary>
        /// Gets the flow name selected for this test run.
        /// </summary>
        public string FlowName { get; }

        /// <summary>
        /// Gets the barcode values passed to the legacy ProductTest flow.
        /// </summary>
        public IReadOnlyList<string> Barcodes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductTestSessionRequest"/> class.
        /// </summary>
        /// <param name="channel">Legacy test channel number.</param>
        /// <param name="previousFlowName">Flow name that was active before the barcode dialog accepted a new flow.</param>
        /// <param name="flowName">Flow name selected for the test run.</param>
        /// <param name="barcodes">Barcode values passed to the legacy test chain.</param>
        /// <exception cref="ArgumentNullException">Thrown when a required value is null.</exception>
        public ProductTestSessionRequest(
            ushort channel,
            string previousFlowName,
            string flowName,
            IEnumerable<string> barcodes)
        {
            PreviousFlowName = previousFlowName ?? throw new ArgumentNullException(nameof(previousFlowName));
            FlowName = flowName ?? throw new ArgumentNullException(nameof(flowName));
            Barcodes = (barcodes ?? throw new ArgumentNullException(nameof(barcodes))).ToArray();
            Channel = channel;
        }
    }
}
