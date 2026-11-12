using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Modbus.Data;
using Modbus.Device;

namespace PlcSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1.占住502端口，等主站来连（Modbus TCP == 一个TCP服务）
            TcpListener listener = new TcpListener(IPAddress.Any, 502);
            listener.Start();

            // 2.从站本体，站号设为1（上位机读是必须保持为一致）
            ModbusTcpSlave slave = ModbusTcpSlave.CreateTcp(1, listener);

            // 3.一张"寄存器表格"
            slave.DataStore = DataStoreFactory.CreateDefaultDataStore();

            // 4.开始接待主站
            slave.Listen();

            Console.WriteLine("PLC模拟器已启用：127.0.0.1：502，站号1");
            Console.WriteLine("(模拟设备开启 -- 勿关)");

            Random random = new Random();

            // 5.死循环：模拟设备产出
            while (true)
            {
                // ▲ NModbus4的坑：这张表格是"1基"的——索引1 才对应 Modbus 地址0
                ushort current = slave.DataStore.HoldingRegisters[1];

                int next = current + random.Next(1, 11);
                if (next > 65535) next = 0;     // 16位寄存器，超了归0              

                slave.DataStore.HoldingRegisters[1] = (ushort)next;

                Console.WriteLine("保持寄存器[地址0] = " + next);
                Thread.Sleep(200);
            }
        }
    }
}
