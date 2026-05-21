using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ATS5.Application.DataQuery;
using ATS5.Application.Logging;
using ATS5.Wpf.Core;
using Prism.Commands;
using Prism.Mvvm;

namespace ATS5.Modules.DataQuery.ViewModels
{
    public sealed class DataQueryViewModel : BindableBase
    {
        private const int DefaultPageSize = 200;
        private const string DateEmptyWarningMessage = "日期不能为空";
        private const string DateSpanWarningMessage = "日期跨度不能大于180天，请调整查询日期";
        private const string NoDataInfoMessage = "未查询到相关数据";
        private const string DataQueryErrorMessage = "数据查询异常";
        private const string MesUploadErrorMessage = "MES上传异常";

        private readonly DataQueryService _dataQueryService;
        private readonly ManualMesUploadService _manualMesUploadService;
        private readonly IMessageDialogService _messageDialogService;
        private readonly ILogService _logService;
        private DateTime? _startDate = DateTime.Today;
        private DateTime? _endDate = DateTime.Today;
        private bool _isBarcodeFilterEnabled;
        private string _barcode = string.Empty;
        private bool _isFlowFilterEnabled;
        private string _flowName = string.Empty;
        private string _statisticsFlowName = string.Empty;
        private string _statisticsProjectName = string.Empty;
        private DateTime? _statisticsStartTime;
        private DateTime? _statisticsEndTime;
        private bool _isChannelFilterEnabled;
        private ushort _channel = 1;
        private int _currentPage = 1;
        private int _totalCount;
        private string _message = string.Empty;
        private TestRecord? _selectedRecord;
        private Task _lastLoadDetailsTask = Task.CompletedTask;
        private int _detailLoadVersion;

        public DateTime? StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public bool IsBarcodeFilterEnabled
        {
            get => _isBarcodeFilterEnabled;
            set => SetProperty(ref _isBarcodeFilterEnabled, value);
        }

        public string Barcode
        {
            get => _barcode;
            set => SetProperty(ref _barcode, value);
        }

        public bool IsFlowFilterEnabled
        {
            get => _isFlowFilterEnabled;
            set => SetProperty(ref _isFlowFilterEnabled, value);
        }

        public string FlowName
        {
            get => _flowName;
            set => SetProperty(ref _flowName, value);
        }

        public string StatisticsFlowName
        {
            get => _statisticsFlowName;
            set => SetProperty(ref _statisticsFlowName, value);
        }

        public string StatisticsProjectName
        {
            get => _statisticsProjectName;
            set => SetProperty(ref _statisticsProjectName, value);
        }

        public DateTime? StatisticsStartTime
        {
            get => _statisticsStartTime;
            set => SetProperty(ref _statisticsStartTime, value);
        }

        public DateTime? StatisticsEndTime
        {
            get => _statisticsEndTime;
            set => SetProperty(ref _statisticsEndTime, value);
        }

        public bool IsChannelFilterEnabled
        {
            get => _isChannelFilterEnabled;
            set => SetProperty(ref _isChannelFilterEnabled, value);
        }

        public ushort Channel
        {
            get => _channel;
            set => SetProperty(ref _channel, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public TestRecord? SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (SetProperty(ref _selectedRecord, value))
                {
                    StartLoadDetails();
                }
            }
        }

        public ObservableCollection<TestRecord> Records { get; } = new ObservableCollection<TestRecord>();

        /// <summary>
        /// Gets the records selected for manual MES upload.
        /// </summary>
        public ObservableCollection<TestRecord> SelectedUploadRecords { get; } = new ObservableCollection<TestRecord>();

        public ObservableCollection<TestDetailRecord> Details { get; } = new ObservableCollection<TestDetailRecord>();

        public DelegateCommand QueryCommand { get; }

        public DelegateCommand LoadDetailsCommand { get; }

        public DelegateCommand ManualMesUploadCommand { get; }

        public Task LastLoadDetailsTask
        {
            get => _lastLoadDetailsTask;
            private set => SetProperty(ref _lastLoadDetailsTask, value);
        }

        public DataQueryViewModel(
            DataQueryService dataQueryService,
            ManualMesUploadService manualMesUploadService,
            IMessageDialogService messageDialogService,
            ILogService logService)
        {
            _dataQueryService = dataQueryService;
            _manualMesUploadService = manualMesUploadService;
            _messageDialogService = messageDialogService;
            _logService = logService;
            _startDate = DateTime.Today.AddDays(-1);
            QueryCommand = new DelegateCommand(async () => await QueryAsync());
            ManualMesUploadCommand = new DelegateCommand(async () => await ManualMesUploadAsync());
            LoadDetailsCommand = new DelegateCommand(async () => await LoadDetailsAsync(), () => SelectedRecord != null).ObservesProperty(() => SelectedRecord);
        }

