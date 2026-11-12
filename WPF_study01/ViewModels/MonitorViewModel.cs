using System;
using System.Windows.Threading;
using WPF_study01.Services;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace WPF_study01.ViewModels
{
    public class MonitorViewModel : ViewModelBase
    {
        // 1. 私有字段（_random 删掉了，随机数逻辑已搬进服务）
        private readonly IProductionService _productionService;
        private DispatcherTimer _timer;
        private string _statusText = "状态：待机";
        private int _totalCount = 0;

        private int _inspectedCount;
        private int _passedCount;
        private int _failedCount;
        private bool _isDetecting;
        private int _lastTriggeredCount;   // 上一次已触发检测的产量值

        // 视觉附加字段
        private readonly IVisionService _visionService;
        private DetectionResult _lastResult;
        private ImageSource _resultImage;

        // 数据库附加字段
        private readonly IHistoryService _historyService;

        // 2. 公开属性
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TotalCount
        {
            get { return _totalCount; }
            set
            {
                if (_totalCount != value)
                {
                    _totalCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public int InspectedCount
        {
            get { return _inspectedCount; }
            private set
            {
                if (_inspectedCount != value)
                {
                    _inspectedCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PassRate));
                }
            }
        }

        public int PassedCount
        {
            get { return _passedCount; }
            private set
            {
                if (_passedCount != value)
                {
                    _passedCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PassRate));
                }
            }
        }

        public int FailedCount
        {
            get { return _failedCount; }
            private set
            {
                if (_failedCount != value)
                {
                    _failedCount = value;
                    OnPropertyChanged();
                }
            }
        }

        // 计算属性：不存值，读时现算
        public double PassRate
        {
            get { return _inspectedCount == 0 ? 0 : Math.Round(_passedCount * 100.0 / _inspectedCount, 1); }
        }

        // 视觉附加属性
        public DetectionResult LastResult
        {
            get { return _lastResult; }
            set
            {
                if (_lastResult != value)
                {
                    _lastResult = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DefectText));
                }
            }
        }

        /// <summary>缺陷个数显示文本（-1 = 检测异常，不显示数字）</summary>
        public string DefectText
        {
            get
            {
                if (_lastResult == null) return "缺陷个数：-";
                if (_lastResult.DefectCount == -1) return "缺陷个数：--";
                return "缺陷个数：" + _lastResult.DefectCount;
            }
        }

        public ImageSource ResultImage
        {
            get { return _resultImage; }
            set { _resultImage = value;  OnPropertyChanged(); }
        }

        // 3. 命令
        public RelayCommand StartCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand ClearCommand { get; }

        // 视觉附加命令
        public RelayCommand CaptureCommand { get; }

        // 4. 构造函数——不再自己new服务，从参数接收（这就是"注入"）
        // 视觉附加构造函数
        public MonitorViewModel(IProductionService productionService, IVisionService visionService, IHistoryService historyService)
        {
            _productionService = productionService;
            _visionService = visionService;
            _historyService = historyService;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;

            StartCommand = new RelayCommand(Start, () => !_timer.IsEnabled);
            StopCommand = new RelayCommand(Stop, () => _timer.IsEnabled);
            ClearCommand = new RelayCommand(Clear);
            CaptureCommand = new RelayCommand(CaptureAndDetect, () => !_isDetecting);
        }

        // 5. 计时器回调——向服务要数据，不再自己算
        private void Timer_Tick(object sender, EventArgs e)
        {
            TotalCount = _productionService.GetCurrentCount();

            // 产量增加 = 来了新工件 -> 自动触发一次检测（闭环：PLC -> 相机 -> 判定 -> 存库）
            if (TotalCount > _lastTriggeredCount && !_isDetecting)
            {
                _lastTriggeredCount = TotalCount;
                CaptureAndDetect();
            }
        }

        // 6. 命令方法
        private void Start()
        {
            StatusText = "状态：运行中";
            // 记下启动时的产量做基准，只有"新增工件"才自动触发检测
            _lastTriggeredCount = _productionService.GetCurrentCount();
            _timer.Start();
            StartCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
        }

        private void Stop()
        {
            StatusText = "状态：已停止";
            _timer.Stop();
            StartCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
        }

        private void Clear()
        {
            _timer.Stop();
            _productionService.Reset();   // 让服务也归零
            TotalCount = 0;
            _lastTriggeredCount = 0;
            InspectedCount = 0;
            PassedCount = 0;
            FailedCount = 0;
            LastResult = null;            // 清掉上一张图和判定
            ResultImage = null;
            StatusText = "状态：待机";
            StartCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
        }
        // 视觉附加方法
        private async void CaptureAndDetect()
        {
            _isDetecting = true;
            CaptureCommand.RaiseCanExecuteChanged();
            try
            {
                StatusText = "状态：检测中...";

                var isOnline = await _visionService.IsOnlineAsync();
                if (!isOnline)
                {
                    StatusText = "状态：视觉服务离线";
                    return;
                }

                LastResult = await _visionService.CaptureAndDetectAsync();

                // DefectCount = -1 是服务层约定的"检测失败"暗号，不入统计、不显示图
                if (LastResult.DefectCount == -1)
                {
                    ResultImage = null;
                    StatusText = "状态：检测失败 - " + (LastResult.Defects.Count > 0 ? LastResult.Defects[0].Type : "未知错误");
                    return;
                }

                if (!string.IsNullOrEmpty(LastResult.AnnotatedImage))
                {
                    var bytes = Convert.FromBase64String(LastResult.AnnotatedImage);
                    using (var stream = new MemoryStream(bytes))
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.StreamSource = stream;
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        ResultImage = bmp;
                    }
                }

                InspectedCount++;
                if (LastResult.IsPassed) PassedCount++; else FailedCount++;

                StatusText = LastResult.IsPassed
                    ? "状态：检测通过"
                    : $"状态：发现{LastResult.DefectCount}个缺陷";

                // 写库的图片名跟实际检测的图保持一致
                _historyService.SaveRecord(LastResult.IsPassed, LastResult.DefectCount, LastResult.ImageName);
            }
            catch (Exception ex)
            {
                StatusText = "状态：异常 - " + ex.Message;
            }
            finally
            {
                _isDetecting = false;
                CaptureCommand.RaiseCanExecuteChanged();
            }
        }
    }
}