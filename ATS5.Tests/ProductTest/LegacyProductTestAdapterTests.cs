using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using ATS5.Application.ProductTest;
using ATS5.Infrastructure.LegacyAdapters.ProductTest;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATS5.Tests.Infrastructure;
using Xunit;

namespace ATS5.Tests.ProductTest
{
    [Collection(LegacyTestCollections.SysCacheStaticState)]
    public sealed class LegacyProductTestAdapterTests : IDisposable
    {
        private static readonly Dictionary<string, object> MesResponses = new Dictionary<string, object>();

        private static readonly List<string> StaticCalls = new List<string>();

        private static bool _startTestResult = true;
        private static Exception? _startTestException;

        public LegacyProductTestAdapterTests()
        {
            ResetStaticState();
        }

        public void Dispose()
        {
            ResetStaticState();
        }

        [Fact]
        public async Task MesUploadGateway_UploadsThenUpdatesMesStatusOne_WhenCompileInitAndUploadSucceed()
        {
            var calls = new List<string>();
            var gateway = new RecordingProductTestMesUploadGateway(calls)
            {
                HasUploadDelegate = true,
                IsMesEnabledValue = true,
                UploadStatus = true
            };

            var result = await gateway.UploadAfterLocalSaveAsync(
                20260521010101UL,
                2,
                isCompileAndInit: true,
                CancellationToken.None);

            Assert.True(result);
            Assert.Equal(new[]
            {
                "UploadData:20260521010101:2",
                "UpdateMesInfo:20260521010101:2:1"
            }, calls);
        }

        [Fact]
        public async Task MesUploadGateway_UpdatesMesStatusZero_WhenUploadFailsOrMesDisabled()
        {
            var calls = new List<string>();
            var gateway = new RecordingProductTestMesUploadGateway(calls)
            {
                HasUploadDelegate = true,
                IsMesEnabledValue = false,
                UploadStatus = true
            };

            var result = await gateway.UploadAfterLocalSaveAsync(
                20260521010101UL,
                1,
                isCompileAndInit: true,
                CancellationToken.None);

            Assert.True(result);
            Assert.Equal(new[]
            {
                "UploadData:20260521010101:1",
                "UpdateMesInfo:20260521010101:1:0"
            }, calls);

            calls.Clear();
            gateway.IsMesEnabledValue = true;
            gateway.UploadStatus = false;

            result = await gateway.UploadAfterLocalSaveAsync(
                20260521010101UL,
                1,
                isCompileAndInit: true,
                CancellationToken.None);

            Assert.False(result);
            Assert.Equal(new[]
            {
                "UploadData:20260521010101:1",
                "UpdateMesInfo:20260521010101:1:0"
            }, calls);
        }

        [Fact]
        public async Task MesUploadGateway_SkipsUploadAndUsesWinFormsFallback_WhenUploadDelegateMissingOrCompileFails()
        {
            var calls = new List<string>();
            var gateway = new RecordingProductTestMesUploadGateway(calls)
            {
                HasUploadDelegate = false
            };

            var result = await gateway.UploadAfterLocalSaveAsync(
                20260521010101UL,
                1,
                isCompileAndInit: true,
                CancellationToken.None);

            Assert.True(result);
            Assert.Equal(new[] { "UpdateMesInfo:20260521010101:1:0" }, calls);

            calls.Clear();
            gateway.HasUploadDelegate = true;

            result = await gateway.UploadAfterLocalSaveAsync(
                0,
                1,
                isCompileAndInit: false,
                CancellationToken.None);

            Assert.True(result);
            Assert.Equal(new[] { "UpdateMesInfo:0:1:0" }, calls);
        }

        [Fact]
        public void AutomationGateway_CheckBarcode_CallsSendChannelBarcodeBeforeBarcodeCheck()
        {
            SetMesResponse("SendChannelBarcode", CreateMesResponse(true, "sent"));
            SetMesResponse("BarcodeCheck", CreateMesResponse(true, "checked"));
            SetSysCacheDelegate("SendChannelBarcode", CreateSysCacheDelegate("SendChannelBarcode"));
            SetSysCacheDelegate("BarcodeCheck", CreateSysCacheDelegate("BarcodeCheck"));
            var gateway = CreateAutomationGateway();

            var result = gateway.CheckBarcode(2, new[] { "SN1", "SN2" });

            Assert.True(result);
            Assert.Equal(new[]
            {
                "SendChannelBarcode:2:SN1|SN2",
                "BarcodeCheck:SN1|SN2"
            }, StaticCalls);
        }

