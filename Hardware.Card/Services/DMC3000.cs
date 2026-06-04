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
    public class DMC3000 : ControlCardBase
    {

        private ReaderWriterLockSlim RWlock = new ReaderWriterLockSlim();//读写锁

        public override double GetPosition(int id)
        {
            var axis = GetAxis(id);
            if (axis == null) return 0;
            double pos = 0;
            pos = LTDMC.dmc_get_position(axis.CardID, axis.AxisID) / axis.Equiv;
            return Math.Round(pos, 2);
        }

        public override bool SetPosition(int id, double value)
        {
            var axis = GetAxis(id);
            if (axis == null) return false;
            int pos = Convert.ToInt32(value * axis.Equiv);
            if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_position(axis.CardID, axis.AxisID, pos))
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
            var axis = GetAxis(id);
            if (axis == null) return 0;
            double speed = 0;
            speed = LTDMC.dmc_read_current_speed(axis.CardID, axis.AxisID) / axis.Equiv;

            return speed;
        }

        public override bool VMove(int id, double speed)
        {
            var axis = GetAxis(id);
            if (axis == null) return false;
            double startspd = speed / 2;
            short result = LTDMC.dmc_set_profile(axis.CardID, axis.AxisID, startspd * axis.Equiv, speed * axis.Equiv, axis.AccTime, axis.AccTime, startspd * axis.Equiv);
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

        public override bool AbsMove(int id, double speed, double destination)
        {
            var axis = GetAxis(id);
            if (axis == null) return false;
            short ret = -1;
            if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_profile(axis.CardID, axis.AxisID, axis.StartSpeed * axis.Equiv, speed * axis.Equiv, axis.AccTime, axis.AccTime, axis.StartSpeed * axis.Equiv))
            {
                Thread.Sleep(10);
                ret = LTDMC.dmc_pmove(axis.CardID, axis.AxisID, Convert.ToInt32(destination * axis.Equiv), 1);
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

        public override bool RelMove(int id, double speed, double destination)
        {
            var axis = GetAxis(id);
            if (axis == null) return false;
            short ret = -1;
            if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_profile(axis.CardID, axis.AxisID, axis.StartSpeed * axis.Equiv, speed * axis.Equiv, axis.AccTime, axis.AccTime, axis.StartSpeed * axis.Equiv))
            {
                Thread.Sleep(10);
                ret = LTDMC.dmc_pmove(axis.CardID, axis.AxisID, Convert.ToInt32(destination * axis.Equiv), 0);
                if (ret != (ushort)ERR_CODE_DMC.ERR_NOERR)
                {
                    OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"{axis.Name}相对定位错误"));
                }
            }
            else
            {
                OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"设定{axis.Name}运动参数错误"));
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

        private bool StartHomeMove(AxisParameter axis, double speed, double plus)
        {
            lock (axis.Threadlock)
            {
                if (GetAxisStatus(axis.RegID) == false)
                {
                    OnErrorEvent(new DMC_ErrMsg(CardErr.Medium, $"{axis.Name}处于运行状态,不可复位"));
                    return false;
                }
                bool status = false;//完成标志
                axis.ORGStatus = true;
                axis.HomeOK = false;
                //将速度参数转为为 脉冲/s 频率单位
                double startspeed = axis.StartSpeed * axis.Equiv;
                double runspeed = speed * axis.Equiv;
                //设置回原参数
                LTDMC.dmc_set_profile(axis.CardID, axis.AxisID, runspeed / 4, runspeed, 0.2, 0.2, runspeed / 4);
                LTDMC.dmc_set_home_pin_logic(axis.CardID, axis.AxisID, 0, 0);//设置原点传感器低电平有效

                int step = 0;

                while (axis.ORGStatus)
                {
                    switch (step)
                    {
                        case 0:
                            {
                                //负向高速回原点
                                if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_homemode(axis.CardID, axis.AxisID, 0, 1, 0, 0))
                                {
                                    if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_home_move(axis.CardID, axis.AxisID))
                                    {
                                        Thread.Sleep(30);
                                        step = 1;
                                    }
                                }
                                else
                                {
                                    axis.ORGStatus = false;
                                    step = -1;
                                    OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"{axis.Name}回原函数调用失败:0"));
                                }
                            }
                            break;
                        case 1:
                            {
                                int Value = 0;
                                Value = LTDMC.dmc_check_done(axis.CardID, axis.AxisID);
                                step = (Value == 1) ? 2 : 1;
                                if (step == 2) Thread.Sleep(100);
                            }
                            break;
                        case 2: // 往正方向先走一段距离
                            {
                                if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_profile(axis.CardID, axis.AxisID, startspeed, runspeed, 0.2, 0.2, startspeed))
                                {
                                    Thread.Sleep(10);
                                    if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_pmove(axis.CardID, axis.AxisID, Convert.ToInt32(plus * axis.Equiv), 0))
                                    {
                                        Thread.Sleep(30);
                                        step = 3;
                                    }
                                }
                                else
                                {
                                    axis.ORGStatus = false;
                                    step = -1;
                                    OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"{axis.Name}回原出错:步序:2"));
                                }
                            }
                            break;
                        case 3:
                            {
                                int Value = 0;
                                Value = LTDMC.dmc_check_done(axis.CardID, axis.AxisID);
                                step = (Value == 1) ? 4 : 3;
                                if (step == 4) Thread.Sleep(100);
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
                                        step = 5;
                                    }
                                    else
                                    {
                                        axis.ORGStatus = false;
                                        step = -1;
                                        OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"{axis.Name}回原出错:步序:4"));
                                    }
                                }
                                else
                                {
                                    axis.ORGStatus = false;
                                    step = -1;
                                    OnErrorEvent(new DMC_ErrMsg(CardErr.High, $"{axis.Name}回原出错:步序:4"));
                                }
                            }
                            break;
                        case 5:
                            {
                                int Value = 0;
                                Value = LTDMC.dmc_check_done(axis.CardID, axis.AxisID);
                                step = (Value == 1) ? 6 : 5;
                                if (step == 6) Thread.Sleep(100);
                            }
                            break;
                        case 6:
                            {
                                Thread.Sleep(100);
                                if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_position(axis.CardID, axis.AxisID, 0))
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
                                step = -1;
                            }
                            break;
                        default:
                            LTDMC.dmc_stop(axis.CardID, axis.AxisID, 0);
                            axis.ORGStatus = false;
                            step = -1;
                            status = false;
                            break;
                    }
                    Thread.Sleep(20);
                }
                return status;
            }
        }
    }
}
