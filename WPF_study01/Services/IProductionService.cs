using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_study01.Models;

namespace WPF_study01.Services
{
    /// <summary>
    /// 生产数据服务接口--定义"能干什么"
    /// </summary>
    public interface IProductionService
    {
        // 获取当前产量
        int GetCurrentCount();
        // 获取历史记录列表
        List<ProductionData> GetHistory();
        // 重置计数
        void Reset();
    }
}
