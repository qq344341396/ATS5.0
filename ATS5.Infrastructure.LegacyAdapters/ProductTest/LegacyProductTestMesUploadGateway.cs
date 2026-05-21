using System;
using System.Threading;
using System.Threading.Tasks;
using ATS5.Application.ProductTest;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATSCommon;
using ATSCore;
using ATSCore.DBCore;

namespace ATS5.Infrastructure.LegacyAdapters.ProductTest
{
    /// <summary>
    /// Wraps the legacy ProductTest MES upload point without changing WinForms upload fallback rules.
    /// </summary>
    public class LegacyProductTestMesUploadGateway : IProductTestMesUploadGateway
    {
        private const string MesEnableKey = "MesEnable";
        private const string MesEnabledValue = "1";

        private readonly LegacyRuntimeContext _runtimeContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyProductTestMesUploadGateway"/> class.
        /// </summary>
        /// <param name="runtimeContext">Legacy runtime directory context.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="runtimeContext"/> is null.</exception>
        public LegacyProductTestMesUploadGateway(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        }

        /// <inheritdoc />
        public Task<bool> UploadAfterLocalSaveAsync(
            ulong logGuid,
            ushort channel,
            bool isCompileAndInit,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                _runtimeContext.Execute(
                    () =>
                    {
                        if (isCompileAndInit && HasUploadData())
                        {
                            var uploadStatus = UploadData(logGuid, channel);
                            UpdateMesInfo(logGuid, channel, IsMesEnabled() && uploadStatus ? 1 : 0);
                            return uploadStatus;
                        }

                        UpdateMesInfo(logGuid, channel, 0);
                        return true;
                    }));
        }

        /// <summary>
        /// Gets whether the legacy upload delegate is registered.
        /// </summary>
        /// <returns><c>true</c> when upload should be invoked.</returns>
        protected virtual bool HasUploadData()
        {
            return SysCache.UploadData != null;
        }

        /// <summary>
        /// Gets whether legacy MES upload status should be persisted as enabled.
        /// </summary>
        /// <returns><c>true</c> when legacy configuration value is enabled.</returns>
        protected virtual bool IsMesEnabled()
        {
            return ConfigHelper.GetValueApp(MesEnableKey) == MesEnabledValue;
        }

        /// <summary>
        /// Invokes the legacy upload delegate and returns its status.
        /// </summary>
        /// <param name="logGuid">Legacy log guid.</param>
        /// <param name="channel">Legacy channel.</param>
        /// <returns>The legacy upload result status.</returns>
        protected virtual bool UploadData(ulong logGuid, ushort channel)
        {
            return SysCache.UploadData!(logGuid, channel).Status;
        }

        /// <summary>
        /// Updates legacy index MES status using the original static core.
        /// </summary>
        /// <param name="logGuid">Legacy log guid.</param>
        /// <param name="channel">Legacy channel.</param>
        /// <param name="uploadMesStatus">Legacy UploadMesStatus value.</param>
        protected virtual void UpdateMesInfo(ulong logGuid, ushort channel, int uploadMesStatus)
        {
            IndexInfoCore.UpdateMesInfo(logGuid, channel, uploadMesStatus);
        }
    }
}
