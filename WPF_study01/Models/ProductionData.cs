using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_study01.Models
{
    /// <summary>
    /// 生产数据模型--代表一次检测的结果
    /// </summary>
    public class ProductionData
    {
        public DateTime Timestamp { get; set; }  // 时间戳
        public int Count { get; set; }  // 产量
        public bool IsPassed { get; set; }  // 是否合格
    }
}
