using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;

namespace WPF_study01.Services
{
    /// <summary>
    /// HTTP视觉服务--调用Python FastAPI
    /// </summary>
    public class HttpVisionService : IVisionService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = AppConfig.VisionBaseUrl;
        private readonly string[] _frames;   // 样本集里的所有图片
        private int _frameIndex;             // 当前取到第几张

        public HttpVisionService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(5);

            _frames = Directory.Exists(AppConfig.PartsDir)
                ? Directory.GetFiles(AppConfig.PartsDir, "*.jpg").OrderBy(f => f).ToArray()
                : new string[0];
        }

        public async Task<bool> IsOnlineAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<DetectionResult> CaptureAndDetectAsync()
        {
            if (_frames.Length == 0)
            {
                return new DetectionResult
                {
                    IsPassed = false,
                    DefectCount = -1,
                    Defects = new List<Defect> { new Defect { Type = $"样本集为空: {AppConfig.PartsDir}" } }
                };
            }

            // 模拟相机：每检测一次取下一张，用完从头循环
            var imagePath = _frames[_frameIndex % _frames.Length];
            _frameIndex++;

            if (!File.Exists(imagePath))
            {
                // 图片不存在，返回失败（不再返回假的"合格"，避免误判）
                return new DetectionResult
                {
                    IsPassed = false,
                    DefectCount = -1,
                    Defects = new List<Defect> { new Defect { Type = $"图片不存在: {imagePath}" } }
                };
            }

            try
            {
                // 读取图片字节
                var imageBytes = File.ReadAllBytes(imagePath);

                // 构建 multipart/form-data（和Python的UploadFile对应）
                using var content = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(imageBytes);
                content.Add(fileContent, "image", "test.jpg");

                // 发送POST请求
                var response = await _httpClient.PostAsync($"{BaseUrl}/detect", content);
                response.EnsureSuccessStatusCode();

                // 解析JSON
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<DetectionResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // 1. 先兜底：解析失败就直接返回失败，后面不再碰 result
                if (result == null)
                {
                    return new DetectionResult
                    {
                        IsPassed = false,
                        DefectCount = -1,
                        Defects = new List<Defect> { new Defect { Type = "服务返回内容为空" } }
                    };
                }

                // 2. 确认 result 非空后，再补图片名
                result.ImageName = Path.GetFileName(imagePath);
                return result;
            }
            catch(Exception ex)
            {
                // 网络错误或服务挂了，返回失败
                return new DetectionResult
                {
                    IsPassed = false,
                    DefectCount=-1,  // -1 表示检测错误
                    Defects=new List<Defect> { new Defect { Type = ex.Message} }
                };
            }
        }
    }
}
