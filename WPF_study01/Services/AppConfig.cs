using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_study01.Services
{
    /// <summary>
    /// 全局配置——所有硬编码地址集中在这里
    /// </summary>
    public static class AppConfig
    {
        public const string PlcIp = "127.0.0.1";
        public const int PlcPort = 502;
        public const string VisionBaseUrl = "http://127.0.0.1:8000";
        public const string MysqlConnStr = "Server=127.0.0.1;Port=3306;Database=vision_inspection;Uid=root;Pwd=123456;";

        /// <summary>模拟相机的样本集目录（每检测一次取下一张）</summary>
        public const string PartsDir = @"D:\WPF\VisionService\samples\parts";
    }
}
