using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WPF_study01.Services;
using WPF_study01.ViewModels;

namespace WPF_study01
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        // DI 容器--整个程序的对象工厂
        public static IServiceProvider Services { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // 1.配置容器：注册"谁对应谁"
            var services = new ServiceCollection();

            // 注册服务：遇到 IProductionService 就给 ModbusProductionService
            services.AddSingleton<IProductionService, ModbusProductionService>();

            // 注册视觉服务
            services.AddSingleton<IVisionService, HttpVisionService>();

            // 注册ViewModel
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MonitorViewModel>();
            services.AddSingleton<HistoryViewModel>();
            services.AddSingleton<SettingsViewModel>();

            // 数据库实现
            services.AddSingleton<IHistoryService, MySQLHistoryService>();

            // 2.构建容器
            Services = services.BuildServiceProvider();

            // 3.从容器获取主窗口的ViewModel
            var mainVM = Services.GetRequiredService<MainViewModel>();

            // 4.创建主窗口，设置DataContext,显示
            var mainWindow = new MainWindow();
            mainWindow.DataContext = mainVM;
            mainWindow.Show();
        }
        public App()
        {
            DispatcherUnhandledException += (s, e) =>
            {
                MessageBox.Show("程序发生未处理异常：\n" + e.Exception.Message,
                    "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;   // 标记已处理，程序不退出
            };
        }
    }
}
