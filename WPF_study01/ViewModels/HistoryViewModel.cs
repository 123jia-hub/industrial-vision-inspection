using System;
using System.Collections.Generic;
using WPF_study01.Services;

namespace WPF_study01.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        private readonly IHistoryService _historyService;
        private List<DetectionRecord> _records;
        private string _errorText = "";
        public string ErrorText
        {
            get { return _errorText; }
            set
            {
                if (_errorText != value)
                {
                    _errorText = value;
                    OnPropertyChanged();
                }
            }
        }

        /// 表格数据源
        public List<DetectionRecord> Records
        {
            get { return _records; }
            set { _records = value; OnPropertyChanged(); }
        }

        public RelayCommand RefreshCommand { get; }

        public HistoryViewModel(IHistoryService historyService)
        {
            _historyService = historyService;
            RefreshCommand = new RelayCommand(LoadRecords);
            LoadRecords();   // 启动时先加载一次
        }

        private void LoadRecords()
        {
            try
            {
                Records = _historyService.GetAllRecords();
                ErrorText = "";
            }
            catch (Exception ex)
            {
                Records = new List<DetectionRecord>();
                ErrorText = "数据库连接失败：" + ex.Message;
            }
        }
        // 历史页自动刷新
        public void Refresh()
        {
            LoadRecords();
        }
    }
}