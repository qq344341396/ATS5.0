using System;
using System.Collections.Generic;
using ATS5.Application.DataQuery;
using ATS5.Application.ProductTestShell;
using ATS5.Modules.ProductTest.ViewModels;
using Xunit;

namespace ATS5.Tests.ProductTestShell
{
    public sealed class ProductTestShellViewModelTests
    {
        [Fact]
        public void Load_CreatesTwoIndependentChannelsFromChannelNum()
        {
            var viewModel = CreateViewModel(channelNum: "2");

            Assert.Equal(2, viewModel.Channels.Count);
            Assert.Equal("通道1", viewModel.Channels[0].Title);
            Assert.Equal("通道2", viewModel.Channels[1].Title);
            Assert.NotSame(viewModel.Channels[0], viewModel.Channels[1]);
            Assert.Same(viewModel.Channels[0], viewModel.SelectedChannel);
            Assert.True(viewModel.ExecuteCommand.CanExecute());
            Assert.False(viewModel.PauseCommand.CanExecute());
            Assert.False(viewModel.StopCommand.CanExecute());
        }

        [Fact]
        public void ChannelTitle_FollowsWinFormsFreshPageTitleRules()
        {
            var channel = new ProductTestChannelViewModel(1);

            channel.UpdateTitle(ProductTestShellStatus.Start, Array.Empty<string>(), mesResult: false);
            Assert.Equal("通道1【Testing】", channel.Title);

            channel.UpdateTitle(ProductTestShellStatus.Complete, new[] { "PASS", "PASS" }, mesResult: true);
            Assert.Equal("通道1【PASS】", channel.Title);

            channel.UpdateTitle(ProductTestShellStatus.Complete, new[] { "PASS", "FAIL" }, mesResult: true);
            Assert.Equal("通道1【FAIL】", channel.Title);

            channel.UpdateTitle(ProductTestShellStatus.Stop, Array.Empty<string>(), mesResult: false);
            Assert.Equal("通道1【FAIL】", channel.Title);
        }

        [Fact]
        public void ExecuteCommand_UsesBarcodeDialogResultWithoutStartingRealTest()
        {
            var dialogService = new FakeBarcodeDialogService
            {
                Result = BarcodeDialogResult.Confirmed("测试1", new[] { "SN001", "SN002" })
            };
            var viewModel = CreateViewModel("2", dialogService);

            viewModel.ExecuteCommand.Execute();

            var request = Assert.Single(dialogService.Requests);
            Assert.Equal(1, request.ChannelNumber);
            Assert.Equal(new[] { "测试1", "测试2" }, request.AvailableFlowNames);
            Assert.Equal("测试1", viewModel.SelectedChannel!.FlowName);
            Assert.Equal(2, viewModel.SelectedChannel.BarcodeCount);
            Assert.Equal(new[] { "SN001", "SN002" }, viewModel.SelectedChannel.Barcodes);
            Assert.True(viewModel.ExecuteCommand.CanExecute());
            Assert.False(viewModel.PauseCommand.CanExecute());
            Assert.False(viewModel.StopCommand.CanExecute());
        }

        [Fact]
        public void BarcodeDialogViewModel_EnterMovesFocusThenConfirmsLastBarcode()
        {
            var viewModel = CreateBarcodeDialogViewModel("测试1", barcodeCount: 2, barcodeNullable: false);
            viewModel.BarcodeInputs[0].Value = "SN001";
            viewModel.BarcodeInputs[1].Value = "SN002";

            var firstAction = viewModel.HandleEnter(0);
            var secondAction = viewModel.HandleEnter(1);

            Assert.Equal(BarcodeEnterAction.FocusNext, firstAction);
            Assert.Equal(1, viewModel.FocusedBarcodeIndex);
            Assert.Equal(BarcodeEnterAction.Confirmed, secondAction);
            Assert.True(viewModel.Result.IsConfirmed);
            Assert.Equal(1, viewModel.Status);
            Assert.Equal(new[] { "SN001", "SN002" }, viewModel.Result.Barcodes);
        }

        [Fact]
        public void BarcodeDialogViewModel_BarcodeCountChangeRebuildsDynamicInputs()
        {
            var viewModel = CreateBarcodeDialogViewModel("测试1", barcodeCount: 1, barcodeNullable: false);

            viewModel.BarcodeCount = 3;

            Assert.Equal(3, viewModel.BarcodeInputs.Count);
            Assert.Equal(new[] { "条码1", "条码2", "条码3" }, GetLabels(viewModel));

            viewModel.BarcodeCount = 0;

            Assert.Single(viewModel.BarcodeInputs);
            Assert.Equal("条码", viewModel.BarcodeInputs[0].Label);
        }

