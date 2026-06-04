using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Card.Models
{
    /// <summary>
    /// 错误码
    /// </summary>
    public enum ERR_CODE_DMC
    {
        ERR_NOERR = 0,          //成功      
        ERR_UNKNOWN = 1,        //未知错误
        ERR_PARAERR = 2,        //参数错误

        ERR_TIMEOUT = 3,        //操作超时
        ERR_CONTROLLERBUSY = 4, //控制卡状态忙

        ERR_CONTILINE = 6,      //连续插补错误
        ERR_CANNOT_CONNECTETH = 8,//无法连接错误
        ERR_HANDLEERR = 9,      //卡号错误
        ERR_SENDERR = 10,       //数据传输错误
        ERR_FIRMWAREERR = 12, //固件文件错误
        ERR_FIRMWAR_MISMATCH = 14, //固件不匹配

        ERR_FIRMWARE_INVALID_PARA = 20,  //固件参数错误
        ERR_FIRMWARE_PARA_ERR = 20,  //固件参数错误2
        ERR_FIRMWARE_STATE_ERR = 22, //固件当前状态不允许操作
        ERR_FIRMWARE_CARD_NOT_SUPPORT = 24,  //固件不支持的功能 控制器不支持的功能
        ERR_FIRMWARE_LIB_NOTSUPPORT = 24,    //固件不支持的功能2
    }
}
