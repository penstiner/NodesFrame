using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Card.Models
{
    public class AxisParameter
    {
        public int RegID { get; set; }
        public string Name { get; set; }
        public ushort CardID { get; set; }
        public ushort AxisID { get; set; }
        public object Threadlock { get; set; } = new object();
        public double AccTime { get; set; } = 0.2;
        /// <summary>
        /// 启动速度
        /// </summary>
        public double StartSpeed { get; set; } = 20;
        /// <summary>
        /// 复位速度
        /// </summary>
        public double HomeSpeed { get; set; } = 40;
        /// <summary>
        /// 复位距离
        /// </summary>
        public double HomeDis { get; set; } = 20;
        /// <summary>
        /// 脉冲当量
        /// </summary>
        public double Equiv { get; set; } = 1;
        public int HomingStep { get; set; } = -1;
        /// <summary>
        /// 复位条件
        /// </summary>
        public bool ORGStatus { get; set; } = false;
        /// <summary>
        /// 回原完成标志
        /// </summary>
        public bool HomeOK { get; set; } = false;
    }
}
