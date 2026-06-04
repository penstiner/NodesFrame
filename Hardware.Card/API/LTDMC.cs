using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace csLTDMC //命名空间根据应用程序修改
{
    public delegate uint DMC3K5K_OPERATE(IntPtr operate_data); 
    public partial class LTDMC
    {
        //设置和读取打印模式（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_debug_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_debug_mode(UInt16 mode, string FileName);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_debug_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_debug_mode(ref UInt16 mode, IntPtr FileName);
        //---------------------   板卡初始和配置函数  ----------------------
        //初始化控制卡（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_board_init", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_board_init();
        //硬件复位（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_board_reset", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_board_reset();
        //关闭控制卡（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_board_close", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_board_close();
        //控制卡热复位（适用于EtherCAT、RTEX总线卡）  
        [DllImport("LTDMC.dll")]
        public static extern short dmc_soft_reset(ushort CardNo);
        //控制卡冷复位（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_cool_reset(ushort CardNo);
        //控制卡初始复位（适用于EtherCAT总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_original_reset", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_original_reset(ushort CardNo);
        //读取控制卡信息列表（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_CardInfList", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_CardInfList(ref UInt16 CardNum, UInt32[] CardTypeList, UInt16[] CardIdList);
        //读取发布版本号（适用于DMC3000/DMC5X10系列脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_card_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_card_version(UInt16 CardNo, ref UInt32 CardVersion);
        //读取控制卡硬件的固件版本（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_card_soft_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_card_soft_version(UInt16 CardNo, ref UInt32 FirmID, ref UInt32 SubFirmID);
        //读取控制卡动态库版本（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_card_lib_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_card_lib_version(ref UInt32 LibVer);
        //读取发布版本号（适用于DMC3000/DMC5X10系列脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_release_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_release_version(ushort ConnectNo, byte[] ReleaseVersion);
        //读取指定卡轴数（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_total_axes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_total_axes(UInt16 CardNo, ref UInt32 TotalAxis);
        //获取本地IO点数（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_total_ionum", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_total_ionum(ushort CardNo, ref ushort TotalIn, ref ushort TotalOut);
        //获取本地ADDA输入输出数（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_total_adcnum", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_total_adcnum(ushort CardNo, ref ushort TotalIn, ref ushort TotalOut);
        //读取指定卡插补坐标系数（保留）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_total_liners", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_total_liners(UInt16 CardNo, ref UInt32 TotalLiner);
        //定制类（保留）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_board_init_onecard", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_board_init_onecard(ushort CardNo);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_board_close_onecard", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_board_close_onecard(ushort CardNo);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_board_reset_onecard", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_board_reset_onecard(ushort CardNo);

        //密码函数（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_sn", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_sn(UInt16 CardNo, string new_sn);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_check_sn", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_check_sn(UInt16 CardNo, string check_sn);
        //登入sn20191101（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_enter_password_ex(UInt16 CardNo, string str_pass);

        //---------------------运动模块脉冲模式------------------
        //脉冲模式（适用于所有脉冲卡）	
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_pulse_outmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_pulse_outmode(UInt16 CardNo, UInt16 axis, UInt16 outmode);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_pulse_outmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_pulse_outmode(UInt16 CardNo, UInt16 axis, ref UInt16 outmode);
        //脉冲当量（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_equiv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_equiv(UInt16 CardNo, UInt16 axis, ref double equiv);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_equiv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_equiv(UInt16 CardNo, UInt16 axis, double equiv);
        //反向间隙(脉冲)（适用于DMC5000系列脉冲卡）	
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_backlash_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_backlash_unit(UInt16 CardNo, UInt16 axis, double backlash); 
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_backlash_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_backlash_unit(UInt16 CardNo, UInt16 axis, ref double backlash);

        //通用文件下载
        [DllImport("LTDMC.dll", EntryPoint = "dmc_download_file", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_download_file(ushort CardNo, string pfilename, byte[] pfilenameinControl, ushort filetype);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_upload_file", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_upload_file(ushort CardNo, string pfilename, byte[] pfilenameinControl, ushort filetype);
        //下载内存文件 总线卡（适用于EtherCAT总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_download_memfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_download_memfile(ushort CardNo, byte[] pbuffer, uint buffsize, byte[] pfilenameinControl, ushort filetype);
        //上传内存文件（适用于EtherCAT总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_upload_memfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_upload_memfile(ushort CardNo, byte[] pbuffer, uint buffsize, byte[] pfilenameinControl, ref uint puifilesize, ushort filetype);
        //文件进度（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_progress(ushort CardNo, ref float process);
        //下载参数文件（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_download_configfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_download_configfile(UInt16 CardNo, String FileName);
        //下载固件文件（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_download_firmware", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_download_firmware(UInt16 CardNo, String FileName);

        //----------------------限位异常设置-------------------------------	
        //设置读取软限位参数（适用于E3032总线卡、R3032总线卡、DMC3000/5000/5X10系列脉冲卡）	
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_softlimit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_softlimit(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 source_sel, UInt16 SL_action, Int32 N_limit, Int32 P_limit);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_softlimit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_softlimit(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 source_sel, ref UInt16 SL_action, ref Int32 N_limit, ref Int32 P_limit);
        //设置读取软限位参数unit（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_softlimit_unit(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 source_sel, UInt16 SL_action, double N_limit, double P_limit);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_softlimit_unit(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 source_sel, ref UInt16 SL_action, ref double N_limit, ref double P_limit);
        //设置读取EL信号（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_el_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_el_mode(UInt16 CardNo, UInt16 axis, UInt16 el_enable, UInt16 el_logic, UInt16 el_mode);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_el_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_el_mode(UInt16 CardNo, UInt16 axis, ref UInt16 el_enable, ref UInt16 el_logic, ref UInt16 el_mode);
        //设置读取EMG信号（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_emg_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_emg_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 emg_logic);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_emg_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_emg_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enbale, ref UInt16 emg_logic);
        //外部减速停止信号及减速停止时间设置，毫秒为单位（保留）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_dstp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_dstp_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 logic, UInt32 time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_dstp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_dstp_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 logic, ref UInt32 time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_dstp_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_dstp_time(UInt16 CardNo, UInt16 axis, UInt32 time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_dstp_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_dstp_time(UInt16 CardNo, UInt16 axis, ref UInt32 time);
        //外部减速停止信号及减速停止时间设置，秒为单位（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_io_dstp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_io_dstp_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 logic);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_io_dstp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_io_dstp_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 logic);
        //点位运动减速停止时间设置读取（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_dec_stop_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_dec_stop_time(UInt16 CardNo, UInt16 axis, double stop_time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_dec_stop_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_dec_stop_time(UInt16 CardNo, UInt16 axis, ref double stop_time);
        //插补减速停止信号和减速时间设置（适用于DMC5X10系列脉冲卡、EthreCAT总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_vector_dec_stop_time(UInt16 CardNo, UInt16 Crd, double stop_time);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_vector_dec_stop_time(UInt16 CardNo, UInt16 Crd, ref double stop_time);
        //IO减速停止距离（适用于DMC3000、DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_dec_stop_dist(UInt16 CardNo, UInt16 axis, Int32 dist);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_dec_stop_dist(UInt16 CardNo, UInt16 axis, ref Int32 dist);
        //IO减速停止，支持pmove/vmove运动（适用于DMC3000、DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_io_exactstop(UInt16 CardNo, UInt16 axis, UInt16 ioNum, UInt16[] ioList, UInt16 enable, UInt16 valid_logic, UInt16 action, UInt16 move_dir);       
        //设置通用输入口的一位减速停止IO口（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_io_dstp_bitno(UInt16 CardNo, UInt16 axis, UInt16 bitno, double filter);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_io_dstp_bitno(UInt16 CardNo, UInt16 axis, ref UInt16 bitno, ref double filter);

        //---------------------------单轴运动----------------------
        //设定读取速度曲线参数	（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_profile(UInt16 CardNo, UInt16 axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double stop_vel);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_profile(UInt16 CardNo, UInt16 axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double stop_vel);
        //速度设置(脉冲当量)（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）	
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_profile_unit(UInt16 CardNo, UInt16 Axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);   //单轴速度参数
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_profile_unit(UInt16 CardNo, UInt16 Axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);
        //速度曲线设置，加速度值表示(脉冲)（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_acc_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_acc_profile(UInt16 CardNo, UInt16 axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double stop_vel);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_acc_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_acc_profile(UInt16 CardNo, UInt16 axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double stop_vel);
        //速度曲线设置，加速度值表示(当量)（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_profile_unit_acc", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_profile_unit_acc(UInt16 CardNo, UInt16 Axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);   //单轴速度参数
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_profile_unit_acc", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_profile_unit_acc(UInt16 CardNo, UInt16 Axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);      
        //设置读取平滑速度曲线参数（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_s_profile(UInt16 CardNo, UInt16 axis, UInt16 s_mode, double s_para);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_s_profile(UInt16 CardNo, UInt16 axis, UInt16 s_mode, ref double s_para);            
        //点位运动(脉冲)（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_pmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_pmove(UInt16 CardNo, UInt16 axis, Int32 Dist, UInt16 posi_mode);
        //点位运动(当量)（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_pmove_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_pmove_unit(UInt16 CardNo, UInt16 axis, double Dist, UInt16 posi_mode);  
        //指定轴做定长位移运动 同时发送速度和S时间(脉冲)（适用于DMC5X10系列脉冲卡）	
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pmove_extern(UInt16 CardNo, UInt16 axis, double dist, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double stop_Vel, double s_para, UInt16 posi_mode);
        //在线变位(脉冲)，运动中改变目标位置（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_reset_target_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_reset_target_position(UInt16 CardNo, UInt16 axis, Int32 dist, UInt16 posi_mode);
        //变速变位(当量)（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_reset_target_position_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_reset_target_position_unit(UInt16 CardNo, UInt16 Axis, double New_Pos); 
        //在线变速(脉冲)，运动中改变指定轴的当前运动速度（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_change_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_change_speed(UInt16 CardNo, UInt16 axis, double Curr_Vel, double Taccdec);
        //在线变速(当量)，运动中改变指定轴的当前运动速度（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_change_speed_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_change_speed_unit(UInt16 CardNo, UInt16 Axis, double New_Vel, double Taccdec);    
        //无论运动与否强行改变目标位置（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_update_target_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_update_target_position(UInt16 CardNo, UInt16 axis, Int32 dist, UInt16 posi_mode);
        //强行变位扩展（适用于DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_update_target_position_extern(UInt16 CardNo, UInt16 axis, double mid_pos, double aim_pos, double vel, UInt16 posi_mode);
        //在线变速(当量)，运动中改变指定轴的当前运动速度（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_update_target_position_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_update_target_position_unit(UInt16 CardNo, UInt16 Axis, double New_Pos);           
        //---------------------JOG运动--------------------
        //单轴连续速度运动（适用于所有脉冲/总线卡）	
        [DllImport("LTDMC.dll", EntryPoint = "dmc_vmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_vmove(UInt16 CardNo, UInt16 axis, UInt16 dir);

        //---------------------插补运动--------------------
        //插补速度设置(脉冲)（适用于DMC3000系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_vector_profile_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_vector_profile_multicoor(UInt16 CardNo, UInt16 Crd, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_vector_profile_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_vector_profile_multicoor(UInt16 CardNo, UInt16 Crd, ref double Min_Vel, ref double Max_Vel, ref double Taccdec, ref double Tdec, ref double Stop_Vel);       
        //设置读取平滑速度曲线参数（适用于DMC3000系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_vector_s_profile_multicoor(UInt16 CardNo, UInt16 Crd, UInt16 s_mode, double s_para);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_vector_s_profile_multicoor(UInt16 CardNo, UInt16 Crd, UInt16 s_mode, ref double s_para);
        //插补速度参数(当量)（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_vector_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_vector_profile_unit(UInt16 CardNo, UInt16 Crd, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);   //单段插补速度参数
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_vector_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_vector_profile_unit(UInt16 CardNo, UInt16 Crd, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);
        //设置平滑速度曲线参数（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_vector_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_vector_s_profile(UInt16 CardNo, UInt16 Crd, UInt16 s_mode, double s_para);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_vector_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_vector_s_profile(UInt16 CardNo, UInt16 Crd, UInt16 s_mode, ref double s_para);
        //直线插补运动（适用于DMC3000系列脉冲卡）	
        [DllImport("LTDMC.dll", EntryPoint = "dmc_line_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_line_multicoor(UInt16 CardNo, UInt16 crd, UInt16 axisNum, UInt16[] axisList, Int32[] DistList, UInt16 posi_mode);
        //圆弧插补运动（适用于DMC3000系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_arc_move_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_arc_move_multicoor(UInt16 CardNo, UInt16 crd, UInt16[] AxisList, Int32[] Target_Pos, Int32[] Cen_Pos, UInt16 Arc_Dir, UInt16 posi_mode);
        //直线插补(当量)（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_line_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_line_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, UInt16 posi_mode);    //单段直线
        //圆心圆弧插补(当量)（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_arc_move_center_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_arc_move_center_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double[] Cen_Pos, UInt16 Arc_Dir, Int32 Circle, UInt16 posi_mode);     //圆心终点式圆弧/螺旋线/渐开线
        //半径圆弧插补(当量)（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_arc_move_radius_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_arc_move_radius_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double Arc_Radius, UInt16 Arc_Dir, Int32 Circle, UInt16 posi_mode);    //半径终点式圆弧/螺旋线
        //三点圆弧插补(当量)（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_arc_move_3points_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_arc_move_3points_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double[] Mid_Pos, Int32 Circle, UInt16 posi_mode);     //三点式圆弧/螺旋线
        //矩形插补(当量)（适用于EtherCAT总线卡、RTEX总线卡、DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_rectangle_move_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_rectangle_move_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] TargetPos, double[] MaskPos, Int32 Count, UInt16 rect_mode, UInt16 posi_mode);     //矩形区域插补，单段插补指令

        //----------------------PVT运动---------------------------
        //PVT运动旧版 （适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_PvtTable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_PvtTable(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, Int32[] pPos, double[] pVel);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_PtsTable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_PtsTable(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, Int32[] pPos, double[] pPercent);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_PvtsTable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_PvtsTable(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, Int32[] pPos, double velBegin, double velEnd);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_PttTable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_PttTable(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, int[] pPos);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_PvtMove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_PvtMove(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList);
        //PVT缓冲区添加
        [DllImport("LTDMC.dll")]
        public static extern short dmc_PttTable_add(UInt16 CardNo, UInt16 iaxis, UInt16 count, double[] pTime, long[] pPos);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_PtsTable_add(UInt16 CardNo, UInt16 iaxis, UInt16 count, double[] pTime, long[] pPos, double[] pPercent);
        //读取pvt剩余空间
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pvt_get_remain_space(UInt16 CardNo, UInt16 iaxis);
        //PVT运动 总线卡新规划，适用于EtherCAT总线卡
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pvt_table_unit(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, double[] pPos, double[] pVel);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pts_table_unit(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, double[] pPos, double[] pPercent);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pvts_table_unit(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, double[] pPos, double velBegin, double velEnd);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ptt_table_unit(UInt16 CardNo, UInt16 iaxis, UInt32 count, double[] pTime, double[] pPos);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pvt_move(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList);
        //其它类（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_SetGearProfile(UInt16 CardNo, UInt16 axis, UInt16 MasterType, UInt16 MasterIndex, Int32 MasterEven, Int32 SlaveEven, UInt32 MasterSlope);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_GetGearProfile(UInt16 CardNo, UInt16 axis, ref UInt16 MasterType, ref UInt16 MasterIndex, ref UInt32 MasterEven, ref UInt32 SlaveEven, ref UInt32 MasterSlope);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_GearMove(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList);
              
        //--------------------回零运动---------------------
        //设置读取HOME信号（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_home_pin_logic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_home_pin_logic(UInt16 CardNo, UInt16 axis, UInt16 org_logic, double filter);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_home_pin_logic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_home_pin_logic(UInt16 CardNo, UInt16 axis, ref UInt16 org_logic, ref double filter);
        //设定读取指定轴的回原点模式（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_homemode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_homemode(UInt16 CardNo, UInt16 axis, UInt16 home_dir, double vel, UInt16 mode, UInt16 EZ_count);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_homemode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_homemode(UInt16 CardNo, UInt16 axis, ref UInt16 home_dir, ref double vel, ref UInt16 home_mode, ref UInt16 EZ_count);
        //设置回零遇限位是否反找（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_home_el_return(UInt16 CardNo, UInt16 axis, UInt16 enable);
        //读取参数遇限位反找使能（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_el_return(UInt16 CardNo, UInt16 axis, ref UInt16 enable);
        //启动回零（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_home_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_home_move(UInt16 CardNo, UInt16 axis);
        //设置读取回零速度参数（适用于Rtex总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_home_profile_unit(ushort CardNo, ushort axis, double Low_Vel, double High_Vel, double Tacc, double Tdec);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_profile_unit(ushort CardNo, ushort axis, ref double Low_Vel, ref double High_Vel, ref double Tacc, ref double Tdec);
        //读取回零执行状态（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_result(UInt16 CardNo, UInt16 axis, ref UInt16 state);
        //设置读取回零偏移量及清零模式（适用于DMC5X10脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_home_position_unit(UInt16 CardNo, UInt16 axis, UInt16 enable, double position);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_position_unit(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref double position);
        //（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_el_home(UInt16 CardNo, UInt16 axis, UInt16 mode);
        //回零偏移模式函数（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_home_shift_param(UInt16 CardNo, UInt16 axis, UInt16 pos_clear_mode, double ShiftValue);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_shift_param(UInt16 CardNo, UInt16 axis, ref UInt16 pos_clear_mode, ref double ShiftValue);
        //设置回零偏移量及偏移模式（适用于DMC3000系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_home_position(UInt16 CardNo, UInt16 axis, UInt16 enable, double position);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_position(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref double position);
        //设置回零限位距离（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_home_soft_limit(UInt16 CardNo, UInt16 Axis, Int32 N_limit, Int32 P_limit);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_soft_limit(UInt16 CardNo, UInt16 Axis, ref Int32 N_limit, ref Int32 P_limit);
       
        //--------------------原点锁存-------------------
        //设置读取EZ锁存模式（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_homelatch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_homelatch_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 logic, UInt16 source);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_homelatch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_homelatch_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 logic, ref UInt16 source);
        //读取原点锁存标志（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_homelatch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_homelatch_flag(UInt16 CardNo, UInt16 axis);
        //清除原点锁存标志（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_reset_homelatch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_reset_homelatch_flag(UInt16 CardNo, UInt16 axis);
        //读取原点锁存值（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_homelatch_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 dmc_get_homelatch_value(UInt16 CardNo, UInt16 axis);
        //读取原点锁存值（unit）（适用于DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_homelatch_value_unit(UInt16 CardNo, UInt16 axis, ref double pos);

        //--------------------EZ锁存-------------------
        //设置读取EZ锁存模式（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_ezlatch_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 logic, UInt16 source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_ezlatch_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 logic, ref UInt16 source);
        //读取EZ锁存标志（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_ezlatch_flag(UInt16 CardNo, UInt16 axis);
        //清除EZ锁存标志（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_reset_ezlatch_flag(UInt16 CardNo, UInt16 axis);
        //读取EZ锁存值（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern Int32 dmc_get_ezlatch_value(UInt16 CardNo, UInt16 axis);
        //读取EZ锁存值（unit）（适用于DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_ezlatch_value_unit(UInt16 CardNo, UInt16 axis, ref double pos);

        //--------------------手轮运动---------------------	
        //设置读取手轮通道（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_handwheel_channel", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_handwheel_channel(UInt16 CardNo, UInt16 index);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_handwheel_channel", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_handwheel_channel(UInt16 CardNo, ref UInt16 index);
        //设置读取单轴手轮脉冲信号的工作方式（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_handwheel_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_handwheel_inmode(UInt16 CardNo, UInt16 axis, UInt16 inmode, Int32 multi, double vh);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_handwheel_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_handwheel_inmode(UInt16 CardNo, UInt16 axis, ref UInt16 inmode, ref Int32 multi, ref double vh);
        //设置读取单轴手轮脉冲信号的工作方式，浮点型倍率（适用于DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_handwheel_inmode_decimals", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_handwheel_inmode_decimals(UInt16 CardNo, UInt16 axis, UInt16 inmode, double multi, double vh);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_handwheel_inmode_decimals", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_handwheel_inmode_decimals(UInt16 CardNo, UInt16 axis, ref UInt16 inmode, ref double multi, ref double vh);
        //设置读取多轴手轮脉冲信号的工作方式（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_handwheel_inmode_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_handwheel_inmode_extern(UInt16 CardNo, UInt16 inmode, UInt16 AxisNum, UInt16[] AxisList, Int32[] multi);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_handwheel_inmode_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_handwheel_inmode_extern(UInt16 CardNo, ref UInt16 inmode, ref UInt16 AxisNum, UInt16[] AxisList, Int32[] multi);
        //设置读取单轴手轮脉冲信号的工作方式，浮点型倍率（适用于DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_handwheel_inmode_extern_decimals(UInt16 CardNo, UInt16 inmode, UInt16 AxisNum, UInt16[] AxisList, double[] multi);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_handwheel_inmode_extern_decimals(UInt16 CardNo, ref UInt16 inmode, ref UInt16 AxisNum, UInt16[] AxisList, double[] multi);
        //启动手轮运动（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_handwheel_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_handwheel_move(UInt16 CardNo, UInt16 axis);    
        //手轮运动 新增总线的手轮模式  (保留)
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_set_axislist(UInt16 CardNo, UInt16 AxisSelIndex, UInt16 AxisNum, UInt16[] AxisList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_get_axislist(UInt16 CardNo, UInt16 AxisSelIndex, ref UInt16 AxisNum, UInt16[] AxisList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_set_ratiolist(UInt16 CardNo, UInt16 AxisSelIndex, UInt16 StartRatioIndex, UInt16 RatioSelNum, double[] RatioList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_get_ratiolist(UInt16 CardNo, UInt16 AxisSelIndex, UInt16 StartRatioIndex, UInt16 RatioSelNum, double[] RatioList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_set_mode(UInt16 CardNo, UInt16 InMode, UInt16 IfHardEnable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_get_mode(UInt16 CardNo, ref UInt16 InMode, ref UInt16 IfHardEnable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_set_index(UInt16 CardNo, UInt16 AxisSelIndex, UInt16 RatioSelIndex);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_get_index(UInt16 CardNo, ref UInt16 AxisSelIndex, ref UInt16 RatioSelIndex);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_stop(UInt16 CardNo);
        
        //-------------------------高速锁存-------------------
        //设置读取指定轴的LTC信号（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_ltc_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_ltc_mode(UInt16 CardNo, UInt16 axis, UInt16 ltc_logic, UInt16 ltc_mode, Double filter);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_ltc_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_ltc_mode(UInt16 CardNo, UInt16 axis, ref UInt16 ltc_logic, ref UInt16 ltc_mode, ref Double filter);
        //设置读到锁存方式（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_latch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_latch_mode(UInt16 CardNo, UInt16 axis, UInt16 all_enable, UInt16 latch_source, UInt16 triger_chunnel);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_latch_mode(UInt16 CardNo, UInt16 axis, ref UInt16 all_enable, ref UInt16 latch_source, ref UInt16 triger_chunnel);
        //读取编码器锁存器的值（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 dmc_get_latch_value(UInt16 CardNo, UInt16 axis);
        //读取编码器锁存器的值unit（适用于DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_latch_value_unit(UInt16 CardNo, UInt16 axis, ref double pos_by_mm);
        //读取锁存器标志（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_latch_flag(UInt16 CardNo, UInt16 axis);
        //复位锁存器标志（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_reset_latch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_reset_latch_flag(UInt16 CardNo, UInt16 axis);
        //按索引取值（适用DMC3000系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_value_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 dmc_get_latch_value_extern(UInt16 CardNo, UInt16 axis, UInt16 Index);
        //高速锁存（预留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_latch_value_extern_unit(UInt16 CardNo, UInt16 axis, UInt16 index, ref double pos_by_mm);//按索引取值读取 
        //读取锁存个数（适用DMC3000系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_flag_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_latch_flag_extern(UInt16 CardNo, UInt16 axis);
        //设置读取LTC反相输出（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_SetLtcOutMode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_SetLtcOutMode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 bitno);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_GetLtcOutMode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_GetLtcOutMode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 bitno);
        //LTC端口触发延时急停时间 单位us（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_latch_stop_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_latch_stop_time(UInt16 CardNo, UInt16 axis, Int32 time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_stop_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_latch_stop_time(UInt16 CardNo, UInt16 axis, ref Int32 time);
        //设置/回读LTC端口触发延时急停轴配置（适用于EtherCAT总线系列卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_latch_stop_axis(ushort CardNo, ushort latch, ushort num, ushort[] axislist);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_latch_stop_axis(ushort CardNo, ushort latch, ref ushort num, ushort[] axislist);

        //----------------------高速锁存 总线卡---------------------------
        //配置锁存器：锁存模式0-单次锁存，1-连续锁存；锁存边沿0-下降沿，1-上升沿，2-双边沿；滤波时间，单位us（适用于所有总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_set_mode(ushort CardNo, ushort latch, ushort ltc_mode, ushort ltc_logic, double filter);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_get_mode(ushort CardNo, ushort latch, ref ushort ltc_mode, ref ushort ltc_logic, ref double filter);
        //配置锁存源：0-指令位置，1-编码器反馈位置（适用于所有总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_set_source(ushort CardNo, ushort latch, ushort axis, ushort ltc_source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_get_source(ushort CardNo, ushort latch, ushort axis, ref ushort ltc_source);
        //复位锁存器（适用于所有总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_reset(ushort CardNo, ushort latch);
        //读取锁存个数（适用于所有总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_get_number(ushort CardNo, ushort latch, ushort axis, ref int number);
        //读取锁存值（适用于所有总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_get_value_unit(ushort CardNo, ushort latch, ushort axis, ref double value);

        //-----------------------软锁存 所有卡---------------------------------
        //配置锁存器：锁存模式0-单次锁存，1-连续锁存；锁存边沿0-下降沿，1-上升沿，2-双边沿；滤波时间，单位us（适用于DMC5X10/3000系列脉冲卡、总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_set_mode(ushort ConnectNo, ushort latch, ushort ltc_enable, ushort ltc_mode, ushort ltc_inbit, ushort ltc_logic, double filter);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_get_mode(ushort ConnectNo, ushort latch, ref ushort ltc_enable, ref ushort ltc_mode, ref ushort ltc_inbit, ref ushort ltc_logic, ref double filter);
        //配置锁存源：0-指令位置，1-编码器反馈位置（适用于DMC5X10/3000系列脉冲卡、总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_set_source(ushort ConnectNo, ushort latch, ushort axis, ushort ltc_source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_get_source(ushort ConnectNo, ushort latch, ushort axis, ref ushort ltc_source);
        //复位锁存器（适用于DMC5X10/3000系列脉冲卡、总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_reset(ushort ConnectNo, ushort latch);
        //读取锁存个数（适用于DMC5X10/3000系列脉冲卡、总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_get_number(ushort ConnectNo, ushort latch, ushort axis, ref int number);
        //读取锁存值（适用于DMC5X10系列脉冲卡、所有总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_get_value_unit(ushort ConnectNo, ushort latch, ushort axis, ref double value);

        //----------------------单轴低速位置比较-----------------------	
        //配置读取比较器（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_set_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_set_config(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 cmp_source);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_config(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 cmp_source);
        //清除所有比较点（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_clear_points", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_clear_points(UInt16 CardNo, UInt16 axis);
        //添加比较点（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_add_point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_add_point(UInt16 CardNo, UInt16 axis, int pos, UInt16 dir, UInt16 action, UInt32 actpara);
        //添加比较点（适用于所有DMC5X10脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_unit(UInt16 CardNo, UInt16 cmp, double pos, UInt16 dir, UInt16 action, UInt32 actpara);        
        //添加比较点（适用于E3032/R3032）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_cycle(UInt16 CardNo, UInt16 cmp, Int32 pos, UInt16 dir, UInt32 bitno, UInt32 cycle, UInt16 level);
        //添加比较点unit（适用于E5032）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_cycle_unit(UInt16 CardNo, UInt16 cmp, double pos, UInt16 dir, UInt32 bitno, UInt32 cycle, UInt16 level);
        //读取当前比较点（适用于所有脉冲卡、Rtex总线卡、E3032卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_current_point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_current_point(UInt16 CardNo, UInt16 axis, ref Int32 pos);
        //读取当前比较点（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_get_current_point_unit(UInt16 CardNo, UInt16 cmp, ref double pos);
        //查询已经比较过的点（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_points_runned", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_points_runned(UInt16 CardNo, UInt16 axis, ref Int32 pointNum);
        //查询可以加入的比较点数量（适用于所有脉冲/总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_points_remained", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_points_remained(UInt16 CardNo, UInt16 axis, ref Int32 pointNum);
        
        //-------------------二维低速位置比较-----------------------
        //配置读取比较器（适用于所有脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_set_config_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_set_config_extern(UInt16 CardNo, UInt16 enable, UInt16 cmp_source);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_config_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_config_extern(UInt16 CardNo, ref UInt16 enable, ref UInt16 cmp_source);
        //清除所有比较点（适用于所有脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_clear_points_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_clear_points_extern(UInt16 CardNo);
        //添加两轴位置比较点（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_add_point_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_add_point_extern(UInt16 CardNo, UInt16[] axis, Int32[] pos, UInt16[] dir, UInt16 action, UInt32 actpara);
        //读取当前比较点（适用于所有脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_current_point_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_current_point_extern(UInt16 CardNo, Int32[] pos);
        //读取当前比较点unit（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_get_current_point_extern_unit(UInt16 CardNo, double[] pos);
        //添加两轴位置比较点（适用于DMC5X10脉冲卡）      
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_extern_unit(UInt16 CardNo, UInt16[] axis, double[] pos, UInt16[] dir, UInt16 action, UInt32 actpara);
        //添加二维低速位置比较点（适用于EtherCAT总线系列卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_cycle_2d(ushort CardNo, ushort[] axis, double[] pos, ushort[] dir, uint bitno, uint cycle, ushort level);
        //查询已经比较过的点（适用于所有脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_points_runned_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_points_runned_extern(UInt16 CardNo, ref Int32 pointNum);
        //查询可以加入的比较点数量（适用于所有脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_points_remained_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_points_remained_extern(UInt16 CardNo, ref Int32 pointNum);
        //多组位置比较（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_set_config_multi(UInt16 CardNo, UInt16 queue, UInt16 enable, UInt16 axis, UInt16 cmp_source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_get_config_multi(UInt16 CardNo, UInt16 queue, ref UInt16 enable, ref UInt16 axis, ref UInt16 cmp_source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_multi(UInt16 CardNo, UInt16 cmp, Int32 pos, UInt16 dir, UInt16 action, UInt32 actpara, double times);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_multi_unit(UInt16 CardNo, UInt16 cmp, double pos, UInt16 dir, UInt16 action, UInt32 actpara, double times);//添加比较点 增强
        
        //----------- 单轴高速位置比较-----------------------        
        //设置读取高速比较模式（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_set_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_set_mode(UInt16 CardNo, UInt16 hcmp, UInt16 cmp_enable);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_get_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_get_mode(UInt16 CardNo, UInt16 hcmp, ref UInt16 cmp_enable);
        //设置高速比较参数（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_set_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_set_config(UInt16 CardNo, UInt16 hcmp, UInt16 axis, UInt16 cmp_source, UInt16 cmp_logic, Int32 time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_get_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_get_config(UInt16 CardNo, UInt16 hcmp, ref UInt16 axis, ref UInt16 cmp_source, ref UInt16 cmp_logic, ref Int32 time);
        //高速比较模式扩展（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_set_config_extern(UInt16 CardNo, UInt16 hcmp, UInt16 axis, UInt16 cmp_source, UInt16 cmp_logic, UInt16 cmp_mode, Int32 dist, Int32 time);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_get_config_extern(UInt16 CardNo, UInt16 hcmp, ref UInt16 axis, ref UInt16 cmp_source, ref UInt16 cmp_logic, ref UInt16 cmp_mode, ref Int32 dist, ref Int32 time);
        //添加比较点（适用于所有脉冲卡、E3032总线卡、R3032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_add_point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_add_point(UInt16 CardNo, UInt16 hcmp, Int32 cmp_pos);
        //添加比较点unit（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_add_point_unit(UInt16 CardNo, UInt16 hcmp, double cmp_pos);       
        //设置读取线性模式参数（适用于所有脉冲卡、E3032总线卡、R3032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_set_liner", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_set_liner(UInt16 CardNo, UInt16 hcmp, Int32 Increment, Int32 Count);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_get_liner", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_get_liner(UInt16 CardNo, UInt16 hcmp, ref Int32 Increment, ref Int32 Count);
        //设置线性模式参数（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_set_liner_unit(UInt16 CardNo, UInt16 hcmp, double Increment, Int32 Count);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_get_liner_unit(UInt16 CardNo, UInt16 hcmp, ref double Increment, ref Int32 Count);
        //读取高速比较状态（适用于所有脉冲卡、E3032总线卡、R3032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_get_current_state", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_get_current_state(UInt16 CardNo, UInt16 hcmp, ref Int32 remained_points, ref Int32 current_point, ref Int32 runned_points);
        //读取高速比较状态（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_get_current_state_unit(UInt16 CardNo, UInt16 hcmp, ref Int32 remained_points, ref double current_point, ref Int32 runned_points); //读取高速比较状态
        //清除比较点（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_clear_points", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_clear_points(UInt16 CardNo, UInt16 hcmp);
        //读取指定CMP端口的电平（保留）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_cmp_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_cmp_pin(UInt16 CardNo, UInt16 hcmp);
        //控制cmp端口输出（保留）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_cmp_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_cmp_pin(UInt16 CardNo, UInt16 hcmp, UInt16 on_off);
        //1、	启用缓存方式添加比较位置：（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_set_mode(UInt16 CardNo, UInt16 hcmp, UInt16 fifo_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_get_mode(UInt16 CardNo, UInt16 hcmp, ref UInt16 fifo_mode);
        //2、	读取剩余缓存状态，上位机通过此函数判断是否继续添加比较位置（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_get_state(UInt16 CardNo, UInt16 hcmp, ref long remained_points);
        //3、	按数组的方式批量添加比较位置（适用于DMC5000系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_add_point_unit(UInt16 CardNo, UInt16 hcmp, UInt16 num, double[] cmp_pos);
        //4、	清除比较位置,也会把FPGA的位置同步清除掉（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_clear_points(UInt16 CardNo, UInt16 hcmp);
        //添加大数据，会堵塞一段时间，指导数据添加完成（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_add_table(UInt16 CardNo, UInt16 hcmp, UInt16 num, double[] cmp_pos);
        //一维高速比较，队列模式添加的比较点关联运动方向，添加少量数据（适用于DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_add_point_dir_unit(ushort CardNo, ushort hcmp, ushort num, double[] cmp_pos, uint dir);
        //一维高速比较，队列模式添加的比较点关联运动方向，添加大量数据（适用于DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_add_table_dir(ushort CardNo, ushort hcmp, ushort num, double[] cmp_pos, uint dir);
        //----------- 二维高速位置比较-----------------------        
        //设置读取高速比较使能（适用于所有脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_set_enable(UInt16 CardNo, UInt16 hcmp, UInt16 cmp_enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_get_enable(UInt16 CardNo, UInt16 hcmp, ref UInt16 cmp_enable);
        //配置读取二维高速比较器（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_set_config(UInt16 CardNo, UInt16 hcmp, UInt16 cmp_mode, UInt16 x_axis, UInt16 x_cmp_source, UInt16 y_axis, UInt16 y_cmp_source, Int32 error, UInt16 cmp_logic, Int32 time, UInt16 pwm_enable, double duty, Int32 freq, UInt16 port_sel, UInt16 pwm_number);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_get_config(UInt16 CardNo, UInt16 hcmp, ref UInt16 cmp_mode, ref UInt16 x_axis, ref UInt16 x_cmp_source, ref UInt16 y_axis, ref UInt16 y_cmp_source, ref Int32 error, ref UInt16 cmp_logic, ref Int32 time, ref UInt16 pwm_enable, ref double duty, ref Int32 freq, ref UInt16 port_sel, ref UInt16 pwm_number);
        //配置读取二维高速比较器（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_set_config_unit(UInt16 CardNo, UInt16 hcmp, UInt16 cmp_mode, UInt16 x_axis, UInt16 x_cmp_source, double x_cmp_error, UInt16 y_axis, UInt16 y_cmp_source, double y_cmp_error, UInt16 cmp_logic, int time);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_get_config_unit(UInt16 CardNo, UInt16 hcmp, ref UInt16 cmp_mode, ref UInt16 x_axis, ref UInt16 x_cmp_source, ref double x_cmp_error, ref UInt16 y_axis, ref UInt16 y_cmp_source, ref double y_cmp_error, ref UInt16 cmp_logic, ref int time);
        //添加二维高速位置比较点（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_add_point(UInt16 CardNo, UInt16 hcmp, Int32 x_cmp_pos, Int32 y_cmp_pos);
        //添加二维高速位置比较点unit（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_add_point_unit(UInt16 CardNo, UInt16 hcmp, double x_cmp_pos, double y_cmp_pos, UInt16 cmp_outbit);
        //读取二维高速比较参数（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_get_current_state(UInt16 CardNo, UInt16 hcmp, ref Int32 remained_points, ref Int32 x_current_point, ref Int32 y_current_point, ref Int32 runned_points, ref UInt16 current_state);
        //读取二维高速比较参数（适用于DMC5X10系列脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_get_current_state_unit(UInt16 CardNo, UInt16 hcmp, ref int remained_points, ref double x_current_point, ref double y_current_point, ref int runned_points, ref UInt16 current_state, ref UInt16 current_outbit); 
        //清除二维高速位置比较点（适用于所有脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_clear_points(UInt16 CardNo, UInt16 hcmp);
        //强制二维高速比较输出（适用于所有脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_force_output(UInt16 CardNo, UInt16 hcmp, UInt16 cmp_outbit);
        //配置读取二维比较PWM输出模式（适用于DMC5X10系列脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_set_pwmoutput(UInt16 CardNo, UInt16 hcmp, UInt16 pwm_enable, double duty, double freq, UInt16 pwm_number);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_get_pwmoutput(UInt16 CardNo, UInt16 hcmp, ref UInt16 pwm_enable, ref double duty, ref double freq, ref UInt16 pwm_number);
        
        //------------------------通用IO-----------------------
        //读取输入口的状态（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_inbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_inbit(UInt16 CardNo, UInt16 bitno);
        //设置输出口的状态（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_outbit(UInt16 CardNo, UInt16 bitno, UInt16 on_off);
        //读取输出口的状态（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_outbit(UInt16 CardNo, UInt16 bitno);
        //读取输入端口的值（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_inport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 dmc_read_inport(UInt16 CardNo, UInt16 portno);
        //读取输出端口的值（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 dmc_read_outport(UInt16 CardNo, UInt16 portno);
        //设置所有输出端口的值（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_outport(UInt16 CardNo, UInt16 portno, UInt32 outport_val);
        //设置通用输出端口的值（保留）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_outport_16X", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_outport_16X(UInt16 CardNo, UInt16 portno, UInt32 outport_val);
        //---------------------------通用IO带返回值检测----------------------
        //读取输入口的状态（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_inbit_ex(ushort CardNo, ushort bitno, ref ushort state);
        //读取输出口的状态（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_outbit_ex(ushort CardNo, ushort bitno, ref ushort state);
        //读取输入端口的值（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_inport_ex(ushort CardNo, ushort portno, ref UInt32 state);
        //读取输出端口的值（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_outport_ex(ushort CardNo, ushort portno, ref UInt32 state);
        
        //设置读取虚拟IO映射关系（适用于所有脉冲卡） 
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_io_map_virtual", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_io_map_virtual(UInt16 CardNo, UInt16 bitno, UInt16 MapIoType, UInt16 MapIoIndex, double Filter);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_io_map_virtual", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_io_map_virtual(UInt16 CardNo, UInt16 bitno, ref UInt16 MapIoType, ref UInt16 MapIoIndex, ref double Filter);
        //读取虚拟输入口的状态（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_inbit_virtual", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_inbit_virtual(UInt16 CardNo, UInt16 bitno);
        //IO延时翻转（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_reverse_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_reverse_outbit(UInt16 CardNo, UInt16 bitno, double reverse_time);
        //设置读取IO计数模式（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_io_count_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_io_count_mode(UInt16 CardNo, UInt16 bitno, UInt16 mode, double filter_time);        
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_io_count_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_io_count_mode(UInt16 CardNo, UInt16 bitno, ref UInt16 mode, ref double filter_time);
        //设置IO计数值（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_io_count_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_io_count_value(UInt16 CardNo, UInt16 bitno, UInt32 CountValue);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_io_count_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_io_count_value(UInt16 CardNo, UInt16 bitno, ref UInt32 CountValue);
                 
        //-----------------------专用IO 脉冲卡专用-------------------------
        //设置读取轴IO映射关系（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_axis_io_map", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_axis_io_map(UInt16 CardNo, UInt16 Axis, UInt16 IoType, UInt16 MapIoType, UInt16 MapIoIndex, double Filter);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_axis_io_map", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_axis_io_map(UInt16 CardNo, UInt16 Axis, UInt16 IoType, ref UInt16 MapIoType, ref UInt16 MapIoIndex, ref double Filter);
        //设置所有专用IO滤波时间（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_special_input_filter", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_special_input_filter(UInt16 CardNo, double Filter);
        // 回原点减速信号配置，(DMC3410专用)
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_sd_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_sd_mode(UInt16 CardNo, UInt16 axis, UInt16 sd_logic, UInt16 sd_mode);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_sd_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_sd_mode(UInt16 CardNo, UInt16 axis, ref UInt16 sd_logic, ref UInt16 sd_mode);
        //设置读取INP信号（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_inp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_inp_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 inp_logic);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_inp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_inp_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 inp_logic);
        //设置读取RDY信号（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_rdy_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 rdy_logic);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_rdy_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 rdy_logic);
        //设置读取ERC信号（保留）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_erc_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_erc_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 erc_logic, UInt16 erc_width, UInt16 erc_off_time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_erc_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_erc_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 erc_logic, ref UInt16 erc_width, ref UInt16 erc_off_time);
        //设置读取ALM信号（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_alm_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_alm_mode(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 alm_logic, UInt16 alm_action);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_alm_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_alm_mode(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 alm_logic, ref UInt16 alm_action);
        //设置读取EZ信号（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_ez_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_ez_mode(UInt16 CardNo, UInt16 axis, UInt16 ez_logic, UInt16 ez_mode, double filter);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_ez_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_ez_mode(UInt16 CardNo, UInt16 axis, ref UInt16 ez_logic, ref UInt16 ez_mode, ref double filter);
        //输出读取SEVON信号（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_sevon_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_sevon_pin(UInt16 CardNo, UInt16 axis, UInt16 on_off);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_sevon_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_sevon_pin(UInt16 CardNo, UInt16 axis);
        //控制ERC信号输出（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_erc_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_erc_pin(UInt16 CardNo, UInt16 axis, UInt16 sel);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_erc_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_erc_pin(UInt16 CardNo, UInt16 axis);
        //读取RDY状态（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_rdy_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_rdy_pin(UInt16 CardNo, UInt16 axis);
        //输出伺服复位信号（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_write_sevrst_pin(UInt16 CardNo, UInt16 axis, UInt16 on_off);
        //读伺服复位信号（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_sevrst_pin(UInt16 CardNo, UInt16 axis);

        //---------------------编码器 脉冲卡---------------------
        //设定读取编码器的计数方式（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_counter_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_counter_inmode(UInt16 CardNo, UInt16 axis, UInt16 mode);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_counter_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_counter_inmode(UInt16 CardNo, UInt16 axis, ref UInt16 mode);
        //编码器值（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 dmc_get_encoder(UInt16 CardNo, UInt16 axis);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_encoder(UInt16 CardNo, UInt16 axis, Int32 encoder_value);
        //编码器值(当量)（适用于DMC5000/5X10系列脉冲卡、所有总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_encoder_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_encoder_unit(UInt16 CardNo, UInt16 axis, double pos);     //当前反馈位置
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_encoder_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_encoder_unit(UInt16 CardNo, UInt16 axis, ref double pos);        
        //---------------------辅助编码器 总线卡---------------------
        //手轮编码器（备用，同dmc_set_extra_encoder）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_handwheel_encoder(ushort CardNo, ushort channel, int pos);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_handwheel_encoder(ushort CardNo, ushort channel, ref int pos);
        //设置辅助编码模式（适用于所有总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_extra_encoder_mode(ushort CardNo, ushort channel, ushort inmode, ushort multi);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_extra_encoder_mode(ushort CardNo, ushort channel, ref ushort inmode, ref ushort multi);
        //设置辅助编码器值（适用于所有总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_extra_encoder(ushort CardNo, ushort channel, int pos);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_extra_encoder(ushort CardNo, ushort channel, ref int pos);
        //---------------------位置计数控制---------------------
        //当前位置(当量)（适用于DMC5000/5X10系列脉冲卡、所有总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_position_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_position_unit(UInt16 CardNo, UInt16 axis, double pos);   //当前指令位置
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_position_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_position_unit(UInt16 CardNo, UInt16 axis, ref double pos);
        //当前位置(脉冲)（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 dmc_get_position(UInt16 CardNo, UInt16 axis);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_position(UInt16 CardNo, UInt16 axis, Int32 current_position);
        //--------------------运动状态----------------------	
        //读取指定轴的当前速度（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_current_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern double dmc_read_current_speed(UInt16 CardNo, UInt16 axis);
        //读取当前速度(当量)（适用于DMC5000/5X10系列脉冲卡、所有总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_current_speed_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_current_speed_unit(UInt16 CardNo, UInt16 Axis, ref double current_speed);   //轴当前运行速度
        //读取当前卡的插补速度（适用于DMC5X10系列脉冲卡、所有总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_vector_speed_unit(UInt16 CardNo, UInt16 Crd, ref double current_speed);	//读取当前卡的插补速度
        //读取指定轴的目标位置（适用于所有脉冲卡、R3032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_target_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 dmc_get_target_position(UInt16 CardNo, UInt16 axis);
        //读取指定轴的目标位置(当量)（适用于DMC5X10系列脉冲卡、所有EtherCAT总线系列卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_target_position_unit(UInt16 CardNo, UInt16 axis, ref double pos);
        //读取指定轴的运动状态（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_check_done", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_check_done(UInt16 CardNo, UInt16 axis);
        //读取指定轴的运动状态（适用于所有卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_check_done_ex(ushort CardNo, ushort axis, ref ushort state);
        //插补运动状态（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_check_done_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_check_done_multicoor(UInt16 CardNo, UInt16 crd);
        //读取指定轴有关运动信号的状态（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_axis_io_status", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 dmc_axis_io_status(UInt16 CardNo, UInt16 axis);
        //读取指定轴有关运动信号的状态（适用于所有卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_axis_io_status_ex(ushort CardNo, ushort axis, ref uint state);
        //单轴停止（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_stop(UInt16 CardNo, UInt16 axis, UInt16 stop_mode);
        //停止插补器（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_stop_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_stop_multicoor(UInt16 CardNo, UInt16 crd, UInt16 stop_mode);
        //紧急停止所有轴（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_emg_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_emg_stop(UInt16 CardNo);
        //脉冲卡指令 主卡与接线盒通讯状态（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_LinkState", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_LinkState(UInt16 CardNo, ref UInt16 State);
        //读取指定轴的运动模式（适用于DMC5000/5X10系列脉冲卡、所有总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_axis_run_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_axis_run_mode(UInt16 CardNo, UInt16 axis, ref UInt16 run_mode);  
        //读取轴停止原因（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_stop_reason", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_stop_reason(UInt16 CardNo, UInt16 axis, ref Int32 StopReason);    
        //清除轴停止原因（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_clear_stop_reason", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_clear_stop_reason(UInt16 CardNo, UInt16 axis);
        //trace功能（内部使用函数）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_trace", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_trace(UInt16 CardNo, UInt16 axis, UInt16 enable);   //trace功能
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_trace", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_trace(UInt16 CardNo, UInt16 axis, ref UInt16 enable);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_trace_data", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_trace_data(UInt16 CardNo, UInt16 axis, UInt16 data_option, ref Int32 ReceiveSize, double[] time, double[] data, ref Int32 remain_num);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_trace_start(ushort CardNo, ushort AxisNum, ushort[] AxisList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_trace_stop(ushort CardNo);

        //弧长计算（备用）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_calculate_arclength_center", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_calculate_arclength_center(double[] start_pos, double[] target_pos, double[] cen_pos, UInt16 arc_dir, double circle, ref double ArcLength);      //计算圆心圆弧弧长
        [DllImport("LTDMC.dll", EntryPoint = "dmc_calculate_arclength_3point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_calculate_arclength_3point(double[] start_pos, double[] mid_pos, double[] target_pos, double circle, ref double ArcLength);      //计算三点圆弧弧长
        [DllImport("LTDMC.dll", EntryPoint = "dmc_calculate_arclength_radius", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_calculate_arclength_radius(double[] start_pos, double[] target_pos, double arc_radius, UInt16 arc_dir, double circle, ref double ArcLength);     //计算半径圆弧弧长

        //--------------------CAN-IO扩展----------------------	
        //CAN-IO扩展，旧接口函数（保留）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_can_state", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_can_state(UInt16 CardNo, UInt16 NodeNum, UInt16 state, UInt16 Baud);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_can_state", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_can_state(UInt16 CardNo, ref UInt16 NodeNum, ref UInt16 state);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_can_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_can_outbit(UInt16 CardNo, UInt16 Node, UInt16 bitno, UInt16 on_off);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_can_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_can_outbit(UInt16 CardNo, UInt16 Node, UInt16 bitno);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_can_inbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_can_inbit(UInt16 CardNo, UInt16 Node, UInt16 bitno);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_can_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_can_outport(UInt16 CardNo, UInt16 Node, UInt16 PortNo, UInt32 outport_val);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_can_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 dmc_read_can_outport(UInt16 CardNo, UInt16 Node, UInt16 PortNo);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_can_inport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 dmc_read_can_inport(UInt16 CardNo, UInt16 Node, UInt16 PortNo);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_can_errcode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_can_errcode(UInt16 CardNo, ref UInt16 Errcode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_can_errcode_extern(UInt16 CardNo, ref UInt16 Errcode, ref UInt16 msg_losed, ref UInt16 emg_msg_num, ref UInt16 lostHeartB, ref UInt16 EmgMsg);
        //设置CAN io输出（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_outbit(ushort CardNo, ushort NodeID, ushort IoBit, ushort IoValue);
        //读取CAN io输出（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_outbit(ushort CardNo, ushort NodeID, ushort IoBit, ref ushort IoValue);
        //读取CAN io输入（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_inbit(ushort CardNo, ushort NodeID, ushort IoBit, ref ushort IoValue);
        //设置CAN io输出32位（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_outport(ushort CardNo, ushort NodeID, ushort PortNo, UInt32 IoValue);
        //读取CAN io输出32位（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_outport(ushort CardNo, ushort NodeID, ushort PortNo, ref UInt32 IoValue);
        //读取CAN io输入32位（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_inport(ushort CardNo, ushort NodeID, ushort PortNo, ref UInt32 IoValue);
        //---------------------------CAN IO带返回值检测----------------------
        //设置CAN io输出（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_outbit_ex(ushort CardNo, ushort NoteID, ushort IoBit, ushort IoValue, ref ushort state);
        //读取CAN io输出（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_outbit_ex(ushort CardNo, ushort NoteID, ushort IoBit, ref ushort IoValue, ref ushort state);
        //读取CAN io输入（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_inbit_ex(ushort CardNo, ushort NoteID, ushort IoBit, ref ushort IoValue, ref ushort state);
        //设置CAN io输出32位（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_outport_ex(ushort CardNo, ushort NoteID, ushort portno, UInt32 outport_val, ref ushort state);
        //读取CAN io输出32位（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_outport_ex(ushort CardNo, ushort NoteID, ushort portno, ref UInt32 outport_val, ref ushort state);
        //读取CAN io输入32位（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_inport_ex(ushort CardNo, ushort NoteID, ushort portno, ref UInt32 inport_val, ref ushort state);
        //---------------------------CAN ADDA----------------------
        //CAN ADDA指令 设置DA参数 （适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_da_output(ushort CardNo, ushort NoteID, ushort channel, double Value);
        //读取CAN DA参数（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_da_output(ushort CardNo, ushort NoteID, ushort channel, ref double Value);
        //读取CAN AD参数（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_ad_input(ushort CardNo, ushort NoteID, ushort channel, ref double Value);
        //配置CAN AD模式（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_ad_mode(ushort CardNo, ushort NoteID, ushort channel, ushort mode, uint buffer_nums);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_ad_mode(ushort CardNo, ushort NoteID, ushort channel, ref ushort mode, uint buffer_nums);
        //配置CAN DA模式（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_da_mode(ushort CardNo, ushort NoteID, ushort channel, ushort mode, uint buffer_nums);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_da_mode(ushort CardNo, ushort NoteID, ushort channel, ref ushort mode, uint buffer_nums);
        //CAN参数写入flash（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_to_flash(ushort CardNo, ushort PortNum, ushort NodeNum);
        //CAN总线链接（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_connect_state(UInt16 CardNo, UInt16 NodeNum, UInt16 state, UInt16 baud);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_connect_state(UInt16 CardNo, ref UInt16 NodeNum, ref UInt16 state);
        //---------------------------CAN ADDA带返回值检测----------------------
        //设置CAN DA参数（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_da_output_ex(ushort CardNo, ushort NoteID, ushort channel, double Value, ref ushort state);
        //读取CAN DA参数（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_da_output_ex(ushort CardNo, ushort NoteID, ushort channel, ref double Value, ref ushort state);
        //读取CAN AD参数（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_ad_input_ex(ushort CardNo, ushort NoteID, ushort channel, ref double Value, ref ushort state);
        //配置CAN AD模式（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_ad_mode_ex(ushort CardNo, ushort NoteID, ushort channel, ushort mode, UInt32 buffer_nums, ref ushort state);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_ad_mode_ex(ushort CardNo, ushort NoteID, ushort channel, ref ushort mode, UInt32 buffer_nums, ref ushort state);
        //配置CAN DA模式（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_da_mode_ex(ushort CardNo, ushort NoteID, ushort channel, ushort mode, UInt32 buffer_nums, ref ushort state);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_da_mode_ex(ushort CardNo, ushort NoteID, ushort channel, ref ushort mode, UInt32 buffer_nums, ref ushort state);
        //参数写入flash（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_to_flash_ex(ushort CardNo, ushort PortNum, ushort NodeNum, ref ushort state);

        //--------------------连续插补函数----------------------	
        //打开连续缓存区（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_open_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_open_list(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList);
        //关闭连续缓存区（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_close_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_close_list(UInt16 CardNo, UInt16 Crd);
        //复位连续缓存区（预留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_reset_list(UInt16 CardNo, UInt16 Crd);
        //连续插补中停止（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_stop_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_stop_list(UInt16 CardNo, UInt16 Crd, UInt16 stop_mode);
        //连续插补中暂停（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_pause_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_pause_list(UInt16 CardNo, UInt16 Crd);
        //开始连续插补（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_start_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_start_list(UInt16 CardNo, UInt16 Crd);
        //检测连续插补运动状态：0-运行，1-暂停，2-正常停止（DMC5X10不支持），3-未启动，4-空闲（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_run_state", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_run_state(UInt16 CardNo, UInt16 Crd);
        //检测连续插补运动状态：0-运行，1-停止（预留）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_check_done", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_check_done(UInt16 CardNo, UInt16 Crd);  
        //查连续插补剩余缓存数（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_remain_space", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 dmc_conti_remain_space(UInt16 CardNo, UInt16 Crd);
        //读取当前连续插补段的标号（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_read_current_mark", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 dmc_conti_read_current_mark(UInt16 CardNo, UInt16 Crd);
        //blend拐角过度模式（适用于DMC5000系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_blend", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_blend(UInt16 CardNo, UInt16 Crd, UInt16 enable);      
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_blend", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_blend(UInt16 CardNo, UInt16 Crd, ref UInt16 enable);
        //设置每段速度比例  缓冲区指令（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_override", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_override(UInt16 CardNo, UInt16 Crd, double Percent);      
        //设置插补中动态变速（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_change_speed_ratio", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_change_speed_ratio(UInt16 CardNo, UInt16 Crd, double Percent);
        //小线段前瞻（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_lookahead_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_lookahead_mode(UInt16 CardNo, UInt16 Crd, UInt16 enable, Int32 LookaheadSegments, double PathError, double LookaheadAcc);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_lookahead_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_lookahead_mode(UInt16 CardNo, UInt16 Crd, ref UInt16 enable, ref Int32 LookaheadSegments, ref double PathError, ref double LookaheadAcc);
        //--------------------连续插补IO功能----------------------
        //等待IO输入（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_wait_input", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_wait_input(UInt16 CardNo, UInt16 Crd, UInt16 bitno, UInt16 on_off, double TimeOut, Int32 mark);
        //相对于轨迹起点IO滞后输出（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_delay_outbit_to_start", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_delay_outbit_to_start(UInt16 CardNo, UInt16 Crd, UInt16 bitno, UInt16 on_off, double delay_value, UInt16 delay_mode, double ReverseTime);      
        //相对于轨迹终点IO滞后输出（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_delay_outbit_to_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_delay_outbit_to_stop(UInt16 CardNo, UInt16 Crd, UInt16 bitno, UInt16 on_off, double delay_time, double ReverseTime);      
        //相对于轨迹终点IO提前输出（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_ahead_outbit_to_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_ahead_outbit_to_stop(UInt16 CardNo, UInt16 Crd, UInt16 bitno, UInt16 on_off, double ahead_value, UInt16 ahead_mode, double ReverseTime);  
        //连续插补精确位置CMP输出（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_accurate_outbit_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_accurate_outbit_unit(UInt16 CardNo, UInt16 Crd, UInt16 cmp_no, UInt16 on_off, UInt16 map_axis, double abs_pos, UInt16 pos_source, double ReverseTime);    
        //连续插补立即IO输出（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_write_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_write_outbit(UInt16 CardNo, UInt16 Crd, UInt16 bitno, UInt16 on_off, double ReverseTime);     
        //清除段内未执行完的IO（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_clear_io_action", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_clear_io_action(UInt16 CardNo, UInt16 Crd, UInt32 IoMask);    
        //连续插补暂停及异常时IO输出状态（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_pause_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_pause_output(UInt16 CardNo, UInt16 Crd, UInt16 action, Int32 mask, Int32 state);     //暂停时IO输出 action 0, 不工作；1， 暂停时输出io_state; 2 暂停时输出io_state, 继续运行时首先恢复原来的io; 3,在2的基础上，停止时也生效。
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_pause_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_pause_output(UInt16 CardNo, UInt16 Crd, ref UInt16 action, ref Int32 mask, ref Int32 state);
        //延时指令（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_delay", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_delay(UInt16 CardNo, UInt16 Crd, double delay_time, Int32 mark);     //添加延时指令
        //IO输出延时翻转（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_reverse_outbit(UInt16 CardNo, UInt16 Crd, UInt16 bitno, double reverse_time);
        //IO延时输出（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_delay_outbit(UInt16 CardNo, UInt16 Crd, UInt16 bitno, UInt16 on_off, double delay_time);
        //连续插补单轴运动（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_pmove_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_pmove_unit(UInt16 CardNo, UInt16 Crd, UInt16 Axis, double dist, UInt16 posi_mode, UInt16 mode, Int32 mark); //连续插补中控制指定外轴运动
        //连续插补直线插补（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_line_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_line_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, UInt16 posi_mode, Int32 mark); //连续插补直线
        //连续插补圆心圆弧插补（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_arc_move_center_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_arc_move_center_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double[] Cen_Pos, UInt16 Arc_Dir, Int32 Circle, UInt16 posi_mode, Int32 mark);    
        //连续插补半径圆弧插补（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_arc_move_radius_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_arc_move_radius_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double Arc_Radius, UInt16 Arc_Dir, Int32 Circle, UInt16 posi_mode, Int32 mark);   
        //连续插补3点圆弧插补（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_arc_move_3points_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_arc_move_3points_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double[] Mid_Pos, Int32 Circle, UInt16 posi_mode, Int32 mark);     
        //连续插补矩形插补（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_rectangle_move_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_rectangle_move_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] TargetPos, double[] MaskPos, Int32 Count, UInt16 rect_mode, UInt16 posi_mode, Int32 mark);     
        //设置螺旋线插补运动模式（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_involute_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_involute_mode(UInt16 CardNo, UInt16 Crd, UInt16 mode);      //设置螺旋线是否封闭
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_involute_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_involute_mode(UInt16 CardNo, UInt16 Crd, ref UInt16 mode);   //读取螺旋线是否封闭设置
        //（备用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_line_unit_extern(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double[] Cen_Pos, UInt16 posi_mode, Int32 mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_arc_move_center_unit_extern(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Target_Pos, double[] Cen_Pos, double Arc_Radius, UInt16 posi_mode, Int32 mark);
        //设置读取龙门跟随模式（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_gear_follow_profile(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt16 master_axis, double ratio);//双Z轴
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_gear_follow_profile(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt16 master_axis, ref double ratio);
              
        //--------------------PWM控制----------------------
        //PWM控制（备用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_pwm_pin(UInt16 CardNo, UInt16 portno, UInt16 ON_OFF, double dfreqency, double dduty);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pwm_pin(UInt16 CardNo, UInt16 portno, ref UInt16 ON_OFF, ref double dfreqency, ref double dduty);
        //设置读取PWM使能（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_pwm_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_pwm_enable(UInt16 CardNo, UInt16 enable);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_pwm_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_pwm_enable(UInt16 CardNo, ref UInt16 enable);
        //设置读取PWM立即输出（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_pwm_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_pwm_output(UInt16 CardNo, UInt16 pwm_no, double fDuty, double fFre);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_pwm_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_pwm_output(UInt16 CardNo, UInt16 pwm_no, ref double fDuty, ref double fFre);        
        //连续插补PWM输出（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_pwm_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_pwm_output(UInt16 CardNo, UInt16 Crd, UInt16 pwm_no, double fDuty, double fFre);
        //高速PWM功能（备用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_pwm_enable_extern(UInt16 CardNo, UInt16 channel, UInt16 enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pwm_enable_extern(UInt16 CardNo, UInt16 channel, ref UInt16 enable);
        //设置PWM开关对应的占空比（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_pwm_onoff_duty", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_pwm_onoff_duty(UInt16 CardNo, UInt16 PwmNo, double fOnDuty, double fOffDuty);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_pwm_onoff_duty", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_pwm_onoff_duty(UInt16 CardNo, UInt16 PwmNo, ref double fOnDuty, ref double fOffDuty);
        //连续插补PWM速度跟随（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_pwm_follow_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_pwm_follow_speed(UInt16 CardNo, UInt16 Crd, UInt16 pwm_no, UInt16 mode, double MaxVel, double MaxValue, double OutValue);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_pwm_follow_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_pwm_follow_speed(UInt16 CardNo, UInt16 Crd, UInt16 pwm_no, ref UInt16 mode, ref double MaxVel, ref double MaxValue, ref double OutValue);
        //连续插补相对轨迹起点PWM滞后输出（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_delay_pwm_to_start", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_delay_pwm_to_start(UInt16 CardNo, UInt16 Crd, UInt16 pwmno, UInt16 on_off, double delay_value, UInt16 delay_mode, double ReverseTime);
        //连续插补相对轨迹终点PWM提前输出（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_ahead_pwm_to_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_ahead_pwm_to_stop(UInt16 CardNo, UInt16 Crd, UInt16 pwmno, UInt16 on_off, double ahead_value, UInt16 ahead_mode, double ReverseTime);
        //连续插补PWM立即输出（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_write_pwm", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_write_pwm(UInt16 CardNo, UInt16 Crd, UInt16 pwmno, UInt16 on_off, double ReverseTime);

        //--------------------ADDA输出----------------------
        //控制卡接线盒DA输出，设置DA输出使能（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_da_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_da_enable(UInt16 CardNo, UInt16 enable);      
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_da_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_da_enable(UInt16 CardNo, ref UInt16 enable);
        //设置DA输出（适用于所有脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_da_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_da_output(UInt16 CardNo, UInt16 channel, double Vout);   
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_da_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_da_output(UInt16 CardNo, UInt16 channel, ref double Vout);
        //控制卡接线盒AD输入，读取AD输入（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_ad_input(ushort CardNo, ushort channel, ref double Vout);
        //设置连续DA使能（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_set_da_output(UInt16 CardNo, UInt16 Crd, UInt16 channel, double Vout);
        //设置连续DA使能（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_da_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_da_enable(ushort CardNo, ushort Crd, ushort enable, ushort channel, int mark);
        //编码器da跟随（预留）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_encoder_da_follow_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_encoder_da_follow_enable(ushort CardNo, ushort axis, ushort enable);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_encoder_da_follow_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_encoder_da_follow_enable(ushort CardNo, ushort axis, ref ushort enable);
        //连续插补DA速度跟随（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_da_follow_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_da_follow_speed(ushort CardNo, ushort Crd, ushort da_no, double MaxVel, double MaxValue, double acc_offset, double dec_offset, double acc_dist, double dec_dist);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_da_follow_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_da_follow_speed(ushort CardNo, ushort Crd, ushort da_no, ref double MaxVel, ref double MaxValue, ref double acc_offset, ref double dec_offset, ref double acc_dist, ref double dec_dist);

        //小圆限速使能（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_arc_limit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_arc_limit(UInt16 CardNo, UInt16 Crd, UInt16 Enable, double MaxCenAcc, double MaxArcError);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_arc_limit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_arc_limit(UInt16 CardNo, UInt16 Crd, ref UInt16 Enable, ref double MaxCenAcc, ref double MaxArcError);
        //（预留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_IoFilter(UInt16 CardNo, UInt16 bitno, double filter);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_IoFilter(UInt16 CardNo, UInt16 bitno, ref double filter);
        //螺距补偿（旧指令，不使用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_lsc_index_value(UInt16 CardNo, UInt16 axis, UInt16 IndexID, Int32 IndexValue);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_lsc_index_value(UInt16 CardNo, UInt16 axis, UInt16 IndexID, ref Int32 IndexValue);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_lsc_config(UInt16 CardNo, UInt16 axis, UInt16 Origin, UInt32 Interal, UInt32 NegIndex, UInt32 PosIndex, double Ratio);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_lsc_config(UInt16 CardNo, UInt16 axis, ref UInt16 Origin, ref UInt32 Interal, ref UInt32 NegIndex, ref UInt32 PosIndex, ref double Ratio);
        //看门狗旧指令，不使用
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_watchdog(UInt16 CardNo, UInt16 enable, UInt32 time);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_call_watchdog(UInt16 CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_diagnoseData(UInt16 CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_set_cmd_end(UInt16 CardNo, UInt16 Crd, UInt16 enable);
        //区域软限位（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_zone_limit_config(UInt16 CardNo, UInt16[] axis, UInt16[] Source, Int32 x_pos_p, Int32 x_pos_n, Int32 y_pos_p, Int32 y_pos_n, UInt16 action_para);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_zone_limit_config(UInt16 CardNo, UInt16[] axis, UInt16[] Source, ref Int32 x_pos_p, ref Int32 x_pos_n, ref Int32 y_pos_p, ref Int32 y_pos_n, ref UInt16 action_para);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_zone_limit_enable(UInt16 CardNo, UInt16 enable);
        //轴互锁功能（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_interlock_config(UInt16 CardNo, UInt16[] axis, UInt16[] Source, Int32 delta_pos, UInt16 action_para);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_interlock_config(UInt16 CardNo, UInt16[] axis, UInt16[] Source, ref Int32 delta_pos, ref UInt16 action_para);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_interlock_enable(UInt16 CardNo, UInt16 enable);
        //龙门模式的误差保护（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_grant_error_protect(UInt16 CardNo, UInt16 axis, UInt16 enable, UInt32 dstp_error, UInt32 emg_error);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_grant_error_protect(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref UInt32 dstp_error, ref UInt32 emg_error);
        //龙门模式的误差保护当量函数（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_grant_error_protect_unit(UInt16 CardNo, UInt16 axis, UInt16 enable, double dstp_error, double emg_error);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_grant_error_protect_unit(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref double dstp_error, ref double emg_error);

        //物件分拣功能 （分拣固件专用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_camerablow_config(UInt16 CardNo, UInt16 camerablow_en, Int32 cameraPos, UInt16 piece_num, Int32 piece_distance, UInt16 axis_sel, Int32 latch_distance_min);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_camerablow_config(UInt16 CardNo, ref UInt16 camerablow_en, ref Int32 cameraPos, ref UInt16 piece_num, ref Int32 piece_distance, ref UInt16 axis_sel, ref Int32 latch_distance_min);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_clear_camerablow_errorcode(UInt16 CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_camerablow_errorcode(UInt16 CardNo, ref UInt16 errorcode);
        //配置通用输入（0~15）做为轴的限位信号（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_io_limit_config(UInt16 CardNo, UInt16 portno, UInt16 enable, UInt16 axis_sel, UInt16 el_mode, UInt16 el_logic);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_io_limit_config(UInt16 CardNo, UInt16 portno, ref UInt16 enable, ref UInt16 axis_sel, ref UInt16 el_mode, ref UInt16 el_logic);
        //手轮滤波参数（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_handwheel_filter(UInt16 CardNo, UInt16 axis, double filter_factor);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_handwheel_filter(UInt16 CardNo, UInt16 axis, ref double filter_factor);
        //读取坐标系各轴的当前规划坐标（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_get_interp_map(UInt16 CardNo, UInt16 Crd, ref UInt16 AxisNum, UInt16[] AxisList, double[] pPosList);
        //坐标系错误代码 （保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_get_crd_errcode(UInt16 CardNo, UInt16 Crd, ref UInt16 errcode);
        //保留
        [DllImport("LTDMC.dll")]
        public static extern short dmc_line_unit_follow(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] Dist, UInt16 posi_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_line_unit_follow(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] pPosList, UInt16 posi_mode, Int32 mark);
        //连续插补缓冲区DA操作（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_set_da_action(UInt16 CardNo, UInt16 Crd, UInt16 mode, UInt16 portno, double dvalue);
        //读编码器速度（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_encoder_speed(UInt16 CardNo, UInt16 Axis, ref double current_speed);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_axis_follow_line_enable(UInt16 CardNo, UInt16 Crd, UInt16 enable_flag);
        //插补轴脉冲补偿（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_interp_compensation(UInt16 CardNo, UInt16 axis, double dvalue, double time);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_interp_compensation(UInt16 CardNo, UInt16 axis, ref double dvalue, ref double time);
        //读取相对于起点的距离（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_distance_to_start(UInt16 CardNo, UInt16 Crd, ref double distance_x, ref double distance_y, Int32 imark);
        //设置标志位 表示是否开始计算相对起点（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_start_distance_flag(UInt16 CardNo, UInt16 Crd, UInt16 flag);

        //刀向跟随（适用于DMC5000/5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_gear_unit(UInt16 CardNo, UInt16 Crd, UInt16 axis, double dist, UInt16 follow_mode, Int32 imark);
        //轨迹拟合使能设置（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_path_fitting_enable(UInt16 CardNo, UInt16 Crd, UInt16 enable);
        //--------------------螺距补偿----------------------
        //螺距补偿功能(新)（适用于所有脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_enable_leadscrew_comp(UInt16 CardNo, UInt16 axis, UInt16 enable);
        //配置逻辑补偿参数（脉冲）（适用于所有脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_leadscrew_comp_config(UInt16 CardNo, UInt16 axis, UInt16 n, Int32 startpos, Int32 lenpos, Int32[] pCompPos, Int32[] pCompNeg);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_leadscrew_comp_config(UInt16 CardNo, UInt16 axis, ref UInt16 n, ref int startpos, ref int lenpos, int[] pCompPos, int[] pCompNeg);
        //配置逻辑补偿参数（当量）（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_leadscrew_comp_config_unit(UInt16 CardNo, UInt16 axis, UInt16 n, double startpos, double lenpos, double[] pCompPos, double[] pCompNeg);       
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_leadscrew_comp_config_unit(UInt16 CardNo, UInt16 axis, ref UInt16 n, ref double startpos, ref double lenpos, double[] pCompPos, double[] pCompNeg);
        //螺距补偿前的脉冲位置，编码器位置//20191025（适用于DMC3000系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_position_ex(UInt16 CardNo, UInt16 axis, ref double pos);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_encoder_ex(UInt16 CardNo, UInt16 axis, ref double pos);
        //螺距补偿前的脉冲位置，编码器位置 当量（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_position_ex_unit(UInt16 CardNo, UInt16 axis, ref double pos);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_encoder_ex_unit(UInt16 CardNo, UInt16 axis, ref double pos);

        //指定轴做定长位移运动 按固定曲线运动（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_t_pmove_extern(UInt16 CardNo, UInt16 axis, double MidPos, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc, double dec, UInt16 posi_mode);
        //
        [DllImport("LTDMC.dll")]
        public static extern short dmc_t_pmove_extern_unit(UInt16 CardNo, UInt16 axis, double MidPos, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc, double dec, UInt16 posi_mode);
        //设置脉冲计数值和编码器反馈值之间差值的报警阀值（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_pulse_encoder_count_error(UInt16 CardNo, UInt16 axis, UInt16 error);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pulse_encoder_count_error(UInt16 CardNo, UInt16 axis, ref UInt16 error);
        //检查脉冲计数值和编码器反馈值之间差值是否超过报警阀值（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_check_pulse_encoder_count_error(UInt16 CardNo, UInt16 axis, ref Int32 pulse_position, ref Int32 enc_position);
        //设置/回读脉冲计数值和编码器反馈值之间差值的报警阈值unit（适用于DMC5X10脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_pulse_encoder_count_error_unit(ushort CardNo, ushort axis, double error);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pulse_encoder_count_error_unit(ushort CardNo, ushort axis, ref double error);
        //检查脉冲计数值和编码器反馈值之间差值是否超过报警阈值unit（适用于DMC5X10脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_check_pulse_encoder_count_error_unit(ushort CardNo, ushort axis, ref double pulse_position, ref double enc_position);
        //使能和设置跟踪编码器误差不在范围内时轴的停止模式（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_encoder_count_error_action_config(UInt16 CardNo, UInt16 enable, UInt16 stopmode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_encoder_count_error_action_config(UInt16 CardNo, ref UInt16 enable, ref UInt16 stopmode);
        
        //新物件分拣功能 分拣固件专用
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_close(UInt16 CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_start(UInt16 CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_init_config(UInt16 CardNo, UInt16 cameraCount, Int32[] pCameraPos, UInt16[] pCamIONo, UInt32 cameraTime, UInt16 cameraTrigLevel, UInt16 blowCount, Int32[] pBlowPos, UInt16[] pBlowIONo, UInt32 blowTime, UInt16 blowTrigLevel, UInt16 axis, UInt16 dir, UInt16 checkMode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_camera_trig_count(UInt16 CardNo, UInt16 cameraNum, UInt32 cameraTrigCnt);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_camera_trig_count(UInt16 CardNo, UInt16 cameraNum, ref UInt32 pCameraTrigCnt, UInt16 count);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_blow_trig_count(UInt16 CardNo, UInt16 blowNum, UInt32 blowTrigCnt);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_blow_trig_count(UInt16 CardNo, UInt16 blowNum, ref UInt32 pBlowTrigCnt, UInt16 count);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_camera_config(UInt16 CardNo, UInt16 index, ref Int32 pos, ref UInt32 trigTime, ref UInt16 ioNo, ref UInt16 trigLevel);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_blow_config(UInt16 CardNo, UInt16 index, ref Int32 pos, ref UInt32 trigTime, ref UInt16 ioNo, ref UInt16 trigLevel);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_blow_status(UInt16 CardNo, ref Int32 trigCntAll, ref UInt16 trigMore, ref UInt16 trigLess);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_trig_blow(UInt16 CardNo, UInt16 blowNum);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_blow_enable(UInt16 CardNo, UInt16 blowNum, UInt16 enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_piece_config(UInt16 CardNo, UInt32 maxWidth, UInt32 minWidth, UInt32 minDistance, UInt32 minTimeIntervel);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_piece_status(UInt16 CardNo, ref UInt32 pieceFind, ref UInt32 piecePassCam, ref UInt32 dist2next, ref UInt32 pieceWidth);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_cam_trig_phase(UInt16 CardNo, UInt16 blowNo, double coef);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_blow_trig_phase(UInt16 CardNo, UInt16 blowNo, double coef);
        
        //内部使用（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_sevon_enable(UInt16 CardNo, UInt16 axis, UInt16 on_off);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_sevon_enable(UInt16 CardNo, UInt16 axis);

        //连续编码器da跟随（适用于DMC5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_set_encoder_da_follow_enable(UInt16 CardNo, UInt16 Crd, UInt16 axis, UInt16 enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_get_encoder_da_follow_enable(UInt16 CardNo, UInt16 Crd, ref UInt16 axis, ref UInt16 enable);
        //设置位置误差带（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_factor_error", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_factor_error(UInt16 CardNo, UInt16 axis, double factor, Int32 error);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_factor_error", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_factor_error(UInt16 CardNo, UInt16 axis, ref double factor, ref Int32 error);
        //设置/回读位置误差带unit（适用于DMC5X10脉冲卡、EtherCAT总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_factor_error_unit(ushort CardNo, ushort axis, double factor, double error);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_factor_error_unit(ushort CardNo, ushort axis, ref double factor, ref double error);
        //保留
        [DllImport("LTDMC.dll")]
        public static extern short dmc_check_done_pos(UInt16 CardNo, UInt16 axis, UInt16 posi_mode);
        //保留
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_factor(UInt16 CardNo, UInt16 axis, double factor);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_error(UInt16 CardNo, UInt16 axis, Int32 error);
        //检测指令到位（适用于所有脉冲卡、总线卡）
        [DllImport("LTDMC.dll", EntryPoint = "dmc_check_success_pulse", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_check_success_pulse(UInt16 CardNo, UInt16 axis);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_check_success_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_check_success_encoder(UInt16 CardNo, UInt16 axis);

        //IO及编码器计数功能（保留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_io_count_profile(UInt16 CardNo, UInt16 chan, UInt16 bitno,UInt16 mode,double filter, double count_value, UInt16[] axis_list, UInt16 axis_num, UInt16 stop_mode );
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_io_count_profile(UInt16 CardNo, UInt16 chan, ref UInt16 bitno,ref UInt16 mode,ref double filter, ref double count_value, UInt16[] axis_list, ref UInt16 axis_num, ref UInt16 stop_mode );
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_io_count_enable(UInt16 CardNo, UInt16 chan, UInt16 ifenable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_clear_io_count(UInt16 CardNo, UInt16 chan);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_io_count_value_extern(UInt16 CardNo, UInt16 chan, ref Int32 current_value);
        //保留
        [DllImport("LTDMC.dll")]
        public static extern short dmc_change_speed_extend(UInt16 CardNo,UInt16 axis,double Curr_Vel, double Taccdec, UInt16 pin_num, UInt16 trig_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_follow_vector_speed_move(UInt16 CardNo,UInt16 axis,UInt16 Follow_AxisNum,UInt16[] Follow_AxisList,double ratio);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_line_unit_extend(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] pPosList, UInt16 posi_mode, double Extend_Len, UInt16 enable,Int32 mark); //连续插补直线
     
        //总线参数
        [DllImport("LTDMC.dll", EntryPoint = "nmc_download_configfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_download_configfile(UInt16 CardNo, UInt16 PortNum, String FileName);//总线ENI配置文件
        [DllImport("LTDMC.dll", EntryPoint = "nmc_download_mapfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_download_mapfile(UInt16 CardNo, String FileName);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_upload_configfile(UInt16 CardNo, UInt16 PortNum, String FileName);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_set_manager_para", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_set_manager_para(UInt16 CardNo, UInt16 PortNum, Int32 baudrate, UInt16 ManagerID);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_get_manager_para", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_get_manager_para(UInt16 CardNo, UInt16 PortNum, ref UInt32 baudrate, ref UInt16 ManagerID);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_set_manager_od", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_set_manager_od(UInt16 CardNo, UInt16 PortNum, UInt16 index, UInt16 subindex, UInt16 valuelength, UInt32 value);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_get_manager_od", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_get_manager_od(UInt16 CardNo, UInt16 PortNum, UInt16 index, UInt16 subindex, UInt16 valuelength, ref UInt32 value);

        [DllImport("LTDMC.dll", EntryPoint = "nmc_get_total_axes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_get_total_axes(ushort CardNo, ref uint TotalAxis);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_get_total_ionum", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_get_total_ionum(UInt16 CardNo, ref UInt16 TotalIn, ref UInt16 TotalOut);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_get_LostHeartbeat_Nodes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_get_LostHeartbeat_Nodes(UInt16 CardNo, UInt16 PortNum, UInt16[] NodeID, ref UInt16 NodeNum);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_get_EmergeneyMessege_Nodes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_get_EmergeneyMessege_Nodes(UInt16 CardNo, UInt16 PortNum, UInt32[] NodeMsg, ref UInt16 MsgNum);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_SendNmtCommand", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_SendNmtCommand(UInt16 CardNo, UInt16 PortNum, UInt16 NodeID, UInt16 NmtCommand);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_syn_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_syn_move(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, Int32[] Position, UInt16[] PosiMode);
        //
        [DllImport("LTDMC.dll")]
        public static extern short nmc_syn_move_unit(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, double[] Position, UInt16[] PosiMode);
        //总线多轴同步运动
        [DllImport("LTDMC.dll")]
        public static extern short nmc_sync_pmove_unit(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, double[] Dist, UInt16[] PosiMode);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_sync_vmove_unit(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, UInt16[] Dir);
        //设置主站参数
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_master_para(UInt16 CardNo, UInt16 PortNum, UInt16 Baudrate, UInt32 NodeCnt, UInt16 MasterId);
        //读取主站参数
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_master_para(UInt16 CardNo, UInt16 PortNum, ref UInt16 Baudrate, ref UInt32 NodeCnt, ref UInt16 MasterId);
        //获取总线ADDA输入输出口数
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_total_adcnum(ushort CardNo, ref ushort TotalIn, ref ushort TotalOut);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_controller_workmode(ushort CardNo, ushort controller_mode);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_controller_workmode(ushort CardNo, ref ushort controller_mode);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_cycletime(ushort CardNo, ushort FieldbusType, int CycleTime);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_cycletime(ushort CardNo, ushort FieldbusType, ref int CycleTime);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_node_od(ushort CardNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, ref int value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_node_od(ushort CardNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, int value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_reset_to_factory(ushort CardNo, ushort PortNum, ushort NodeNum);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_alarm_clear(ushort CardNo, ushort PortNum, ushort nodenum);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_slave_nodes(ushort CardNo, ushort PortNum, ushort BaudRate, ref ushort NodeId, ref ushort NodeNum);
        
        //轴状态机
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_state_machine(ushort CardNo, ushort axis, ref ushort Axis_StateMachine);
        //获取轴状态字
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_statusword(ushort CardNo, ushort axis, ref int statusword);
        //获取轴配置控制模式，返回值（6回零模式，8csp模式）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_setting_contrlmode(ushort CardNo, ushort axis, ref int contrlmode);
        //设置总线轴控制字
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_axis_contrlword(ushort CardNo, ushort axis, int contrlword);
        //获取总线轴控制字
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_contrlword(ushort CardNo, ushort axis, ref int contrlword);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_type(ushort CardNo, ushort axis, ref ushort Axis_Type);
        //获取总线时间量，平均时间，最大时间，执行周期数
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_consume_time_fieldbus(ushort CardNo, ushort Fieldbustype, ref uint Average_time, ref uint Max_time, ref UInt64 Cycles);
        //清除时间量
        [DllImport("LTDMC.dll")]
        public static extern short nmc_clear_consume_time_fieldbus(ushort CardNo, ushort Fieldbustype);
        //总线单轴使能函数 255表示全使能
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_axis_enable(ushort CardNo, ushort axis);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_axis_disable(ushort CardNo, ushort axis);
        // 获取轴的从站信息
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_node_address(ushort CardNo, ushort axis, ref ushort SlaveAddr, ref ushort Sub_SlaveAddr);
        //获取总线轴数
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_total_slaves(ushort CardNo, ushort PortNum, ref ushort TotalSlaves);
        [DllImport("LTDMC.dll")]
        //总线回原点函数
        public static extern short nmc_set_home_profile(ushort CardNo, ushort axis, ushort home_mode, double Low_Vel, double High_Vel, double Tacc, double Tdec, double offsetpos);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_home_profile(ushort CardNo, ushort axis, ref ushort home_mode, ref double Low_Vel, ref double High_Vel, ref double Tacc, ref double Tdec, ref double offsetpos);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_home_move(ushort CardNo, ushort axis);
        //
        [DllImport("LTDMC.dll")]
        public static extern short nmc_start_scan_ethercat(ushort CardNo, ushort AddressID);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_stop_scan_ethercat(ushort CardNo, ushort AddressID);
        //设置轴的运行模式 1为pp模式，6为回零模式，8为csp模式
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_axis_run_mode(ushort CardNo, ushort axis, ushort run_mode);
        //清除端口报警
        [DllImport("LTDMC.dll")]
        public static extern short nmc_clear_alarm_fieldbus(ushort CardNo, ushort PortNum);
        //停止ethercat总线,返回0表示成功，其他参数表示不成功
        [DllImport("LTDMC.dll")]
        public static extern short nmc_stop_etc(ushort CardNo, ref ushort ETCState);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_contrlmode(ushort CardNo, ushort Axis, ref int Contrlmode);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_io_in(ushort CardNo, ushort axis);

        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_axis_io_out(UInt16 CardNo, UInt16 axis, UInt32 iostate);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_io_out(UInt16 CardNo, UInt16 axis);
        // 获取总线端口错误码
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_errcode(ushort CardNo, ushort channel, ref ushort errcode);
        // 清除总线端口错误码
        [DllImport("LTDMC.dll")]
        public static extern short nmc_clear_errcode(ushort CardNo, ushort channel);
        // 获取总线轴错误码
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_errcode(ushort CardNo, ushort axis, ref ushort Errcode);
        // 清除总线轴错误码
        [DllImport("LTDMC.dll")]
        public static extern short nmc_clear_axis_errcode(ushort CardNo, ushort axis);

        //RTEX卡添加函数
        [DllImport("LTDMC.dll")]
        public static extern short nmc_start_connect(UInt16 CardNo, UInt16 chan, ref UInt16 info, ref UInt16 len);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_vendor_info(UInt16 CardNo, UInt16 axis, Byte[] info, ref UInt16 len);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_slave_type_info(UInt16 CardNo, UInt16 axis, Byte[] info, ref UInt16 len);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_slave_name_info(UInt16 CardNo, UInt16 axis, Byte[] info, ref UInt16 len);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_slave_version_info(UInt16 CardNo, UInt16 axis, Byte[] info, ref UInt16 len);

        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_parameter(UInt16 CardNo, UInt16 axis, UInt16 index, UInt16 subindex, UInt32 para_data);
        /**************************************************************
        *功能说明：RTEX驱动器写EEPROM操作
        **************************************************************/
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_slave_eeprom(UInt16 CardNo, UInt16 axis);

        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_parameter(UInt16 CardNo, UInt16 axis, UInt16 index, UInt16 subindex, ref UInt32 para_data);
        /**************************************************************
         * *index:rtex驱动器的参数分类
         * *subindex:rtex驱动器在index类别下的参数编号
         * *para_data:读出的参数值
         * **************************************************************/
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_parameter_attributes(UInt16 CardNo, UInt16 axis, UInt16 index, UInt16 subindex, ref UInt32 para_data);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_cmdcycletime(UInt16 CardNo, UInt16 PortNum, UInt32 cmdtime);
        //设置RTEX总线周期比(us)
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_cmdcycletime(UInt16 CardNo, UInt16 PortNum, ref UInt32 cmdtime);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_config_atuo_log(UInt16 CardNo, UInt16 ifenable, UInt16 dir, UInt16 byte_index, UInt16 mask, UInt16 condition, UInt32 counter);

        //扩展PDO
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_rxpdo_extra(UInt16 CardNo, UInt16 PortNum, UInt16 address, UInt16 DataLen, Int32 Value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_rxpdo_extra(UInt16 CardNo, UInt16 PortNum, UInt16 address, UInt16 DataLen, ref Int32 Value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_txpdo_extra(UInt16 CardNo, UInt16 PortNum, UInt16 address, UInt16 DataLen, ref Int32 Value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_rxpdo_extra_uint(UInt16 CardNo, UInt16 PortNum, UInt16 address, UInt16 DataLen, UInt32 Value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_rxpdo_extra_uint(UInt16 CardNo, UInt16 PortNum, UInt16 address, UInt16 DataLen, ref UInt32 Value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_txpdo_extra_uint(UInt16 CardNo, UInt16 PortNum, UInt16 address, UInt16 DataLen, ref UInt32 Value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_log_state(UInt16 CardNo, UInt16 chan, ref UInt32 state);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_driver_reset(UInt16 CardNo, UInt16 axis);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_offset_pos(UInt16 CardNo, UInt16 axis, double offset_pos);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_offset_pos(UInt16 CardNo, UInt16 axis, ref double offset_pos);
        //清除rtex绝对值编码器的多圈值
        [DllImport("LTDMC.dll")]
        public static extern short nmc_clear_abs_driver_multi_cycle(UInt16 CardNo, UInt16 axis);
        //---------------------------EtherCAT IO扩展模块操作指令----------------------
        //设置io输出32位总线扩展
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_outport_extern(UInt16 CardNo, UInt16 Channel, UInt16 NoteID, UInt16 portno, UInt32 outport_val);
        //读取io输出32位总线扩展
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_outport_extern(UInt16 CardNo, UInt16 Channel, UInt16 NoteID, UInt16 portno, ref UInt32 outport_val);
        //读取io输入32位总线扩展
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_inport_extern(UInt16 CardNo, UInt16 Channel, UInt16 NoteID, UInt16 portno, ref UInt32 inport_val);
        //设置io输出
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_outbit_extern(UInt16 CardNo, UInt16 Channel, UInt16 NoteID, UInt16 IoBit, UInt16 IoValue);
        //读取io输出
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_outbit_extern(UInt16 CardNo, UInt16 Channel, UInt16 NoteID, UInt16 IoBit, ref UInt16 IoValue);
        //读取io输入
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_inbit_extern(UInt16 CardNo, UInt16 Channel, UInt16 NoteID, UInt16 IoBit, ref UInt16 IoValue);
        
        //返回最近错误码
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_current_fieldbus_state_info(UInt16 CardNo, UInt16 Channel, ref UInt16 Axis, ref UInt16 ErrorType, ref UInt16 SlaveAddr, ref UInt32 ErrorFieldbusCode);
        // 返回历史错误码
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_detail_fieldbus_state_info(UInt16 CardNo, UInt16 Channel, UInt32 ReadErrorNum, ref UInt32 TotalNum, ref UInt32 ActualNum, UInt16[] Axis, UInt16[] ErrorType, UInt16[] SlaveAddr, UInt32[] ErrorFieldbusCode);
        //启动采集
        [DllImport("LTDMC.dll")]
        public static extern short nmc_start_pdo_trace(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, UInt16 Index_Num, UInt32 Trace_Len, UInt16[] Index, UInt16[] Sub_Index);
        //获取采集参数
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_pdo_trace(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, ref UInt16 Index_Num, ref UInt32 Trace_Len, UInt16[] Index, UInt16[] Sub_Index);
        //设置触发采集参数
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_pdo_trace_trig_para(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, UInt16 Trig_Index, UInt16 Trig_Sub_Index, int Trig_Value, UInt16 Trig_Mode);
        //获取触发采集参数
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_pdo_trace_trig_para(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, ref UInt16 Trig_Index, ref UInt16 Trig_Sub_Index, ref int Trig_Value, ref UInt16 Trig_Mode);
        //采集清除
        [DllImport("LTDMC.dll")]
        public static extern short nmc_clear_pdo_trace_data(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr);
        //采集停止
        [DllImport("LTDMC.dll")]
        public static extern short nmc_stop_pdo_trace(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr);
        //采集数据读取
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_pdo_trace_data(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, UInt32 StartAddr, UInt32 Readlen, ref UInt32 ActReadlen, Byte[] Data);
        //已采集个数
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_pdo_trace_num(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, ref UInt32 Data_num, ref UInt32 Size_of_each_bag);
        //采集状态
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_pdo_trace_state(UInt16 CardNo, UInt16 Channel, UInt16 SlaveAddr, ref UInt16 Trace_state);
        //总线专用
        [DllImport("LTDMC.dll")]
        public static extern short nmc_reset_canopen(UInt16 CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_reset_rtex(UInt16 CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_reset_etc(UInt16 CardNo);
        //总线错误处理配置
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_fieldbus_error_switch(UInt16 CardNo, UInt16 channel, UInt16 data);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_fieldbus_error_switch(UInt16 CardNo, UInt16 channel, ref UInt16 data);

        ////配置CST切换到CSP后，由于驱动器不能及时同步主站目标位置，延时时间内主站继续同步驱动器实际位置，已取消该功能
        //[DllImport("LTDMC.dll")]
        //public static extern short nmc_torque_set_delay_cycle(ushort CardNo, ushort axis, int delay_cycle);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_torque_move(UInt16 CardNo, UInt16 axis, int Torque, UInt16 PosLimitValid, double PosLimitValue, UInt16 PosMode);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_change_torque(UInt16 CardNo, UInt16 axis, int Torque);
        //读取转矩大小
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_torque(UInt16 CardNo, UInt16 axis, ref int Torque);
        //modbus函数
        [DllImport("LTDMC.dll")]
        public static extern short dmc_modbus_active_COM1(UInt16 id, string COMID,int speed, int bits, int check, int stop);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_modbus_active_COM2(UInt16 id, string COMID, int speed, int bits, int check, int stop);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_modbus_active_ETH(UInt16 id, UInt16 port);

        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_modbus_0x(UInt16 CardNo, UInt16 start, UInt16 inum, byte[] pdata);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_modbus_0x(UInt16 CardNo, UInt16 start, UInt16 inum, byte[] pdata);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_modbus_4x(UInt16 CardNo, UInt16 start, UInt16 inum, UInt16[] pdata);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_modbus_4x(UInt16 CardNo, UInt16 start, UInt16 inum, UInt16[] pdata);

        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_modbus_4x_float(UInt16 CardNo, UInt16 start, UInt16 inum, float[] pdata);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_modbus_4x_float(UInt16 CardNo, UInt16 start, UInt16 inum, float[] pdata);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_modbus_4x_int(UInt16 CardNo, UInt16 start, UInt16 inum, int[] pdata);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_modbus_4x_int(UInt16 CardNo, UInt16 start, UInt16 inum, int[] pdata);
        //保留
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_line_io_union(UInt16 CardNo,UInt16 Crd,UInt16 AxisNum,UInt16[] AxisList,double[] pPosList,UInt16 posi_mode,UInt16 bitno,UInt16 on_off,double io_value,UInt16 io_mode,UInt16 MapAxis,UInt16 pos_source,double ReverseTime,long mark);
        //设置编码器方向（适用于DMC3000系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_encoder_dir(UInt16 CardNo, UInt16 axis,UInt16 dir);
        
        //圆弧区域软限位（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_arc_zone_limit_config(UInt16 CardNo, UInt16[] AxisList, UInt16 AxisNum, double[] Center, double Radius, UInt16 Source,UInt16 StopMode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_arc_zone_limit_config(UInt16 CardNo, UInt16[] AxisList, ref UInt16 AxisNum, double[] Center, ref double Radius, ref UInt16 Source,ref UInt16 StopMode);
        //圆形区域软限位unit（适用于DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_arc_zone_limit_config_unit(ushort CardNo, ushort[] AxisList, ushort AxisNum, double[] Center, double Radius, ushort Source, ushort StopMode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_arc_zone_limit_config_unit(ushort CardNo, ushort[] AxisList, ref ushort AxisNum, double[] Center, ref double Radius, ref ushort Source, ref ushort StopMode);
        //查询相应轴的状态（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_arc_zone_limit_axis_status(UInt16 CardNo, UInt16 axis);
        //圆形限位使能（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_arc_zone_limit_enable(UInt16 CardNo, UInt16 enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_arc_zone_limit_enable(UInt16 CardNo, ref UInt16 enable);
        
        //控制卡接线盒断线后是否初始化输出电平
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_output_status_repower(UInt16 CardNo, UInt16 enable);
        //旧接口（软启动），不使用
        [DllImport("LTDMC.dll")]
        public static extern short dmc_t_pmove_extern_softlanding(UInt16 CardNo, UInt16 axis, double MidPos, double TargetPos, double start_Vel, double Max_Vel, double stop_Vel, UInt32 delay_ms, double Max_Vel2, double stop_vel2, double acc_time, double dec_time, UInt16 posi_mode);
        //保留
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_XD(UInt16 CardNo, UInt16 cmp, long pos, UInt16 dir, UInt16 action, UInt32 actpara, long startPos);//硒电定制比较函数
        
        //---------------------------ORG输入触发在线变速变位----------------------
        //配置ORG输入触发在线变速变位（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pmove_change_pos_speed_config(UInt16 CardNo, UInt16 axis, double tar_vel, double tar_rel_pos, UInt16 trig_mode, UInt16 source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pmove_change_pos_speed_config(UInt16 CardNo, UInt16 axis, ref double tar_vel, ref double tar_rel_pos, ref UInt16 trig_mode, ref UInt16 source);
        //ORG输入触发在线变速变位使能（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pmove_change_pos_speed_enable(UInt16 CardNo, UInt16 axis, UInt16 enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pmove_change_pos_speed_enable(UInt16 CardNo, UInt16 axis, ref UInt16 enable);
        //读取ORG输入触发在线变速变位的状态  trig_num 触发次数，trig_pos 触发位置（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pmove_change_pos_speed_state(ushort CardNo, ushort axis, ref ushort trig_num, double[] trig_pos);
        //IO变速变位，配置io输入口（适用于EtherCAT总线系列卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pmove_change_pos_speed_inbit(ushort CardNo, ushort axis, ushort inbit, ushort enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pmove_change_pos_speed_inbit(ushort CardNo, ushort axis, ref ushort inbit, ref ushort enable);
        //保留
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_extend(UInt16 CardNo, UInt16 axis, long pos, UInt16 dir, UInt16 action, UInt16 para_num, ref UInt32 actpara, UInt32 compare_time);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_cmd_position(UInt16 CardNo, UInt16 axis, ref double pos);
        //逻辑采样配置（内部使用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_logic_analyzer_config(UInt16 CardNo, UInt16 channel, UInt32 SampleFre, UInt32 SampleDepth, UInt16 SampleMode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_start_logic_analyzer(UInt16 CardNo, UInt16 channel, UInt16 enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_logic_analyzer_counter(UInt16 CardNo, UInt16 channel, ref UInt32 counter);
        //20190923修改kg定制函数接口（客户定制）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_inbit_append(UInt16 CardNo, UInt16 bitno);//读取输入口的状态
        [DllImport("LTDMC.dll")]
        public static extern short dmc_write_outbit_append(UInt16 CardNo, UInt16 bitno, UInt16 on_off);//设置输出口的状态
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_outbit_append(UInt16 CardNo, UInt16 bitno);//读取输出口的状态
        [DllImport("LTDMC.dll")]
        public static extern UInt32 dmc_read_inport_append(UInt16 CardNo, UInt16 portno);//读取输入端口的值
        [DllImport("LTDMC.dll")]
        public static extern UInt32 dmc_read_outport_append(UInt16 CardNo, UInt16 portno);//读取输出端口的值
        [DllImport("LTDMC.dll")]
        public static extern short dmc_write_outport_append(UInt16 CardNo, UInt16 portno, UInt32 port_value);//设置所有输出端口的值

        //---------------------------椭圆插补及切向跟随----------------------
        // 设置坐标系切向跟随（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_tangent_follow(UInt16 CardNo, UInt16 Crd, UInt16 axis, UInt16 follow_curve, UInt16 rotate_dir, double degree_equivalent);
        // 获取指定坐标系切向跟随参数（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_tangent_follow_param(UInt16 CardNo, UInt16 Crd, ref UInt16 axis, ref UInt16 follow_curve, ref UInt16 rotate_dir, ref double degree_equivalent);
        // 取消坐标系跟随（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_disable_follow_move(UInt16 CardNo, UInt16 Crd);
        // 椭圆插补（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ellipse_move(UInt16 CardNo, UInt16 Crd, UInt16 axisNum, UInt16[] Axis_List, double[] Target_Pos, double[] Cen_Pos, double A_Axis_Len, double B_Axis_Len, UInt16 Dir, UInt16 Pos_Mode);

        //---------------------------看门狗功能----------------------
        //设置看门口触发响应事件（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_watchdog_action_event(UInt16 CardNo, UInt16 event_mask);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_watchdog_action_event(UInt16 CardNo, ref UInt16 event_mask);
        //使能看门口保护机制（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_watchdog_enable(UInt16 CardNo, double timer_period, UInt16 enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_watchdog_enable(UInt16 CardNo, ref double timer_period, ref UInt16 enable);
        //复位看门狗定时器（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_reset_watchdog_timer(UInt16 CardNo);

        //io定制功能（定制类）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_io_check_control(UInt16 CardNo, UInt16 sensor_in_no, UInt16 check_mode, UInt16 A_out_no, UInt16 B_out_no, UInt16 C_out_no, UInt16 output_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_io_check_control(UInt16 CardNo, ref UInt16 sensor_in_no, ref UInt16 check_mode, ref UInt16 A_out_no, ref UInt16 B_out_no, ref UInt16 C_out_no, ref UInt16 output_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_stop_io_check_control(UInt16 CardNo);

        //设置限位反找偏移距离（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_el_ret_deviation(UInt16 CardNo, UInt16 axis, UInt16 enable, double deviation);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_el_ret_deviation(UInt16 CardNo, UInt16 axis, ref UInt16 enable, ref double deviation);

        //两轴位置叠加，高速比较功能（测试使用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_set_config_overlap(UInt16 CardNo, UInt16 hcmp, UInt16 axis, UInt16 cmp_source, UInt16 cmp_logic, Int32 time, UInt16 axis_num, UInt16 aux_axis, UInt16 aux_source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_get_config_overlap(UInt16 CardNo, UInt16 hcmp, ref UInt16 axis, ref UInt16 cmp_source, ref UInt16 cmp_logic, ref Int32 time, ref UInt16 axis_num, ref UInt16 aux_axis, ref UInt16 aux_source);
        
        //启动或者关闭RTCP功能,后续添加

        //螺旋插补(测试使用，DMC5000/5X10系列脉冲卡、E5032总线卡)
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_helix_move_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AixsList, double[] StartPos, double[] TargetPos, UInt16 Arc_Dir, int Circle, UInt16 mode, int mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_helix_move_unit(UInt16 CardNo, UInt16 Crd, UInt16 AxisNum, UInt16[] AxisList, double[] StartPos, double[] TargetPos, UInt16 Arc_Dir, int Circle, UInt16 mode);

        //PDO缓存20190715（内部使用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_enter(UInt16 CardNo, UInt16 axis);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_stop(UInt16 CardNo, UInt16 axis);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_clear(UInt16 CardNo, UInt16 axis);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_run_state(UInt16 CardNo,UInt16 axis, ref int RunState, ref int Remain, ref int NotRunned, ref int Runned);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_add_data(UInt16 CardNo,UInt16 axis, int size, int[] data_table);       
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_start_multi(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, UInt16[] ResultList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_pause_multi(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, UInt16[] ResultList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_stop_multi(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, UInt16[] ResultList);
        //[DllImport("LTDMC.dll")]
        //public static extern short dmc_pdo_buffer_add_data_multi(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, int size, int[][] data_table);
        //保留
        [DllImport("LTDMC.dll")]
        public static extern short dmc_calculate_arccenter_3point(double[] start_pos, double[] mid_pos, double[] target_pos, double[] cen_pos);

        //---------------------指令缓存门型运动------------------
        //指令缓存门型运动（适用于DMC3000/5000系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_set_muti_profile_unit(ushort card, ushort group, ushort axis_num, ushort[] axis_list, double[] start_vel, double[] max_vel, double[] tacc, double[] tdec, double[] stop_vel);//两轴速度设置
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_set_profile_unit(ushort card, ushort group, ushort axis, double start_vel, double max_vel, double tacc, double tdec, double stop_vel);//单轴速度设置
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_sigaxis_moveseg_data(ushort card, ushort group, ushort axis, double Target_pos, ushort process_mode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_sigaxis_move_twoseg_data(ushort card, ushort group, ushort axis, double Target_pos, double second_pos, double second_vel, double second_endvel, ushort process_mode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_mutiaxis_moveseg_data(ushort card, ushort group, ushort axisnum, ushort[] axis_list, double[] Target_pos, ushort process_mode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_mutiaxis_move_twoseg_data(ushort card, ushort group, ushort axisnum, ushort[] axis_list, double[] Target_pos, double[] second_pos, double[] second_vel, double[] second_endvel, ushort process_mode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_ioTrig_movseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, ushort process_mode, ushort trigINbit, ushort trigINstate, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_mutiposTrig_movseg_data(ushort card, ushort group, ushort axis, double Target_pos, ushort process_mode, ushort trigaxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);//位置触发移动
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_mutiposTrig_mov_twoseg_data(ushort card, ushort group, ushort axis, double Target_pos, double softland_pos, double softland_vel, double softland_endvel, ushort process_mode, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);//多轴位置触发移动
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_upseg_data(ushort card, ushort group, ushort axis, double Target_pos, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_up_twoseg_data(ushort card, ushort group, ushort axis, double Target_pos, double second_pos, double second_vel, double second_endvel, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_ioPosTrig_movseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, ushort process_mode, ushort trigAxis, double trigPos, ushort trigPosType, ushort trigMode, ushort TrigINNum, ushort[] trigINList, ushort[] trigINstate, uint mark);//位置+io触发移动
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_ioPosTrig_mov_twoseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, double[] second_pos, double[] second_vel, double[] second_endvel, ushort process_mode, ushort trigAxis, double trigPos, ushort trigPosType, ushort trigMode, ushort TrigINNum, ushort[] trigINList, ushort[] trigINstate, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_posTrig_movseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, ushort process_mode, ushort trigAxis, double trigPos, ushort trigPosType, ushort trigMode, uint mark);//位置触发移动
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_posTrig_mov_twoseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, double[] second_pos, double[] second_vel, double[] second_endvel, ushort process_mode, ushort trigAxis, double trigPos, ushort trigPosType, ushort trigMode, uint mark);//位置触发移动
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_ioPosTrig_down_seg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, ushort trigIN, ushort trigINstate, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_ioPosTrig_down_twoseg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, double second_pos, double second_vel, double second_endvel, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, ushort trigIN, ushort trigINstate, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_posTrig_down_seg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_posTrig_down_twoseg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, double second_pos, double second_vel, double second_endvel, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_posTrig_down_seg_cmd_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, ushort trigAxisNum, ushort[] trigAxisList, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_posTrig_down_twoseg_cmd_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, double second_pos, double second_vel, double second_endvel, ushort trigAxisNum, ushort[] trigAxisList, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_mutiposTrig_singledown_seg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, ushort process_mode, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_mutiposTrig_mutidown_seg_data(ushort card, ushort group, ushort axisnum, ushort[] axis_list, double[] safePos, double[] Target_pos, ushort process_mode, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_posTrig_outbit(ushort card, ushort group, ushort bitno, ushort on_off, ushort ahead_axis, double ahead_value, ushort ahead_PosType, ushort ahead_Mode, uint mark);//位置触发IO输出
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_immediate_write_outbit(ushort card, ushort group, ushort bitno, ushort on_off, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_wait_input(ushort card, ushort group, ushort bitno, ushort on_off, double time_out, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_delay_time(ushort card, ushort group, double delay_time, uint mark);//延时指令
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_get_run_state(ushort card, ushort group, ref ushort state, ref ushort enable, ref uint stop_reason, ref ushort trig_phase, ref uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_open_list(ushort card, ushort group, ushort axis_num, ushort[] axis_list);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_close_list(ushort card, ushort group);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_start_list(ushort card, ushort group);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_stop_list(ushort card, ushort group, ushort stopMode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_pause_list(ushort card, ushort group, ushort stopMode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_set_encoder_error_allow(ushort card, ushort group, double allow_error);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_get_encoder_error_allow(ushort card, ushort group, ref double allow_error);

        //读取所有AD输入（适用于DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_ad_input_all(ushort CardNo, ref double Vout);
        //连续插补暂停后使用pmove（适用于DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_pmove_unit_pausemode(ushort CardNo, ushort axis, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc, double dec, double smooth_time, ushort posi_mode);
        //连续插补暂停使用pmove后，回到暂停位置（适用于DMC5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_return_pausemode(ushort CardNo, ushort Crd, ushort axis);
        //检验接线盒是否支持通讯校验（适用于DMC3000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_check_if_crc_support(ushort CardNo);

        //轴碰撞检测功能接口 （适用于DMC3000系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_axis_conflict_config(ushort CardNo, ushort[] axis_list, ushort[] axis_depart_dir, double home_dist, double conflict_dist, ushort stop_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_axis_conflict_config(ushort CardNo, ushort[] axis_list, ushort[] axis_depart_dir, ref double home_dist, ref double conflict_dist, ref ushort stop_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_axis_conflict_config_en(ushort CardNo, ushort enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_axis_conflict_config_en(ushort CardNo, ref ushort enable);
       
        //物件分拣加通道,分拣固件专用
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_close_ex(ushort CardNo, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_start_ex(ushort CardNo, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_init_config_ex(ushort CardNo, ushort cameraCount, int[] pCameraPos, ushort[] pCamIONo, UInt32 cameraTime, ushort cameraTrigLevel, ushort blowCount, int[] pBlowPos, ushort[] pBlowIONo, UInt32 blowTime, ushort blowTrigLevel, ushort axis, ushort dir, ushort checkMode, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_camera_trig_count_ex(ushort CardNo, ushort cameraNum, UInt32 cameraTrigCnt, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_camera_trig_count_ex(ushort CardNo, ushort cameraNum, ref UInt32 pCameraTrigCnt, ushort count, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_blow_trig_count_ex(ushort CardNo, ushort blowNum, UInt32 blowTrigCnt, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_blow_trig_count_ex(ushort CardNo, ushort blowNum, ref UInt32 pBlowTrigCnt, ushort count, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_camera_config_ex(ushort CardNo, ushort index, ref int pos, ref UInt32 trigTime, ref ushort ioNo, ref ushort trigLevel, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_blow_config_ex(ushort CardNo, ushort index, ref int pos, ref UInt32 trigTime, ref ushort ioNo, ref ushort trigLevel, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_blow_status_ex(ushort CardNo, ref UInt32 trigCntAll, ref ushort trigMore, ref ushort trigLess, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_trig_blow_ex(ushort CardNo, ushort blowNum, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_blow_enable_ex(ushort CardNo, ushort blowNum, ushort enable, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_piece_config_ex(ushort CardNo, UInt32 maxWidth, UInt32 minWidth, UInt32 minDistance, UInt32 minTimeIntervel, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_piece_status_ex(ushort CardNo, ref UInt32 pieceFind, ref UInt32 piecePassCam, ref UInt32 dist2next, ref UInt32 pieceWidth, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_cam_trig_phase_ex(ushort CardNo, ushort blowNo, double coef, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_blow_trig_phase_ex(ushort CardNo, ushort blowNo, double coef, ushort sortModuleNo);
        //获取分拣指令数量函数
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_sortdev_blow_cmd_cnt(ushort CardNo, ushort blowDevNum, ref long cnt);
        //获取未处理指令数量函数函数
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_sortdev_blow_cmderr_cnt(ushort CardNo, ushort blowDevNum, ref long errCnt);
        //分拣队列状态
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_sortqueue_status(ushort CardNo, ref long curSorQueueLen, ref long passCamWithNoCmd);

        // 椭圆连续插补（适用于DMC5X10系列脉冲卡、E5032总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_ellipse_move_unit(ushort CardNo, ushort Crd,ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, double A_Axis_Len, double B_Axis_Len, ushort Dir, ushort Pos_Mode,long mark);
        //获取轴状态函数（预留）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_axis_status_advance(ushort CardNo, ushort axis_no, ushort motion_no, ref ushort axis_plan_state, ref UInt32 ErrPlulseCnt, ref ushort fpga_busy);
        //连续插补vmove（DMC5000系列卡受限使用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_vmove_unit(ushort CardNo, ushort Crd, ushort axis, double vel, double acc, ushort dir, Int32 imark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_vmove_stop(ushort CardNo, ushort Crd, ushort axis, double dec, Int32 imark);

        //---------------------读写掉电保持区------------------//
        //写入字符数据到断电保持区（DMC3000/5000系列卡受限使用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_persistent_reg_byte(ushort CardNo, uint start, uint inum, byte[] pdata);
        //从断电保持区读取写入的字符（DMC3000/5000系列卡受限使用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_persistent_reg_byte(ushort CardNo, uint start, uint inum, byte[] pdata);
        //写入浮点型数据到断电保持区（DMC3000/5000系列卡受限使用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_persistent_reg_float(ushort CardNo, uint start, uint inum, float[] pdata);
        //从断电保持区读取写入的浮点型数据（DMC3000/5000系列卡受限使用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_persistent_reg_float(ushort CardNo, uint start, uint inum, float[] pdata);
        //写入整型数据到断电保持区（DMC3000/5000系列卡受限使用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_persistent_reg_int(ushort CardNo, uint start, uint inum, int[] pdata);
        //从断电保持区读取写入的整型数据（DMC3000/5000系列卡受限使用）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_persistent_reg_int(ushort CardNo, uint start, uint inum, int[] pdata);
        //----------------------------------------------------//

        //EtherCAT总线复位IO模块输出保持开关设置202001（适用于所有EtherCAT总线卡）
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_slave_output_retain(ushort CardNo, ushort Enable);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_slave_output_retain(ushort CardNo, ref ushort Enable);

        //轴参数配置写flash，实现断电保存急停信号配置（适用于DMC3000系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_persistent_param_config(ushort CardNo, ushort axis, uint item);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_persistent_param_config(ushort CardNo, ushort axis, ref uint item);               

        //读取运行时是启动正常固件还是备份固件（适用于DMC3000/5000/5X10系列脉冲卡）
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_firmware_boot_type(ushort CardNo, ref ushort boot_type);

        /**************************中断功能 （适用于DMC5X10系列脉冲卡）************************/
        //开启控制卡中断功能
        [DllImport("LTDMC.dll")]
        public static extern uint dmc_int_enable(ushort CardNo, DMC3K5K_OPERATE funcIntHandler, IntPtr operate_data);
        //禁止控制卡的中断
        [DllImport("LTDMC.dll")]
        public static extern uint dmc_int_disable(ushort CardNo);
        //设置/读取指定控制卡中断通道使能
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_intmode_enable(ushort Cardno,ushort Intno,ushort Enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_intmode_enable(ushort Cardno,ushort Intno,ref ushort Status);
        //设置/读取指定控制卡中断配置
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_intmode_config(ushort Cardno,ushort Intno,ushort IntItem,ushort IntIndex,ushort IntSubIndex,ushort Logic);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_intmode_config(ushort Cardno,ushort Intno,ref ushort IntItem,ref ushort IntIndex,ref ushort IntSubIndex,ref ushort Logic);
        //读取指定控制卡中断通道的中断状态
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_int_status(ushort Cardno,ref uint IntStatus);
        //复位指定控制卡输入口的中断
        [DllImport("LTDMC.dll")]
        public static extern short dmc_reset_int_status(ushort Cardno, ushort Intno);
        /**************************************************************************************/

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
        };
    }
}
