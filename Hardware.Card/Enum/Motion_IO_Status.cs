using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Card.Models
{
    /// <summary>
    /// 轴IO状态
    /// </summary>
    public enum Motion_IO_Status
    {
        ALM = 0,
        PEL = 1,
        NEL = 2,
        EMG = 3,
        ORG = 4,
        PSL = 6,
        NSL = 7,
        INP = 8,
        EZ = 9,
        RDY = 10,
        DSTOP = 11
    }
}
