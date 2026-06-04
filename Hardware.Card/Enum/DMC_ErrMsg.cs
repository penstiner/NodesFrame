using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Card.Models
{
    /// <summary>
    /// 控制卡函数执行错误
    /// </summary>
    public enum CardErr
    {
        Low = 1,
        Medium,
        High
    }

    /// <summary>
    /// 控制卡错误信息
    /// </summary>
    public class DMC_ErrMsg
    {
        public CardErr CardErr { get; set; }
        public string Msg { get; set; }
        public DMC_ErrMsg(CardErr cardErr, string msg)
        {
            CardErr = CardErr;
            Msg = msg;
        }
    }
}
