using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ATS5.Application.ProductTest;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATSCommon;
using ATSCore;
using ATSModel;
using static ATSModel.Enums;

namespace ATS5.Infrastructure.LegacyAdapters.ProductTest
{
    /// <summary>
    /// Wraps safe ProductTest legacy static delegates and file checks without starting real device execution.
    /// </summary>
    public sealed class LegacyProductTestAutomationGateway : IProductTestAutomationGateway
    {
        private readonly LegacyRuntimeContext _runtimeContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyProductTestAutomationGateway"/> class.
        /// </summary>
        /// <param name="runtimeContext">Legacy runtime directory context.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="runtimeContext"/> is null.</exception>
        public LegacyProductTestAutomationGateway(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        }

        /// <inheritdoc />
        public bool CheckRegister()
        {
            return _runtimeContext.Execute(() => SysCache.CheckRegister == null || SysCache.CheckRegister());
        }

        /// <inheritdoc />
        public bool CheckPencil(ProductTestSessionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return true;
        }

        /// <inheritdoc />
        public void StartReleaseSingleTest(string previousFlowName)
        {
        }

        /// <inheritdoc />
        public void LoadFlowTestInfo(ushort channel, string flowName)
        {
        }

        /// <inheritdoc />
        public void CheckSwtichConfig(string flowName)
        {
        }

        /// <inheritdoc />
        public bool CheckBarcode(ushort channel, IReadOnlyList<string> barcodes)
        {
            if (barcodes == null)
            {
                throw new ArgumentNullException(nameof(barcodes));
            }

            try
            {
                return _runtimeContext.Execute(
                    () =>
                    {
                        var barcodeList = new List<string>(barcodes);
                        if (SysCache.SendChannelBarcode != null && !SysCache.SendChannelBarcode(channel, barcodeList).Status)
                        {
                            return false;
                        }

                        return SysCache.BarcodeCheck == null || SysCache.BarcodeCheck(barcodeList).Status;
                    });
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public Task<bool> PreTestMesCheckAsync(
            string flowName,
            IReadOnlyList<string> barcodes,
            CancellationToken cancellationToken)
        {
            if (barcodes == null)
            {
                throw new ArgumentNullException(nameof(barcodes));
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return Task.FromResult(
                    _runtimeContext.Execute(
                        () =>
                        {
                            var barcodeText = string.Join(";", barcodes);
                            if (!CallMesStep(SysCache.LoginMes))
                            {
                                return false;
                            }

                            if (!CallMesStep(SysCache.GetOrderNo, barcodeText))
                            {
                                return false;
                            }

                            if (!CallMesStep(SysCache.OrderNoCheck, barcodeText))
                            {
                                return false;
                            }

                            if (!CallMesStep(SysCache.StationCheck, barcodeText))
                            {
                                return false;
                            }

                            if (SysCache.GetMESDownloadPara == null)
                            {
                                return true;
                            }

                            var flowPath = $"{SysCache.PathFlows}{flowName}.fw";
                            var mesResult = SysCache.GetMESDownloadPara(barcodeText, flowPath);
                            if (mesResult.MesData != null)
                            {
                                JsonHelper.WriteJsonFile((ATSModel.Flow)mesResult.MesData, flowPath);
                            }

                            return mesResult.Status;
                        }));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc />
        public void JoinReleaseSingleTest()
        {
        }

        /// <inheritdoc />
        public Task<bool> StartAutomationAsync(
            ushort channel,
            IReadOnlyList<string> barcodes,
            CancellationToken cancellationToken)
        {
            if (barcodes == null)
            {
                throw new ArgumentNullException(nameof(barcodes));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                _runtimeContext.Execute(
                    () =>
                    {
                        if (SysCache.StartTest == null)
                        {
                            return true;
                        }

                        try
                        {
                            return SysCache.StartTest(channel, new List<string>(barcodes));
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }));
        }

        /// <inheritdoc />
        public bool IsTestRunning(ushort channel)
        {
            return false;
        }

        /// <inheritdoc />
        public bool FlowExists(string flowName)
        {
            return _runtimeContext.Execute(() => File.Exists($"{SysCache.PathFlows}{flowName}.fw"));
        }

        /// <inheritdoc />
        public bool IsDeviceDebugOpen()
        {
            return false;
        }

        /// <inheritdoc />
        public void RestoreUi()
        {
        }

        /// <inheritdoc />
        public void FreshTestResComplete(IReadOnlyList<string> testResults, bool mesResult)
        {
        }

        /// <inheritdoc />
        public void SendAutoTestResult(ushort channel, string testResult)
        {
            _runtimeContext.Execute(
                () =>
                {
                    if (Enum.TryParse<TestResult>(testResult, out var parsedResult))
                    {
                        SysCache.AutoTestResult?.Invoke(channel, parsedResult);
                    }
                });
        }

        /// <inheritdoc />
        public void SendAutoTestMesResult(ushort channel, bool mesResult)
        {
            _runtimeContext.Execute(() => SysCache.AutoTestMESResult?.Invoke(channel, mesResult));
        }

        private static bool CallMesStep(Func<MesRes>? action)
        {
            return action == null || action().Status;
        }

        private static bool CallMesStep(Func<string, MesRes>? action, string value)
        {
            return action == null || action(value).Status;
        }
    }
}
