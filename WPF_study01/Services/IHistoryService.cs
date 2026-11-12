using System;
using System.Collections.Generic;

namespace WPF_study01.Services
{
    /// <summary>
    /// 历史记录接口
    /// </summary>
    public interface IHistoryService
    {
        // 存一条检测记录
        void SaveRecord(bool isPassed, int defectCount, string imagePath = null);
        // 取所有记录（优先测试SaveRecord）
        List<DetectionRecord> GetAllRecords();
    }

    // 检测记录（数据库行的C#镜像）
    public class DetectionRecord
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsPassed { get; set; }
        public int DefectCount { get; set; }
        public string ImagePath { get; set; }
        public string ResultText => IsPassed ? "合格" : "不合格";
    }

}
