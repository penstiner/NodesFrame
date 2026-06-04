using csLTDMC;
using Hardware.Card.Interface;
using Hardware.Card.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Card.Services
{
    public class DMC5000 : ControlCardBase
    {

        private ReaderWriterLockSlim RWlock = new ReaderWriterLockSlim();//读写锁

        public override double GetPosition(int id)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            double pos = 0;
            LTDMC.dmc_get_position_unit(axis.CardID, axis.AxisID, ref pos);
            return Math.Round(pos, 2);
        }

        public override bool SetPosition(int id, double value)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_position_unit(axis.AxisID, axis.AxisID, value))
            {
                return true;
            }
            else
            {
                OnErrorEvent(new DMC_ErrMsg(CardErr.Low, $"设定{axis.Name}当前位置出错"));
            }
            return false;
        }

        public override double GetSpeed(int id)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            double speed = 0;
            LTDMC.dmc_read_current_speed_unit(axis.CardID, axis.AxisID, ref speed);

            return speed;
        }

        /// <summary>
        /// 正方向连续运动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        public override bool VMove(int id, double speed)
        {
            AxisParameter axis = AxisList.Where(i => i.RegID == id).First();
            double startspd = speed / 2;
            short result = LTDMC.dmc_set_profile_unit(axis.CardID, axis.AxisID, startspd, speed, axis.AccTime, axis.AccTime, startspd);
            if (result == (ushort)ERR_CODE_DMC.ERR_NOERR)
            {
                if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_vmove(axis.CardID, axis.AxisID, 1))
                {
                    return true;
                }
                else
                {
                    OnErrorEvent(new DMC_ErrMsg(CardErr.Low, $"{axis.Name}连续运动错误"));
                }
            }
            else
            {
                OnErrorEvent(new DMC_ErrMsg(CardErr.Low, $"设定{axis.Name}运动参数错误"));
            }
            return false;
        }

        /// <summary>
        /// 绝对定位
        /// </summary>
        /// <param name="id">轴ID</param>
        /// <param name="speed">运行速度</param>
        /// <param name="destination">目标位置</param>
        /// <returns></returns>
        public override bool AbsMove(int id, double speed, double destination)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            short ret = -1;
            if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_profile_unit(axis.CardID, axis.AxisID, axis.StartSpeed, speed, axis.AccTime, axis.AccTime, axis.StartSpeed))
            {
                Thread.Sleep(10);
                ret = LTDMC.dmc_pmove_unit(axis.CardID, axis.AxisID, destination, 1);
                if (ret != (ushort)ERR_CODE_DMC.ERR_NOERR)
                {
                    OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"{axis.Name}绝对定位错误"));
                }
            }
            else
            {
                OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"设定{axis.Name}运动参数错误"));
            }
            return ret == (ushort)ERR_CODE_DMC.ERR_NOERR;
        }

        /// <summary>
        /// 相对定位
        /// </summary>
        /// <param name="id"></param>
        /// <param name="speed"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public override bool RelMove(int id, double speed, double destination)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            short ret = -1;
            double startspd = speed / 2;
            lock (axis.Threadlock)
            {
                if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_profile_unit(axis.CardID, axis.AxisID, axis.StartSpeed, speed, axis.AccTime, axis.AccTime, axis.StartSpeed))
                {
                    ret = LTDMC.dmc_pmove_unit(axis.CardID, axis.AxisID, destination, 0);
                    if (ret != (ushort)ERR_CODE_DMC.ERR_NOERR)
                    {
                        OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"{axis.Name}相对定位错误"));
                    }
                }
                else
                {
                    OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"设定{axis.Name}运动参数错误"));
                }
            }
            return ret == (ushort)ERR_CODE_DMC.ERR_NOERR;
        }


        public override async Task<bool> ProcessHomeMove(int id, double speed, double plus)
        {
            await Task.Delay(2);
            var axis = GetAxis(id);
            if (axis == null) return false;
            return StartHomeMove(axis, speed, plus);
        }

        public bool StartHomeMove(AxisParameter axis, double speed, double plus)
        {
            lock (axis.Threadlock)
            {
                if (GetAxisStatus(axis.RegID) == false)
                {
                    OnErrorEvent(new DMC_ErrMsg(CardErr.Medium, $"{axis.Name}处于运行状态,不可复位"));
                    return false;
                }
                bool status = false;//完成标志
                LTDMC.dmc_set_profile_unit(axis.CardID, axis.AxisID, speed / 4, speed, 0.2, 0.2, speed / 4);
                LTDMC.dmc_set_home_pin_logic(axis.CardID, axis.AxisID, 0, 0);//设置原点传感器低电平有效
                axis.ORGStatus = true;
                axis.HomeOK = false;
                axis.HomingStep = 0;
                Thread.Sleep(50);

                while (axis.ORGStatus)
                {
                    switch (axis.HomingStep)
                    {
                        case 0:
                            {
                                //负向高速回原点
                                if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_homemode(axis.CardID, axis.AxisID, 0, 1, 0, 0))
                                {
                                    Thread.Sleep(20);
                                    if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_home_move(axis.CardID, axis.AxisID))
                                    {
                                        Thread.Sleep(30);
                                        axis.HomingStep = 1;
                                    }
                                }
                                else
                                {
                                    axis.ORGStatus = false;
                                    axis.HomingStep = -1;
                                    OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"{axis.Name}回原函数调用失败:0"));
                                }
                            }
                            break;
                        case 1:
                            {
                                int Value = 0;
                                Value = LTDMC.dmc_check_done(axis.CardID, axis.AxisID);
                                axis.HomingStep = (Value == 1) ? 2 : 1;
                                if (axis.HomingStep == 2) Thread.Sleep(100);
                            }
                            break;
                        case 2: // 往正方向先走一段距离
                            {
                                if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_profile_unit(axis.CardID, axis.AxisID, speed / 4, speed, 0.2, 0.2, speed / 4))
                                {
                                    Thread.Sleep(10);
                                    if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_pmove_unit(axis.CardID, axis.AxisID, plus, 0))
                                    {
                                        Thread.Sleep(30);
                                        axis.HomingStep = 3;
                                    }
                                }
                                else
                                {
                                    axis.ORGStatus = false;
                                    axis.HomingStep = -1;
                                    OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"{axis.Name}回原出错:步序:2"));
                                }
                            }
                            break;
                        case 3:
                            {
                                int Value = 0;
                                Value = LTDMC.dmc_check_done(axis.CardID, axis.AxisID);
                                axis.HomingStep = (Value == 1) ? 4 : 3;
                                if (axis.HomingStep == 4) Thread.Sleep(100);
                            }
                            break;
                        case 4:
                            {
                                if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_homemode(axis.CardID, axis.AxisID, 0, 0, 0, 0))//负向低速回原点
                                {
                                    Thread.Sleep(30);
                                    if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_home_move(axis.CardID, axis.AxisID))//开始回原点
                                    {
                                        Thread.Sleep(30);
                                        axis.HomingStep = 5;
                                    }
                                    else
                                    {
                                        axis.ORGStatus = false;
                                        axis.HomingStep = -1;
                                        OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"{axis.Name}回原出错:步序:4"));
                                    }
                                }
                                else
                                {
                                    axis.ORGStatus = false;
                                    axis.HomingStep = -1;
                                    OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"{axis.Name}回原出错:步序:4"));
                                }
                            }
                            break;
                        case 5:
                            {
                                int Value = 0;
                                Value = LTDMC.dmc_check_done(axis.CardID, axis.AxisID);
                                axis.HomingStep = (Value == 1) ? 6 : 5;
                                if (axis.HomingStep == 6) Thread.Sleep(100);
                            }
                            break;
                        case 6:
                            {
                                Thread.Sleep(100);
                                if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_position_unit(axis.CardID, axis.AxisID, 0.0))
                                {
                                    RWlock.EnterWriteLock();
                                    try
                                    {
                                        status = true;
                                        axis.HomeOK = true;
                                        axis.ORGStatus = false;
                                    }
                                    finally
                                    {
                                        RWlock.ExitWriteLock();
                                    }
                                }
                                else
                                {
                                    OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"{axis.Name}坐标未成功清零:步序6"));
                                }
                                axis.HomingStep = -1;
                            }
                            break;
                        default:
                            LTDMC.dmc_stop(axis.CardID, axis.AxisID, 0);
                            axis.ORGStatus = false;
                            axis.HomingStep = -1;
                            status = false;
                            break;
                    }
                    Thread.Sleep(20);
                }
                return status;
            }
        }

        #region 专用函数
        public bool SetEquiv(AxisParameter axis, double equiv)
        {
            if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_equiv(axis.CardID, axis.AxisID, equiv))
            {
                return true;
            }

            return false;
        }


        #region 龙门功能

        public bool Set_Gear_Follow_Profile(ushort cardno, ushort axis, ushort master_axis)
        {
            short ret = LTDMC.dmc_set_gear_follow_profile(cardno, axis, 1, master_axis, 1);

            return ret == (ushort)ERR_CODE_DMC.ERR_NOERR;
        }

        #endregion

        #endregion

        #region 直线插补

        /// <summary>
        /// 直线插补运动 卡0坐标系0
        /// </summary>
        /// <param name="axislist">轴号数组</param>
        /// <param name="pos">目标位置数组</param>
        /// <param name="speed">插补运动合速度</param>
        /// <returns></returns>
        public bool LineMove(ushort[] axislist, double[] pos, double speed)
        {
            bool status = false;
            if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_vector_profile_unit(0, 0, 20, speed, 0.1, 0.1, 20))
            {
                if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_line_unit(0, 0, 2, axislist, pos, 1))
                {
                    status = true;
                }
            }
            if (status == false)
            {
                OnErrorEvent(new DMC_ErrMsg(CardErr.Low, $"直线插补运行错误"));
            }

            return status;
        }

        /// <summary>
        /// 获取坐标系的运行状态
        /// </summary>
        /// <param name="cardno">卡号</param>
        /// <param name="crd">坐标系号</param>
        /// <returns>true : 停止中 false : 正在使用中</returns>
        public bool GetMulticoorStatus(ushort cardno, ushort crd)
        {
            return LTDMC.dmc_check_done_multicoor(cardno, crd) == 1;
        }

        #endregion
    }
}
