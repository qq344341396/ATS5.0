using System.Threading;
using System.Threading.Tasks;

namespace ATS5.Application.ProductTest
{
    /// <summary>
    /// Adapts the legacy product test execution and stop operations for application-layer orchestration.
    /// </summary>
    public interface IProductTestExecutionAdapter
    {
        /// <summary>
        /// Executes the legacy product test chain, including local index save, detail save, auto export, and core close.
        /// </summary>
        /// <param name="request">Session start request.</param>
        /// <param name="cancellationToken">Token used to cancel the execution wrapper.</param>
        /// <returns>The result required by MES upload and AutoTest callbacks.</returns>
        Task<ProductTestSessionResult> ExecuteAsync(ProductTestSessionRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// Saves the legacy STOP result when a current log guid exists.
        /// </summary>
        /// <param name="channel">Legacy test channel number.</param>
        /// <param name="cancellationToken">Token used to cancel the stop-save wrapper.</param>
        /// <returns>A task that represents the save operation.</returns>
        Task SaveStopAsync(ushort channel, CancellationToken cancellationToken);

        /// <summary>
        /// Aborts the legacy ProductTest background thread.
        /// </summary>
        /// <param name="channel">Legacy test channel number.</param>
        /// <param name="cancellationToken">Token used to cancel the abort wrapper.</param>
        /// <returns>A task that represents the abort operation.</returns>
        Task AbortAsync(ushort channel, CancellationToken cancellationToken);

        /// <summary>
        /// Closes the legacy product test devices and dynamic engine.
        /// </summary>
        /// <param name="channel">Legacy test channel number.</param>
        /// <param name="cancellationToken">Token used to cancel the close wrapper.</param>
        /// <returns>A task that represents the close operation.</returns>
        Task StopCoreAsync(ushort channel, CancellationToken cancellationToken);
    }
}
