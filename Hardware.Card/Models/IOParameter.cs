using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Card.Models
{
    public class IOParameter
    {
        public int RegID { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 卡号
        /// </summary>
        public ushort Cardno { get; set; }
        /// <summary>
        /// 节点号
        /// </summary>
        public ushort Nodeno { get; set; }
        /// <summary>
        /// 端口号
        /// </summary>
        public ushort Bitno { get; set; }
    }
}
