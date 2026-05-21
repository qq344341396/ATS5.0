using System;
using System.Collections.Generic;
using System.Linq;

namespace ATS5.Modules.ProductTest.ViewModels
{
    /// <summary>
    /// Represents the legacy barcode dialog result and status code.
    /// </summary>
    public sealed class BarcodeDialogResult
    {
        private BarcodeDialogResult(bool isConfirmed, int status, string? flowName, IReadOnlyList<string> barcodes)
        {
            IsConfirmed = isConfirmed;
            Status = status;
            FlowName = flowName;
            Barcodes = barcodes;
        }

        public bool IsConfirmed { get; }

        public int Status { get; }

        public string? FlowName { get; }

        public IReadOnlyList<string> Barcodes { get; }

        /// <summary>
        /// Creates a confirmed dialog result.
        /// </summary>
        /// <param name="flowName">The selected flow name.</param>
        /// <param name="barcodes">The entered barcodes.</param>
        /// <returns>A confirmed barcode dialog result.</returns>
        public static BarcodeDialogResult Confirmed(string flowName, IEnumerable<string> barcodes)
        {
            return new BarcodeDialogResult(true, 1, flowName, (barcodes ?? Array.Empty<string>()).ToList());
        }

        /// <summary>
        /// Creates a cancelled dialog result with the legacy status code.
        /// </summary>
        /// <param name="status">The legacy cancel status.</param>
        /// <returns>A cancelled barcode dialog result.</returns>
        public static BarcodeDialogResult Cancelled(int status)
        {
            return new BarcodeDialogResult(false, status, null, Array.Empty<string>());
        }
    }
}
