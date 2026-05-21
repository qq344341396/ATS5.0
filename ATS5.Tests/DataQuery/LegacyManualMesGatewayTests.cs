using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ATS5.Infrastructure.LegacyAdapters.DataQuery;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATS5.Tests.Infrastructure;
using Xunit;

namespace ATS5.Tests.DataQuery
{
    [Collection(LegacyTestCollections.SysCacheStaticState)]
    public sealed class LegacyManualMesGatewayTests : IDisposable
    {
        private static ulong _actualLogGuid;
        private static ushort _actualUploadChannel;
        private static ushort _actualBarcodeChannel;
        private static IReadOnlyList<string>? _actualBarcodes;
        private static object? _uploadDataResponse;
        private static object? _sendChannelBarcodeResponse;

        public LegacyManualMesGatewayTests()
        {
            ResetDelegates();
        }

        public void Dispose()
        {
            ResetDelegates();
        }

        [Fact]
        public void UploadData_ReturnsSkipped_WhenLegacyDelegateIsMissing()
        {
            var gateway = CreateGateway();

            var result = gateway.UploadData(20260520010101UL, 2);

            Assert.True(result.Status);
            Assert.False(result.IsHandled);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void UploadData_InvokesLegacyDelegate_WithOriginalLogGuidAndChannel()
        {
            _uploadDataResponse = CreateMesResponse(false, "NG");
            SetSysCacheDelegate("UploadData", CreateUploadDataDelegate());
            var gateway = CreateGateway();

            var result = gateway.UploadData(20260520010101UL, 2);

            Assert.True(result.IsHandled);
            Assert.False(result.Status);
            Assert.Equal("NG", result.Message);
            Assert.Equal(20260520010101UL, _actualLogGuid);
            Assert.Equal((ushort)2, _actualUploadChannel);
        }

        [Fact]
        public void SendChannelBarcode_InvokesLegacyDelegate_WithOriginalChannelAndBarcodeList()
        {
            _sendChannelBarcodeResponse = CreateMesResponse(true, "OK");
            SetSysCacheDelegate("SendChannelBarcode", CreateSendChannelBarcodeDelegate());
            var gateway = CreateGateway();

            var result = gateway.SendChannelBarcode(2, new[] { "SN1", "SN2" });

            Assert.True(result.IsHandled);
            Assert.True(result.Status);
            Assert.Equal("OK", result.Message);
            Assert.Equal((ushort)2, _actualBarcodeChannel);
            Assert.Equal(new[] { "SN1", "SN2" }, _actualBarcodes);
        }

        private static LegacyManualMesGateway CreateGateway()
        {
            var runtimeContext = new LegacyRuntimeContext(new CurrentDirectoryRuntimePathProvider());
            return new LegacyManualMesGateway(runtimeContext);
        }

        private static void ResetDelegates()
        {
            SetSysCacheDelegate("LoginMes", null);
            SetSysCacheDelegate("SendChannelBarcode", null);
            SetSysCacheDelegate("StationCheck", null);
            SetSysCacheDelegate("UploadData", null);
            _actualLogGuid = 0;
            _actualUploadChannel = 0;
            _actualBarcodeChannel = 0;
            _actualBarcodes = null;
            _uploadDataResponse = null;
            _sendChannelBarcodeResponse = null;
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

        private static Delegate CreateUploadDataDelegate()
        {
            var field = GetSysCacheType().GetField("UploadData", BindingFlags.Public | BindingFlags.Static)!;
            var invoke = field.FieldType.GetMethod("Invoke")!;
            var parameterTypes = new[]
            {
                typeof(ulong),
                typeof(ushort)
            };
            var method = new DynamicMethod(
                "UploadDataDelegate",
                invoke.ReturnType,
                parameterTypes,
                typeof(LegacyManualMesGatewayTests).Module,
                true);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, typeof(LegacyManualMesGatewayTests).GetMethod(nameof(HandleUploadData), BindingFlags.NonPublic | BindingFlags.Static)!);
            il.Emit(OpCodes.Castclass, invoke.ReturnType);
            il.Emit(OpCodes.Ret);
            return method.CreateDelegate(field.FieldType);
        }

        private static Delegate CreateSendChannelBarcodeDelegate()
        {
            var field = GetSysCacheType().GetField("SendChannelBarcode", BindingFlags.Public | BindingFlags.Static)!;
            var invoke = field.FieldType.GetMethod("Invoke")!;
            var parameterTypes = new[]
            {
                typeof(ushort),
                typeof(List<string>)
            };
            var method = new DynamicMethod(
                "SendChannelBarcodeDelegate",
                invoke.ReturnType,
                parameterTypes,
                typeof(LegacyManualMesGatewayTests).Module,
                true);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, typeof(LegacyManualMesGatewayTests).GetMethod(nameof(HandleSendChannelBarcode), BindingFlags.NonPublic | BindingFlags.Static)!);
            il.Emit(OpCodes.Castclass, invoke.ReturnType);
            il.Emit(OpCodes.Ret);
            return method.CreateDelegate(field.FieldType);
        }

        private static object HandleUploadData(ulong logGuid, ushort channel)
        {
            _actualLogGuid = logGuid;
            _actualUploadChannel = channel;
            return _uploadDataResponse ?? CreateMesResponse(true, string.Empty);
        }

        private static object HandleSendChannelBarcode(ushort channel, List<string> barcodes)
        {
            _actualBarcodeChannel = channel;
            _actualBarcodes = barcodes;
            return _sendChannelBarcodeResponse ?? CreateMesResponse(true, string.Empty);
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

        private sealed class CurrentDirectoryRuntimePathProvider : IRuntimePathProvider
        {
            public string LegacyRuntimeRoot => Environment.CurrentDirectory;
        }
    }
}
