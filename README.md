\# 工业视觉检测上位机



PLC 产量触发 → OpenCV 缺陷检测（划痕/污点分类）→ 三态判定 → MySQL 落库的完整闭环。

自写 Modbus 从站与样本生成器，无需真实硬件即可复现全流程。



\## 架构



PLC产量(Modbus TCP) → WPF上位机(100ms轮询) → HTTP → FastAPI+OpenCV视觉服务 → 结果+base64标注图 → 界面绿/红/橙三态显示 + MySQL存储



\## 运行



1\. 视觉服务：`cd VisionService \&\& python main.py`（需 opencv-python numpy fastapi uvicorn）

2\. PLC 模拟器：运行 `PlcSimulator/bin/Debug/PlcSimulator.exe`

3\. 数据库：执行建表脚本（见下方表结构）后启动 MySQL

4\. 上位机：VS 打开 `WPF\_study01/WPF\_study01.slnx` 编译运行



\## 技术



C#/.NET 4.7.2 WPF(MVVM+DI) | Python FastAPI+OpenCV | MySQL | Modbus TCP(NModbus4)



\## 已知限制



\- 高速产出时检测会跳件（无排队机制，正确做法是加队列）

\- 轮询跑在 UI 线程，真机断网会卡界面（应改异步）

\- 算法参数针对合成样本标定，换真实成像需重调