        public async Task QueryAsync()
        {
            if (StartDate == null || EndDate == null)
            {
                ShowLegacyQueryMessage(DateEmptyWarningMessage);
                return;
            }

            try
            {
                CurrentPage = 1;
                var filter = new DataQueryFilter
                {
                    StartDate = StartDate.Value,
                    EndDate = EndDate.Value,
                    IsBarcodeFilterEnabled = IsBarcodeFilterEnabled,
                    Barcode = Barcode,
                    IsFlowFilterEnabled = IsFlowFilterEnabled,
                    FlowName = FlowName,
                    IsChannelFilterEnabled = IsChannelFilterEnabled,
                    Channel = Channel
                };

                var result = await Task.Run(() => _dataQueryService.QueryAsync(filter, CurrentPage, DefaultPageSize, default));
                TotalCount = result.TotalCount;
                Message = result.Message;
                if (!result.IsSuccess && !string.IsNullOrEmpty(result.Message))
                {
                    ShowLegacyQueryMessage(result.Message);
                    if (result.Message == DateSpanWarningMessage)
                    {
                        return;
                    }
                }

                SelectedRecord = null;
                SelectedUploadRecords.Clear();
                Records.Clear();
                Details.Clear();
                if (result.Message == NoDataInfoMessage)
                {
                    ClearStatisticsState();
                }
                else
                {
                    RefreshStatisticsState();
                }

                foreach (var item in result.Records)
                {
                    Records.Add(item);
                }
            }
            catch (Exception ex)
            {
                ShowLegacyError(DataQueryErrorMessage, ex);
            }
        }

        public async Task LoadDetailsAsync()
        {
            var version = ++_detailLoadVersion;
            var selectedRecord = SelectedRecord;
            LastLoadDetailsTask = LoadDetailsCoreAsync(selectedRecord, version);
            await LastLoadDetailsTask;
        }

        public async Task ManualMesUploadAsync()
        {
            var selectedRecords = GetManualMesUploadRecords();
            var validationResult = _manualMesUploadService.ValidateBeforeConfirmation(selectedRecords);
            if (!validationResult.IsSuccess)
            {
                Message = string.Join(Environment.NewLine, validationResult.Messages);
                ShowLegacyMesMessages(validationResult.Messages);
                return;
            }

            try
            {
                var isConfirmed = _messageDialogService.Confirm(_manualMesUploadService.GetConfirmMessage());
                var result = await Task.Run(() => _manualMesUploadService.UploadAsync(selectedRecords, isConfirmed, default));
                Message = string.Join(Environment.NewLine, result.Messages);
                ShowLegacyMesMessages(result.Messages);
            }
            catch (Exception ex)
            {
                ShowLegacyError(MesUploadErrorMessage, ex);
            }
        }

        private void StartLoadDetails()
        {
            var version = ++_detailLoadVersion;
            var selectedRecord = SelectedRecord;
            LastLoadDetailsTask = LoadDetailsCoreAsync(selectedRecord, version);
        }

        private async Task LoadDetailsCoreAsync(TestRecord? selectedRecord, int version)
        {
            Details.Clear();
            if (selectedRecord == null)
            {
                return;
            }

            try
            {
                var details = await Task.Run(() => _dataQueryService.GetDetailsAsync(selectedRecord, default));
                if (version != _detailLoadVersion || !ReferenceEquals(SelectedRecord, selectedRecord))
                {
                    return;
                }

                Details.Clear();
                foreach (var item in details)
                {
                    Details.Add(item);
                }
            }
            catch (Exception ex)
            {
                if (version == _detailLoadVersion)
                {
                    ShowLegacyError(DataQueryErrorMessage, ex);
                }
            }
        }

        private IReadOnlyList<TestRecord> GetManualMesUploadRecords()
        {
            if (SelectedUploadRecords.Count > 0)
            {
                return SelectedUploadRecords.ToList();
            }

            if (SelectedRecord != null)
            {
                return new[] { SelectedRecord };
            }

            return Array.Empty<TestRecord>();
        }

        private void ClearStatisticsState()
        {
            StatisticsFlowName = string.Empty;
            StatisticsProjectName = string.Empty;
            StatisticsStartTime = null;
            StatisticsEndTime = null;
        }

        private void RefreshStatisticsState()
        {
            StatisticsFlowName = string.Empty;
            StatisticsProjectName = string.Empty;
            if (StartDate != null && EndDate != null)
            {
                StatisticsStartTime = StartDate.Value.Date;
                StatisticsEndTime = EndDate.Value.Date.AddDays(1).AddSeconds(-1);
            }
        }

        private void ShowLegacyQueryMessage(string message)
        {
            Message = message;
            if (message == NoDataInfoMessage)
            {
                _messageDialogService.ShowInfo(message);
                return;
            }

            _messageDialogService.ShowWarning(message);
        }

        private void ShowLegacyMesMessages(IReadOnlyList<string> messages)
        {
            foreach (var message in messages)
            {
                if (string.IsNullOrEmpty(message))
                {
                    continue;
                }

                if (message.Contains("MES上传成功!"))
                {
                    _messageDialogService.ShowInfo(message);
                    continue;
                }

                _messageDialogService.ShowWarning(message);
            }
        }

        private void ShowLegacyError(string prefix, Exception exception)
        {
            _logService.Error(prefix, exception);
            Message = $"{prefix}：{exception.Message}";
            _messageDialogService.ShowError(Message);
        }

    }
}