        [Fact]
        public void AutomationGateway_CheckBarcode_StopsBeforeBarcodeRule_WhenSendChannelBarcodeFails()
        {
            SetMesResponse("SendChannelBarcode", CreateMesResponse(false, "bad channel"));
            SetMesResponse("BarcodeCheck", CreateMesResponse(true, "should not run"));
            SetSysCacheDelegate("SendChannelBarcode", CreateSysCacheDelegate("SendChannelBarcode"));
            SetSysCacheDelegate("BarcodeCheck", CreateSysCacheDelegate("BarcodeCheck"));
            var gateway = CreateAutomationGateway();

            var result = gateway.CheckBarcode(2, new[] { "SN1" });

            Assert.False(result);
            Assert.Equal(new[] { "SendChannelBarcode:2:SN1" }, StaticCalls);
        }

        [Fact]
        public void AutomationGateway_CheckBarcode_ReturnsFalse_WhenLegacyDelegateThrows()
        {
            SetMesResponse("SendChannelBarcode", new object());
            SetSysCacheDelegate("SendChannelBarcode", CreateSysCacheDelegate("SendChannelBarcode"));
            var gateway = CreateAutomationGateway();

            var result = gateway.CheckBarcode(2, new[] { "SN1" });

            Assert.False(result);
            Assert.Equal(new[] { "SendChannelBarcode:2:SN1" }, StaticCalls);
        }

        [Fact]
        public async Task AutomationGateway_PreTestMesCheck_CallsLegacyMesDelegatesInWinFormsOrder()
        {
            SetSysCacheDelegate("LoginMes", CreateSysCacheDelegate("LoginMes"));
            SetSysCacheDelegate("GetOrderNo", CreateSysCacheDelegate("GetOrderNo"));
            SetSysCacheDelegate("OrderNoCheck", CreateSysCacheDelegate("OrderNoCheck"));
            SetSysCacheDelegate("StationCheck", CreateSysCacheDelegate("StationCheck"));
            SetSysCacheDelegate("GetMESDownloadPara", CreateSysCacheDelegate("GetMESDownloadPara"));
            var gateway = CreateAutomationGateway();

            var result = await gateway.PreTestMesCheckAsync(
                "FlowA",
                new[] { "SN1", "SN2" },
                CancellationToken.None);

            Assert.True(result);
            Assert.Equal(new[]
            {
                "LoginMes",
                "GetOrderNo:SN1;SN2",
                "OrderNoCheck:SN1;SN2",
                "StationCheck:SN1;SN2",
                "GetMESDownloadPara:SN1;SN2:SysCache/Flows/FlowA.fw"
            }, StaticCalls);
        }

        [Fact]
        public async Task AutomationGateway_PreTestMesCheck_ReturnsFalse_WhenLegacyDelegateThrows()
        {
            SetMesResponse("LoginMes", new object());
            SetSysCacheDelegate("LoginMes", CreateSysCacheDelegate("LoginMes"));
            var gateway = CreateAutomationGateway();

            var result = await gateway.PreTestMesCheckAsync(
                "FlowA",
                new[] { "SN1" },
                CancellationToken.None);

            Assert.False(result);
            Assert.Equal(new[] { "LoginMes" }, StaticCalls);
        }

        [Fact]
        public async Task AutomationGateway_StartAutomation_ReturnsTrueWhenDelegateMissingAndFalseWhenDelegateThrows()
        {
            var gateway = CreateAutomationGateway();

            var result = await gateway.StartAutomationAsync(1, new[] { "SN1" }, CancellationToken.None);

            Assert.True(result);
            Assert.Empty(StaticCalls);

            _startTestException = new InvalidOperationException("busy");
            SetSysCacheDelegate("StartTest", CreateSysCacheDelegate("StartTest"));

            result = await gateway.StartAutomationAsync(1, new[] { "SN1" }, CancellationToken.None);

            Assert.False(result);
            Assert.Equal(new[] { "StartTest:1:SN1" }, StaticCalls);
        }

        [Fact]
        public async Task AutomationGateway_StartAutomation_ReturnsFalse_WhenLegacyDelegateThrowsUnexpectedException()
        {
            _startTestException = new FormatException("legacy failure");
            SetSysCacheDelegate("StartTest", CreateSysCacheDelegate("StartTest"));
            var gateway = CreateAutomationGateway();

            var result = await gateway.StartAutomationAsync(1, new[] { "SN1" }, CancellationToken.None);

            Assert.False(result);
            Assert.Equal(new[] { "StartTest:1:SN1" }, StaticCalls);
        }

