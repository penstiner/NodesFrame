using Hardware.Card.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Card.Interface
{
    /// <summary>
    /// 控制卡接口类
    /// </summary>
    public interface IControlCard
    {
        List<AxisParameter> AxisList { get; set; }
        List<IOParameter> InBitList { get; set; }
        List<IOParameter> OutBitList { get; set;}
        /// <summary>
        /// 是否初始化
        /// </summary>
        bool Initialized { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        bool Init();

        /// <summary>
        /// 参数初始化
        /// </summary>
        /// <param name="axis">轴集合</param>
        /// <param name="inbit">输入信号集合</param>
        /// <param name="outbit">输出信号集合</param>
        /// <returns></returns>
        bool ParamInit(List<AxisParameter> axis, List<IOParameter> inbit, List<IOParameter> outbit);

        /// <summary>
        /// 关闭控制卡
        /// </summary>
        /// <returns></returns>
        bool Close();

        /// <summary>
        /// 检测轴运行状态
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>true:轴停止 false:轴运行中</returns>
        bool GetAxisStatus(int id);

        /// <summary>
        /// 获取轴IO状态
        /// </summary>
        /// <param name="id">轴号</param>
        /// <returns></returns>
        uint GetAxisIOStatus(int id);

        /// <summary>
        /// 获取轴报警状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true：报警 false：正常</returns>
        bool GetAlarmValue(int id);

        /// <summary>
        /// 获取轴原点状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool GetORGStatus(int id);

        /// <summary>
        /// 获取轴正限位状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool GetPEL(int id);

        /// <summary>
        /// 获取轴负限位状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool GetNEL(int id);

        /// <summary>
        /// 获取轴当前位置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        double GetPosition(int id);

        /// <summary>
        /// 设置轴当前位置
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetPosition(int id, double value);

        /// <summary>
        /// 获取轴当前速度
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        double GetSpeed(int id);

        /// <summary>
        /// 连续运动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        bool VMove(int id, double speed);

        /// <summary>
        /// 绝对定位
        /// </summary>
        /// <param name="id"></param>
        /// <param name="speed"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        bool AbsMove(int id, double speed, double destination);

        /// <summary>
        /// 相对定位
        /// </summary>
        /// <param name="id"></param>
        /// <param name="speed"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        bool RelMove(int id, double speed, double destination);

        /// <summary>
        /// 停止运动
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Stop(int id);

        /// <summary>
        /// 设置轴脉冲模式
        /// </summary>
        /// <param name="id"></param>
        /// <param name="PulseMode"></param>
        /// <returns></returns>
        bool SetPulseMode(int id, ushort PulseMode);

        /// <summary>
        /// 设置限位开关有效电平
        /// </summary>
        /// <param name="id"></param>
        /// <param name="el_logic"></param>
        /// <returns></returns>
        bool SetLimitMode(int id, ushort el_logic);

        /// <summary>
        /// 读取输入信号
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool ReadIn(int id);

        /// <summary>
        /// 写入输出信号
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        bool WriteState(int id, IO_STATUS state);

        /// <summary>
        /// 读取输出信号
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool ReadOut(int id);

        /// <summary>
        /// 设置轴使能状态
        /// </summary>
        /// <param name="card"></param>
        /// <param name="axis"></param>
        /// <param name="value">0：使能 1：断开使能</param>
        /// <returns></returns>
        bool SetServoPower(int id, ushort value);

        /// <summary>
        /// 获取轴使能状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true:使能中 false:使能断开</returns>
        bool GetServoPower(int id);

        /// <summary>
        /// 紧急停止所有轴
        /// </summary>
        /// <param name="cardno">卡号</param>
        /// <returns></returns>
        bool EmgStop(ushort cardno);
        /// <summary>
        /// 回原线程
        /// </summary>
        /// <param name="id">轴id</param>
        /// <param name="speed">速度</param>
        /// <param name="plus">位置</param>
        /// <returns></returns>
        Task<bool> ProcessHomeMove(int id, double speed, double plus);

    }
}
