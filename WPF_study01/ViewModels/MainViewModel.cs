using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace WPF_study01.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // 1.创建三个页面的实例
        private readonly MonitorViewModel _monitorVM;
        private readonly HistoryViewModel _historyVM;
        private readonly SettingsViewModel _settingsVM;

        // 2.当前显示的页面（初始化为监控页）
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        // 3.三个导航命令
        public RelayCommand GoMonitorCommand { get; }
        public RelayCommand GoHistoryCommand { get; }
        public RelayCommand GoSettingsCommand { get; }

        // 4.构造函数——三个ViewModel由DI容器自动传入
        public MainViewModel(
            MonitorViewModel monitorVM,
            HistoryViewModel historyVM,
            SettingsViewModel settingsVM)
        {
            _monitorVM = monitorVM;
            _historyVM = historyVM;
            _settingsVM = settingsVM;

            // 初始显示监控页
            CurrentViewModel = _monitorVM;

            // 三个命令分别切换 CurrentViewModel
            GoMonitorCommand = new RelayCommand(() => CurrentViewModel = _monitorVM);
            GoHistoryCommand = new RelayCommand(() => {_historyVM.Refresh(); CurrentViewModel = _historyVM; });
            GoSettingsCommand = new RelayCommand(() => CurrentViewModel = _settingsVM);
        }



    }

    // RelayCommand - ICommand 的实现
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        // 构造函数：传入"执行什么"和"能不能执行"的判断
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // 能不能执行？
        public bool CanExecute(object parameter)
        {
            // 如果没传 canExecute，就永远可以执行
            // 否则调用 canExecute() 获取结果
            return _canExecute == null || _canExecute();
        }

        // 执行
        public void Execute(object parameter)
        {
            _execute?.Invoke();
        }

        // 能不能执行的状态变了的时候触发
        public event EventHandler CanExecuteChanged;

        // 手动触发 CanExecuteChanged（让 WPF 重新检查按钮状态）
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}