        [Fact]
        public void AutomationGateway_SendsAutoTestCallbacksWithoutStartingAutoTestProvider()
        {
            SetSysCacheDelegate("AutoTestResult", CreateSysCacheDelegate("AutoTestResult"));
            SetSysCacheDelegate("AutoTestMESResult", CreateSysCacheDelegate("AutoTestMESResult"));
            var gateway = CreateAutomationGateway();

            gateway.SendAutoTestResult(2, "PASS");
            gateway.SendAutoTestMesResult(2, mesResult: false);

            Assert.Equal(new[]
            {
                "AutoTestResult:2:PASS",
                "AutoTestMESResult:2:False"
            }, StaticCalls);
        }

        [Fact]
        public void AutomationGateway_FlowExists_UsesLegacyRuntimeRootAndRelativeSysCachePath()
        {
            using (var tempRoot = new TemporaryRuntimeRoot())
            {
                Directory.CreateDirectory(Path.Combine(tempRoot.Path, "SysCache", "Flows"));
                File.WriteAllText(Path.Combine(tempRoot.Path, "SysCache", "Flows", "FlowA.fw"), "{}");
                var gateway = new LegacyProductTestAutomationGateway(
                    new LegacyRuntimeContext(new FixedRuntimePathProvider(tempRoot.Path)));

                Assert.True(gateway.FlowExists("FlowA"));
                Assert.False(gateway.FlowExists("Missing"));
            }
        }

        [Fact]
        public async Task ExecutionAdapter_IsGuardedUntilUiSessionOwnerIsProvided()
        {
            var adapter = new LegacyProductTestExecutionAdapter(
                new LegacyRuntimeContext(new FixedRuntimePathProvider(Environment.CurrentDirectory)));
            var request = new ProductTestSessionRequest(1, "旧流程", "FlowA", new[] { "SN1" });

            await Assert.ThrowsAsync<InvalidOperationException>(() => adapter.ExecuteAsync(request, CancellationToken.None));
            await Assert.ThrowsAsync<InvalidOperationException>(() => adapter.SaveStopAsync(1, CancellationToken.None));
            await Assert.ThrowsAsync<InvalidOperationException>(() => adapter.AbortAsync(1, CancellationToken.None));
            await Assert.ThrowsAsync<InvalidOperationException>(() => adapter.StopCoreAsync(1, CancellationToken.None));
        }

        private static LegacyProductTestAutomationGateway CreateAutomationGateway()
        {
            return new LegacyProductTestAutomationGateway(
                new LegacyRuntimeContext(new FixedRuntimePathProvider(Environment.CurrentDirectory)));
        }

        private static void ResetStaticState()
        {
            foreach (var fieldName in new[]
            {
                "SendChannelBarcode",
                "BarcodeCheck",
                "LoginMes",
                "GetOrderNo",
                "OrderNoCheck",
                "StationCheck",
                "GetMESDownloadPara",
                "StartTest",
                "AutoTestResult",
                "AutoTestMESResult"
            })
            {
                SetSysCacheDelegate(fieldName, null);
            }

            StaticCalls.Clear();
            MesResponses.Clear();
            _startTestResult = true;
            _startTestException = null;
        }

        private static void SetMesResponse(string fieldName, object response)
        {
            MesResponses[fieldName] = response;
        }

        private static void SetSysCacheDelegate(string name, Delegate? value)
        {
            var field = GetSysCacheType().GetField(name, BindingFlags.Public | BindingFlags.Static);
            if (field == null)
            {
                throw new MissingFieldException(GetSysCacheType().FullName, name);
            }

            field.SetValue(null, value);
        }

