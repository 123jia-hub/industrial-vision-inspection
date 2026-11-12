using System;
using System.Collections.Generic;
using System.IO;
using WPF_study01.Services;

namespace WPF_study01.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private string _settings = "设置页";

        public string PlcAddress => $"{AppConfig.PlcIp} : {AppConfig.PlcPort}";
        public string VisionUrl => AppConfig.VisionBaseUrl;
        public string DatabaseName => "vision_inspection(MySQL 3306)";

        /// <summary>采用模拟相机本地图片集</summary>
        public string PartsDir => AppConfig.PartsDir;

        public string Settings
        {
            get { return _settings; }
            set
            {
                if (_settings != value)
                {
                    _settings = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
