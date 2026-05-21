using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ATS5.Application.ProductTest
{
    /// <summary>
    /// Orchestrates ProductTest start and stop operations while preserving the legacy WinForms order.
    /// </summary>
    public sealed class ProductTestSessionService : IProductTestSessionService
    {
        private const string PassResult = "PASS";
        private const string FailResult = "FAIL";

        private readonly IProductTestExecutionAdapter _executionAdapter;
        private readonly IProductTestMesUploadGateway _mesUploadGateway;
        private readonly IProductTestAutomationGateway _automationGateway;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductTestSessionService"/> class.
        /// </summary>
        /// <param name="executionAdapter">Adapter for legacy product test execution.</param>
        /// <param name="mesUploadGateway">Gateway for legacy ProductTest MES upload and status update.</param>
        /// <param name="automationGateway">Gateway for legacy pre-checks, UI state, and AutoTest callbacks.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public ProductTestSessionService(
            IProductTestExecutionAdapter executionAdapter,
            IProductTestMesUploadGateway mesUploadGateway,
            IProductTestAutomationGateway automationGateway)
        {
            _executionAdapter = executionAdapter ?? throw new ArgumentNullException(nameof(executionAdapter));
            _mesUploadGateway = mesUploadGateway ?? throw new ArgumentNullException(nameof(mesUploadGateway));
            _automationGateway = automationGateway ?? throw new ArgumentNullException(nameof(automationGateway));
        }

        /// <inheritdoc />
        public async Task StartAsync(ProductTestSessionRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!_automationGateway.CheckRegister())
            {
                return;
            }

            if (!_automationGateway.CheckPencil(request))
            {
                return;
            }

            _automationGateway.StartReleaseSingleTest(request.PreviousFlowName);
            _automationGateway.LoadFlowTestInfo(request.Channel, request.FlowName);
            _automationGateway.CheckSwtichConfig(request.FlowName);

            if (!CanContinueAfterBarcodeCheck(request))
            {
                return;
            }

            if (!await CanContinueAfterPreTestMesCheckAsync(request, cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            _automationGateway.JoinReleaseSingleTest();

            if (!await CanContinueAfterAutomationStartAsync(request, cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            if (!CanContinueAfterDuplicateStartCheck(request.Channel))
            {
                return;
            }

            if (!CanCreateLegacyExecutionCore(request.FlowName))
            {
                return;
            }

            var result = await _executionAdapter.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
            var mesResult = await _mesUploadGateway.UploadAfterLocalSaveAsync(
                result.LogGuid,
                result.Channel,
                result.IsCompileAndInit,
                cancellationToken).ConfigureAwait(false);

            _automationGateway.FreshTestResComplete(result.TestResults, mesResult);
            _automationGateway.SendAutoTestResult(result.Channel, DetermineRawTestResult(result.TestResults));
            _automationGateway.SendAutoTestMesResult(result.Channel, mesResult);
        }

        /// <inheritdoc />
        public async Task StopAsync(ushort channel, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _executionAdapter.SaveStopAsync(channel, cancellationToken).ConfigureAwait(false);
            await _executionAdapter.AbortAsync(channel, cancellationToken).ConfigureAwait(false);
            await _executionAdapter.StopCoreAsync(channel, cancellationToken).ConfigureAwait(false);
        }

        private bool CanContinueAfterBarcodeCheck(ProductTestSessionRequest request)
        {
            if (_automationGateway.CheckBarcode(request.Channel, request.Barcodes))
            {
                return true;
            }

            _automationGateway.RestoreUi();
            return false;
        }

        private async Task<bool> CanContinueAfterPreTestMesCheckAsync(
            ProductTestSessionRequest request,
            CancellationToken cancellationToken)
        {
            var canContinue = await _automationGateway.PreTestMesCheckAsync(
                request.FlowName,
                request.Barcodes,
                cancellationToken).ConfigureAwait(false);

            if (canContinue)
            {
                return true;
            }

            _automationGateway.RestoreUi();
            return false;
        }

        private async Task<bool> CanContinueAfterAutomationStartAsync(
            ProductTestSessionRequest request,
            CancellationToken cancellationToken)
        {
            bool canContinue;
            try
            {
                canContinue = await _automationGateway.StartAutomationAsync(
                    request.Channel,
                    request.Barcodes,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                _automationGateway.RestoreUi();
                return false;
            }

            if (canContinue)
            {
                return true;
            }

            _automationGateway.RestoreUi();
            return false;
        }

        private bool CanContinueAfterDuplicateStartCheck(ushort channel)
        {
            if (!_automationGateway.IsTestRunning(channel))
            {
                return true;
            }

            _automationGateway.RestoreUi();
            return false;
        }

        private bool CanCreateLegacyExecutionCore(string flowName)
        {
            if (!_automationGateway.FlowExists(flowName))
            {
                _automationGateway.RestoreUi();
                return false;
            }

            if (!_automationGateway.IsDeviceDebugOpen())
            {
                return true;
            }

            _automationGateway.RestoreUi();
            return false;
        }

        private static string DetermineRawTestResult(IReadOnlyCollection<string> testResults)
        {
            return testResults.Count > 0 && testResults.All(IsPassResult) ? PassResult : FailResult;
        }

        private static bool IsPassResult(string testResult)
        {
            return string.Equals(testResult, PassResult, StringComparison.Ordinal);
        }
    }
}
