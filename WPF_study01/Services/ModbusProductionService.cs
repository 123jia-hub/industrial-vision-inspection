using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Modbus.Device;
using WPF_study01.Models;

namespace WPF_study01.Services
{
    /// <summary>
    /// Modbus生产服务--从真实PLC读数据（）IProductionService 的第二种实现
    /// </summary>
    public class ModbusProductionService : IProductionService
    {
        private TcpClient _tcpClient;
        private IModbusMaster _master;
        private int _lastCount = 0;     // 断线时兜底，避免界面闪0

        public ModbusProductionService()
        {
            ConnectIfNeeded();
        }

        // 重连
        private bool ConnectIfNeeded()
        {
            try
            {
                if (_tcpClient == null || !_tcpClient.Connected)
                {
                    _tcpClient = new TcpClient();
                    _tcpClient.Connect(AppConfig.PlcIp,AppConfig.PlcPort);

                    _master = ModbusIpMaster.CreateIp(_tcpClient);
                    _master.Transport.ReadTimeout = 2000;       // 读超时
                    _master.Transport.Retries = 3;      // 重试次数
                }
                return true;
            }
            catch
            {
                return false;       // 连接失败
            }
        }

        public int GetCurrentCount()
        {
            if (!ConnectIfNeeded())
            {
                return _lastCount;
            }

            try
            {
                // ★ 站号1、起始地址0、读1个
                ushort[] registers = _master.ReadHoldingRegisters(1, 0, 1);
                _lastCount = registers[0];
            }
            catch
            {
                // 读一半断了，保持旧值
            }
            return _lastCount;
        }
        public List<ProductionData> GetHistory()
        {
            return new List<ProductionData>();
        }
        public void Reset()
        {
            try
            {
                // ★ 往站号1的地址0写0（功能码06）
                _master.WriteSingleRegister(1, 0, 0);
                _lastCount = 0;
            }
            catch { }
        }
    }
}
