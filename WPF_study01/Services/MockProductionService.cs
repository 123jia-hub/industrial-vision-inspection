using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_study01.Models;

namespace WPF_study01.Services
{
    /// <summary>
    /// 模拟生产服务--假数据，未来换成真实Modbus服务
    /// </summary>
    public class MockProductionService : IProductionService 
    {
        private int _count = 0;
        private Random _random = new Random();
        private List<ProductionData> _history = new List<ProductionData>();    

        public int GetCurrentCount()
        {
            // 模拟：每次调用随机加1~10
            int add = _random.Next(1, 11);
            _count += add;
            _history.Add(new ProductionData { Timestamp = DateTime.Now, Count = _count, IsPassed = true });
            return _count;
        }

        public List<ProductionData> GetHistory()
        {
            return _history;
        }

        public void Reset()
        {
            _count = 0;
            _history.Clear();
        }

    }
}
