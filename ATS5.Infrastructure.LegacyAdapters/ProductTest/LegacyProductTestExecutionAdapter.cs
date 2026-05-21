using System;
using System.Threading;
using System.Threading.Tasks;
using ATS5.Application.ProductTest;
using ATS5.Infrastructure.LegacyAdapters.Runtime;

namespace ATS5.Infrastructure.LegacyAdapters.ProductTest
{
    /// <summary>
    /// Guarded placeholder for legacy ProductTest execution until a WPF session owner supplies UI-thread and PLC parity.
    /// </summary>
    public sealed class LegacyProductTestExecutionAdapter : IProductTestExecutionAdapter
    {
        private const string GuardMessage =
            "Legacy ProductTest execution requires a UI-thread session owner with PLC and thread-state parity.";

        private readonly LegacyRuntimeContext _runtimeContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyProductTestExecutionAdapter"/> class.
        /// </summary>
        /// <param name="runtimeContext">Legacy runtime directory context.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="runtimeContext"/> is null.</exception>
        public LegacyProductTestExecutionAdapter(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        }

        /// <inheritdoc />
        public Task<ProductTestSessionResult> ExecuteAsync(
            ProductTestSessionRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_runtimeContext.Execute<ProductTestSessionResult>(ThrowGuardedExecution));
        }

        /// <inheritdoc />
        public Task SaveStopAsync(ushort channel, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _runtimeContext.Execute(ThrowGuardedStop);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task AbortAsync(ushort channel, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _runtimeContext.Execute(ThrowGuardedStop);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopCoreAsync(ushort channel, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _runtimeContext.Execute(ThrowGuardedStop);
            return Task.CompletedTask;
        }

        private static ProductTestSessionResult ThrowGuardedExecution()
        {
            throw new InvalidOperationException(GuardMessage);
        }

        private static void ThrowGuardedStop()
        {
            throw new InvalidOperationException(GuardMessage);
        }
    }
}
