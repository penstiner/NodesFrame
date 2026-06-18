using csLTDMC;
using Hardware.Card.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Card.Interface
{
    /// <summary>
    /// 控制卡抽象基类
    /// </summary>
    public abstract class ControlCardBase : IControlCard
    {
        public List<AxisParameter> AxisList { get; set; }
        public List<IOParameter> InBitList { get; set; }
        public List<IOParameter> OutBitList { get; set; }

        private ReaderWriterLockSlim RWlock = new ReaderWriterLockSlim();//读写锁

        public event EventHandler<DMC_ErrMsg> ErrorEvent;//控制卡出错触发事件
        public bool Initialized { get; set; }

        public virtual bool Init()
        {
            if (Initialized) return true;
            short num = LTDMC.dmc_board_init();
            if (num < 1 || num > 8)
            {
                return false;
            }
            Initialized = true;
            return true;
        }

        public virtual bool ParamInit(List<AxisParameter> axis, List<IOParameter> inbit, List<IOParameter> outbit)
        {
            try
            {
                AxisList = axis;
                InBitList = inbit;
                OutBitList = outbit;
            }
            catch
            {
                return false;
            }
            return true;
        }

        public virtual bool Close()
        {
            if (Initialized)
            {
                Initialized = false;
                short status = LTDMC.dmc_board_close();
                if (status != 0)
                {
                    return false;
                }
                else if (status == 0) return true;
            }
            return false;
        }

        public virtual bool GetAxisStatus(int id)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            return LTDMC.dmc_check_done(axis.CardID, axis.AxisID) == 1;
        }

        public virtual uint GetAxisIOStatus(int id)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            return LTDMC.dmc_axis_io_status(axis.CardID, axis.AxisID);
        }

        public virtual bool GetAlarmValue(int id)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            uint status = LTDMC.dmc_axis_io_status(axis.CardID, axis.AxisID);
            bool value = ((status >> (int)Motion_IO_Status.ALM) & 1) == 1;
            return value;
        }

        public virtual bool GetORGStatus(int id)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            uint status = LTDMC.dmc_axis_io_status(axis.CardID, axis.AxisID);
            bool value = ((status >> (int)Motion_IO_Status.ORG) & 1) == 1;
            return value;
        }

        public virtual bool GetPEL(int id)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            uint status = LTDMC.dmc_axis_io_status(axis.CardID, axis.AxisID);
            bool value = ((status >> (int)Motion_IO_Status.PEL) & 1) == 1;
            return value;
        }

        public virtual bool GetNEL(int id)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            uint status = LTDMC.dmc_axis_io_status(axis.CardID, axis.AxisID);
            bool value = ((status >> (int)Motion_IO_Status.NEL) & 1) == 1;
            return value;
        }

        public virtual bool SetPulseMode(int id, ushort PulseMode)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_pulse_outmode(axis.CardID, axis.AxisID, PulseMode))
            {
                return true;
            }

            return false;
        }

        public bool SetLimitMode(int id, ushort el_logic)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_set_el_mode(axis.CardID, axis.AxisID, 1, el_logic, 0))
            {
                return true;
            }
            return false;
        }

        public bool ReadIn(ushort cardno, ushort node, ushort bitno)
        {
            IO_STATUS state = IO_STATUS.OFF;

            if (node == 0)//控制卡本体IO
            {
                state = (IO_STATUS)LTDMC.dmc_read_inbit(cardno, bitno);
            }
            else if (node > 0)//拓展模块上的IO
            {
                state = (IO_STATUS)LTDMC.dmc_read_can_inbit(cardno, node, bitno);
            }

            return (state == IO_STATUS.ON);
        }

        public bool ReadOut(ushort cardno, ushort node, ushort bitno)
        {
            IO_STATUS state = IO_STATUS.OFF;

            if (node == 0)//控制卡本体IO
            {
                state = (IO_STATUS)LTDMC.dmc_read_outbit(cardno, bitno);
            }
            else if (node > 0)//拓展模块上的IO
            {
                state = (IO_STATUS)LTDMC.dmc_read_can_outbit(cardno, node, bitno);
            }

            return (state == IO_STATUS.ON);
        }

        public bool WriteState(ushort cardno, ushort node, ushort bitno, IO_STATUS state)
        {
            bool flag = false;
            if (node == 0)
            {
                short result = LTDMC.dmc_write_outbit(cardno, bitno, (ushort)state);
                if (result == (short)ERR_CODE_DMC.ERR_NOERR) { flag = true; }
            }
            else if (node > 0)
            {
                short result = LTDMC.dmc_write_can_outbit(cardno, node, bitno, (ushort)state);
                if (result == (short)ERR_CODE_DMC.ERR_NOERR) { flag = true; }
            }

            return flag;
        }

        public bool ReadIn(int id)
        {
            IOParameter parameter = InBitList.First(it => it.RegID == id);
            IO_STATUS state = IO_STATUS.OFF;
            ushort cardno = parameter.Cardno;
            ushort node = parameter.Nodeno;
            ushort bitno = parameter.Bitno;

            if (node == 0)//控制卡本体IO
            {
                state = (IO_STATUS)LTDMC.dmc_read_inbit(cardno, bitno);
            }
            else if (node > 0)//拓展模块上的IO
            {
                state = (IO_STATUS)LTDMC.dmc_read_can_inbit(cardno, node, bitno);
            }

            return (state == IO_STATUS.ON);
        }

        public bool WriteState(int id, IO_STATUS state)
        {
            IOParameter parameter = OutBitList.First(it => it.RegID == id);
            bool flag = false;
            ushort cardno = parameter.Cardno;
            ushort node = parameter.Nodeno;
            ushort bitno = parameter.Bitno;

            if (node == 0)
            {
                short result = LTDMC.dmc_write_outbit(cardno, bitno, (ushort)state);
                if (result == (short)ERR_CODE_DMC.ERR_NOERR) { flag = true; }
            }
            else if (node > 0)
            {
                short result = LTDMC.dmc_write_can_outbit(cardno, node, bitno, (ushort)state);
                if (result == (short)ERR_CODE_DMC.ERR_NOERR) { flag = true; }
            }

            return flag;
        }

        public bool ReadOut(int id)
        {
            IOParameter parameter = OutBitList.First(it => it.RegID == id);
            IO_STATUS state = IO_STATUS.OFF;
            ushort cardno = parameter.Cardno;
            ushort node = parameter.Nodeno;
            ushort bitno = parameter.Bitno;

            if (node == 0)//控制卡本体IO
            {
                state = (IO_STATUS)LTDMC.dmc_read_outbit(cardno, bitno);
            }
            else if (node > 0)//拓展模块上的IO
            {
                state = (IO_STATUS)LTDMC.dmc_read_can_outbit(cardno, node, bitno);
            }

            return (state == IO_STATUS.ON);
        }

        public bool SetServoPower(int id, ushort value)
        {
            var axis = GetAxis(id);
            if (axis == null) return false;
            if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_write_sevon_pin(axis.CardID, axis.AxisID, value))
            {
                return true;
            }
            else
            {
                var handler = ErrorEvent;
                handler?.Invoke(this, new DMC_ErrMsg(CardErr.Low, $"设置卡{axis.Name}使能错误"));
            }
            return false;
        }

        public bool GetServoPower(int id)
        {
            var axis = GetAxis(id);
            if (axis == null) return false;
            short res = LTDMC.dmc_read_sevon_pin(axis.CardID, axis.AxisID);

            return (res == 0);
        }

        public abstract double GetPosition(int id);
        public abstract bool SetPosition(int id, double value);
        public abstract double GetSpeed(int id);


        #region 运动控制函数由子类实现

        public abstract bool VMove(int id, double speed);
        public abstract bool AbsMove(int id, double speed, double destination);
        public abstract bool RelMove(int id, double speed, double destination);
        public abstract Task<bool> ProcessHomeMove(int id, double speed, double plus);

        #endregion

        public virtual bool Stop(int id)
        {
            AxisParameter axis = AxisList.Where(it => it.RegID == id).First();
            if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_stop(axis.CardID, axis.AxisID, 0))
            {
                return true;
            }
            else
            {
                var handler = ErrorEvent;
                handler?.Invoke(this, new DMC_ErrMsg(CardErr.High, $"{axis.Name}停止错误"));
            }
            return false;
        }

        public bool EmgStop(ushort cardno)
        {
            if ((ushort)ERR_CODE_DMC.ERR_NOERR == LTDMC.dmc_emg_stop(cardno))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 事件统一触发
        /// </summary>
        /// <param name="msg"></param>
        public void OnErrorEvent(DMC_ErrMsg msg)
        {
            var handler = ErrorEvent;
            handler?.Invoke(this, msg);
        }

        #region 内部辅助函数

        internal AxisParameter GetAxis(int regId)
        {
            if (AxisList == null)
            {
                return null;
            }
            var axis = AxisList.FirstOrDefault(it => it.RegID == regId);
            if (axis == null)
            {
            }
            return axis;
        }

        internal IOParameter GetIO(IList<IOParameter> list, int regId)
        {
            if (list == null)
            {
                return null;
            }
            var p = list.FirstOrDefault(it => it.RegID == regId);
            return p;
        }

        #endregion
    }
}
