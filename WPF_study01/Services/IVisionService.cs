using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace WPF_study01.Services
{
    /// <summary>
    /// 视觉检测结果
    /// </summary>
    public class DetectionResult
    {
        public bool IsPassed { get; set; }
        public int DefectCount { get; set; }
        public List<Defect> Defects { get; set; } = new List<Defect>();
        [JsonPropertyName("annotated_image")]
        public string AnnotatedImage { get; set; } = "";
        /// <summary>本次检测用的图片文件名（C# 侧填写，不来自 Python）</summary>
        public string ImageName { get; set; } = "";
    }

    public class Defect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; } = "";
    }
    /// <summary>
    /// 视觉服务接口--能力清单
    /// </summary>
    public interface IVisionService
    {
        /// 拍照并检测（模拟：使用的是本地测试图）
        Task<DetectionResult> CaptureAndDetectAsync();
        /// 检查视觉服务是否在线
        Task<bool> IsOnlineAsync();
    }
}
