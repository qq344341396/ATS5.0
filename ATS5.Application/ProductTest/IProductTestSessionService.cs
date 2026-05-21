using System.Threading;
using System.Threading.Tasks;

namespace ATS5.Application.ProductTest
{
    /// <summary>
    /// Coordinates a ProductTest session while preserving the legacy WinForms execution order.
    /// </summary>
    public interface IProductTestSessionService
    {
        /// <summary>
        /// Starts a ProductTest session using the legacy pre-check, execution, save, upload, and callback order.
        /// </summary>
        /// <param name="request">Session start request.</param>
        /// <param name="cancellationToken">Token used to cancel asynchronous gateway calls.</param>
        /// <returns>A task that represents the start operation.</returns>
        Task StartAsync(ProductTestSessionRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// Stops a ProductTest session while preserving STOP save before abort and core close.
        /// </summary>
        /// <param name="channel">Legacy test channel number.</param>
        /// <param name="cancellationToken">Token used to cancel asynchronous adapter calls.</param>
        /// <returns>A task that represents the stop operation.</returns>
        Task StopAsync(ushort channel, CancellationToken cancellationToken);
    }
}
