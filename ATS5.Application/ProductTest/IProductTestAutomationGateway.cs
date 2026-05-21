using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ATS5.Application.ProductTest
{
    /// <summary>
    /// Provides ProductTest UI, pre-check, and AutoTest callback boundaries without exposing legacy static state.
    /// </summary>
    public interface IProductTestAutomationGateway
    {
        /// <summary>
        /// Checks whether the current station registration allows testing.
        /// </summary>
        /// <returns><c>true</c> when testing may continue.</returns>
        bool CheckRegister();

        /// <summary>
        /// Checks the pencil warning and stop rules for the selected flow.
        /// </summary>
        /// <param name="request">Session start request.</param>
        /// <returns><c>true</c> when testing may continue.</returns>
        bool CheckPencil(ProductTestSessionRequest request);

        /// <summary>
        /// Starts the legacy single-test release background operation using the previous flow name.
        /// </summary>
        /// <param name="previousFlowName">Flow name active before the barcode dialog accepted the new flow.</param>
        void StartReleaseSingleTest(string previousFlowName);

        /// <summary>
        /// Loads and projects legacy flow test statistics for the selected channel and flow.
        /// </summary>
        /// <param name="channel">Legacy test channel number.</param>
        /// <param name="flowName">Selected flow name.</param>
        void LoadFlowTestInfo(ushort channel, string flowName);

        /// <summary>
        /// Runs the legacy matrix switch configuration warning check.
        /// </summary>
        /// <param name="flowName">Selected flow name.</param>
        void CheckSwtichConfig(string flowName);

        /// <summary>
        /// Runs the legacy barcode handoff and barcode rule checks.
        /// </summary>
        /// <param name="channel">Legacy test channel number.</param>
        /// <param name="barcodes">Barcode values passed to the legacy test chain.</param>
        /// <returns><c>true</c> when testing may continue.</returns>
        bool CheckBarcode(ushort channel, IReadOnlyList<string> barcodes);

        /// <summary>
        /// Runs the legacy pre-test MES checks before releasing single-test resources.
        /// </summary>
        /// <param name="flowName">Selected flow name.</param>
        /// <param name="barcodes">Barcode values passed to the legacy test chain.</param>
        /// <param name="cancellationToken">Token used to cancel asynchronous MES wrappers.</param>
        /// <returns><c>true</c> when testing may continue.</returns>
        Task<bool> PreTestMesCheckAsync(
            string flowName,
            IReadOnlyList<string> barcodes,
            CancellationToken cancellationToken);

        /// <summary>
        /// Waits for the legacy single-test release operation to finish.
        /// </summary>
        void JoinReleaseSingleTest();

        /// <summary>
        /// Invokes the legacy automation start boundary before creating the execution core.
        /// </summary>
        /// <param name="channel">Legacy test channel number.</param>
        /// <param name="barcodes">Barcode values passed to the legacy test chain.</param>
        /// <param name="cancellationToken">Token used to cancel the automation wrapper.</param>
        /// <returns><c>true</c> when testing may continue.</returns>
        Task<bool> StartAutomationAsync(
            ushort channel,
            IReadOnlyList<string> barcodes,
            CancellationToken cancellationToken);

        /// <summary>
        /// Checks whether the selected channel is already running a ProductTest session.
        /// </summary>
        /// <param name="channel">Legacy test channel number.</param>
        /// <returns><c>true</c> when a duplicate start must be rejected.</returns>
        bool IsTestRunning(ushort channel);

        /// <summary>
        /// Checks whether the selected legacy flow file exists.
        /// </summary>
        /// <param name="flowName">Selected flow name.</param>
        /// <returns><c>true</c> when the flow exists.</returns>
        bool FlowExists(string flowName);

        /// <summary>
        /// Checks whether the legacy device debug form is open.
        /// </summary>
        /// <returns><c>true</c> when the debug UI is open and ProductTest must not start.</returns>
        bool IsDeviceDebugOpen();

        /// <summary>
        /// Restores the legacy ProductTest command UI after a pre-start failure.
        /// </summary>
        void RestoreUi();

        /// <summary>
        /// Projects the legacy FreshTestRes complete callback after MES upload handling.
        /// </summary>
        /// <param name="testResults">Raw per-barcode test results.</param>
        /// <param name="mesResult">MES result returned by the upload gateway.</param>
        void FreshTestResComplete(IReadOnlyList<string> testResults, bool mesResult);

        /// <summary>
        /// Sends the legacy AutoTestResult callback.
        /// </summary>
        /// <param name="channel">Legacy test channel number.</param>
        /// <param name="testResult">Raw PASS or FAIL result independent of the MES result.</param>
        void SendAutoTestResult(ushort channel, string testResult);

        /// <summary>
        /// Sends the legacy AutoTestMESResult callback.
        /// </summary>
        /// <param name="channel">Legacy test channel number.</param>
        /// <param name="mesResult">MES result returned by the upload gateway.</param>
        void SendAutoTestMesResult(ushort channel, bool mesResult);
    }
}