        [Fact]
        public void BarcodeDialogViewModel_ValidatesLegacyRequiredFields()
        {
            var viewModel = CreateBarcodeDialogViewModel(initialFlowName: null, barcodeCount: 1, barcodeNullable: false);

            viewModel.ConfirmCommand.Execute();
            Assert.Equal("流程文件不能为空", viewModel.ErrorMessage);
            Assert.False(viewModel.Result.IsConfirmed);

            viewModel.SelectedFlowName = "测试1";
            viewModel.BarcodeInputs[0].Value = "   ";
            viewModel.ConfirmCommand.Execute();
            Assert.Equal("条码不能为空", viewModel.ErrorMessage);
            Assert.False(viewModel.Result.IsConfirmed);

            viewModel.BarcodeInputs[0].Value = "SN001";
            viewModel.ConfirmCommand.Execute();
            Assert.True(viewModel.Result.IsConfirmed);
            Assert.Equal(1, viewModel.Status);
        }

        [Fact]
        public void BarcodeDialogViewModel_CancelStatusMatchesWinFormsClosingRule()
        {
            var unchanged = CreateBarcodeDialogViewModel("测试1", barcodeCount: 1, barcodeNullable: true);
            unchanged.CancelCommand.Execute();
            Assert.Equal(2, unchanged.Status);

            var changed = CreateBarcodeDialogViewModel("测试1", barcodeCount: 1, barcodeNullable: true);
            changed.SelectedFlowName = "测试2";
            changed.CancelCommand.Execute();
            Assert.Equal(0, changed.Status);
        }

        [Fact]
        public void BarcodeDialogViewModel_ManualFlowNameInputConfirmsLikeEditableLookup()
        {
            var viewModel = CreateBarcodeDialogViewModel(initialFlowName: null, barcodeCount: 1, barcodeNullable: false);

            viewModel.SelectedFlowName = "手输流程";
            viewModel.BarcodeInputs[0].Value = " SN001 ";
            viewModel.ConfirmCommand.Execute();

            Assert.True(viewModel.Result.IsConfirmed);
            Assert.Equal("手输流程", viewModel.Result.FlowName);
            Assert.Equal(new[] { "SN001" }, viewModel.Result.Barcodes);
        }

        private static ProductTestShellViewModel CreateViewModel(
            string channelNum = "2",
            FakeBarcodeDialogService? dialogService = null)
        {
            return new ProductTestShellViewModel(
                new FakeAppConfigService(channelNum),
                new FakeProductTestFlowCatalog(),
                dialogService ?? new FakeBarcodeDialogService());
        }

        private static BarcodeDialogViewModel CreateBarcodeDialogViewModel(
            string? initialFlowName,
            int barcodeCount,
            bool barcodeNullable)
        {
            return new BarcodeDialogViewModel(
                new BarcodeDialogRequest(
                    channelNumber: 1,
                    flowName: initialFlowName,
                    barcodeCount: barcodeCount,
                    barcodeNullable: barcodeNullable,
                    availableFlowNames: new[] { "测试1", "测试2" }));
        }

        private static IReadOnlyList<string> GetLabels(BarcodeDialogViewModel viewModel)
        {
            var labels = new List<string>();
            foreach (var input in viewModel.BarcodeInputs)
            {
                labels.Add(input.Label);
            }

            return labels;
        }

        private sealed class FakeBarcodeDialogService : IBarcodeDialogService
        {
            public List<BarcodeDialogRequest> Requests { get; } = new List<BarcodeDialogRequest>();

            public BarcodeDialogResult Result { get; set; } = BarcodeDialogResult.Cancelled(status: 2);

            public BarcodeDialogResult ShowBarcodeDialog(BarcodeDialogRequest request)
            {
                Requests.Add(request);
                return Result;
            }
        }

        private sealed class FakeAppConfigService : IAppConfigService
        {
            private readonly string _channelNum;

            public FakeAppConfigService(string channelNum)
            {
                _channelNum = channelNum;
            }

            public string GetValue(string key)
            {
                if (key == "ChannelNum")
                {
                    return _channelNum;
                }

                if (key == "BarcodeNullable")
                {
                    return "0";
                }

                return string.Empty;
            }

            public bool SetValue(string key, string value)
            {
                return true;
            }
        }

        private sealed class FakeProductTestFlowCatalog : IProductTestFlowCatalog
        {
            public IReadOnlyList<string> GetFlowNames()
            {
                return new[] { "测试1", "测试2" };
            }
        }
    }
}
