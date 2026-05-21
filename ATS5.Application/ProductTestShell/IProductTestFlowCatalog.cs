using System.Collections.Generic;

namespace ATS5.Application.ProductTestShell
{
    /// <summary>
    /// Provides legacy product-test flow names for the barcode dialog.
    /// </summary>
    public interface IProductTestFlowCatalog
    {
        /// <summary>
        /// Gets available legacy `.fw` flow names without file extensions.
        /// </summary>
        /// <returns>The available flow names.</returns>
        IReadOnlyList<string> GetFlowNames();
    }
}