        private static Delegate CreateSysCacheDelegate(string fieldName)
        {
            var field = GetSysCacheType().GetField(fieldName, BindingFlags.Public | BindingFlags.Static)!;
            var invoke = field.FieldType.GetMethod("Invoke")!;
            var parameterTypes = invoke.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
            var method = new DynamicMethod(
                $"{fieldName}Delegate",
                invoke.ReturnType,
                parameterTypes,
                typeof(LegacyProductTestAdapterTests).Module,
                true);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldstr, fieldName);
            il.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            for (var i = 0; i < parameterTypes.Length; i++)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg, i);
                if (parameterTypes[i].IsValueType)
                {
                    il.Emit(OpCodes.Box, parameterTypes[i]);
                }

                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Call, typeof(LegacyProductTestAdapterTests).GetMethod(nameof(HandleSysCacheDelegate), BindingFlags.NonPublic | BindingFlags.Static)!);
            if (invoke.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Pop);
            }
            else if (invoke.ReturnType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, invoke.ReturnType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, invoke.ReturnType);
            }

            il.Emit(OpCodes.Ret);
            return method.CreateDelegate(field.FieldType);
        }

        private static object HandleSysCacheDelegate(string fieldName, object[] args)
        {
            switch (fieldName)
            {
                case "SendChannelBarcode":
                    StaticCalls.Add($"SendChannelBarcode:{args[0]}:{JoinBarcodes(args[1])}");
                    return GetMesResponse(fieldName);
                case "BarcodeCheck":
                    StaticCalls.Add($"BarcodeCheck:{JoinBarcodes(args[0])}");
                    return GetMesResponse(fieldName);
                case "LoginMes":
                    StaticCalls.Add("LoginMes");
                    return GetMesResponse(fieldName);
                case "GetOrderNo":
                case "OrderNoCheck":
                case "StationCheck":
                    StaticCalls.Add($"{fieldName}:{args[0]}");
                    return GetMesResponse(fieldName);
                case "GetMESDownloadPara":
                    StaticCalls.Add($"GetMESDownloadPara:{args[0]}:{args[1]}");
                    return GetMesResponse(fieldName);
                case "StartTest":
                    StaticCalls.Add($"StartTest:{args[0]}:{JoinBarcodes(args[1])}");
                    if (_startTestException != null)
                    {
                        throw _startTestException;
                    }

                    return _startTestResult;
                case "AutoTestResult":
                    StaticCalls.Add($"AutoTestResult:{args[0]}:{args[1]}");
                    return true;
                case "AutoTestMESResult":
                    StaticCalls.Add($"AutoTestMESResult:{args[0]}:{args[1]}");
                    return true;
                default:
                    throw new InvalidOperationException(fieldName);
            }
        }

        private static object GetMesResponse(string fieldName)
        {
            return MesResponses.TryGetValue(fieldName, out var response)
                ? response
                : CreateMesResponse(true, string.Empty);
        }

        private static string JoinBarcodes(object value)
        {
            return string.Join("|", ((IEnumerable<string>)value).ToArray());
        }

        private static object CreateMesResponse(bool status, string message)
        {
            var response = Activator.CreateInstance(GetMesResponseType())!;
            GetMesResponseType().GetProperty("Status")!.SetValue(response, status);
            GetMesResponseType().GetProperty("Msg")!.SetValue(response, message);
            return response;
        }

        private static Type GetSysCacheType()
        {
            return Type.GetType("ATSCore.SysCache, ATSCore", true)!;
        }

        private static Type GetMesResponseType()
        {
            return Type.GetType("ATSModel.MesRes, ATSModel", true)!;
        }

        private sealed class RecordingProductTestMesUploadGateway : LegacyProductTestMesUploadGateway
        {
            private readonly List<string> _calls;

            public RecordingProductTestMesUploadGateway(List<string> calls)
                : base(new LegacyRuntimeContext(new FixedRuntimePathProvider(Environment.CurrentDirectory)))
            {
                _calls = calls;
            }

            public bool HasUploadDelegate { get; set; }

            public bool IsMesEnabledValue { get; set; } = true;

            public bool UploadStatus { get; set; } = true;

            protected override bool HasUploadData()
            {
                return HasUploadDelegate;
            }

            protected override bool IsMesEnabled()
            {
                return IsMesEnabledValue;
            }

            protected override bool UploadData(ulong logGuid, ushort channel)
            {
                _calls.Add($"UploadData:{logGuid}:{channel}");
                return UploadStatus;
            }

            protected override void UpdateMesInfo(ulong logGuid, ushort channel, int uploadMesStatus)
            {
                _calls.Add($"UpdateMesInfo:{logGuid}:{channel}:{uploadMesStatus}");
            }
        }

        private sealed class FixedRuntimePathProvider : IRuntimePathProvider
        {
            public FixedRuntimePathProvider(string legacyRuntimeRoot)
            {
                LegacyRuntimeRoot = legacyRuntimeRoot;
            }

            public string LegacyRuntimeRoot { get; }
        }

        private sealed class TemporaryRuntimeRoot : IDisposable
        {
            public TemporaryRuntimeRoot()
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"ats5-product-test-{Guid.NewGuid():N}");
                Directory.CreateDirectory(Path);
            }

            public string Path { get; }

            public void Dispose()
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, recursive: true);
                }
            }
        }
    }
}
