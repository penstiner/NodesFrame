using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using Shell.Services;
using Shell.Views;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return new MainWindow();
        }

        protected override void RegisterTypes(IContainerRegistry services)
        {
            services.RegisterForNavigation<MainWindow>();
            // GraphExecutor 已移除，使用 FlowExecutor.RunAsync() 替代
            services.RegisterSingleton<IGraphSerializer, GraphSerializer>();
            services.RegisterSingleton<INodeDialogService, NodeDialogService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            //加载自己写的模块
            //moduleCatalog.AddModule<CoreToolsModule>();//加载工具模块
        }

        //protected override IModuleCatalog CreateModuleCatalog()
        //{
        //    return new DirectoryModuleCatalog() { ModulePath = Environment.CurrentDirectory + "\\Modules" };//配置模块目录
        //}

        public App()
        {
            //程序域异常
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                //Logs.LogError((Exception)(e.ExceptionObject));
            };

            //应用程序异常
            Application.Current.DispatcherUnhandledException += (s, e) =>
            {
                //Logs.LogError(e.Exception);
            };

            //多线程异常
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                //Logs.LogError(e.Exception);
            };
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            //PrismProvider.EventAggregator.GetEvent<ApplicationExitEvent>().Publish();//程序退出时触发事件
        }

        #region 设置只能运行一个实例

        private const int WS_SHOWNORMAL = 1;
        // 设置窗口的显示状态，而无需等待操作完成。
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hwnd, int cmdShow);
        // 将创建指定窗口的线程引入前台并激活窗口。 键盘输入将定向到窗口，
        // 并为用户更改各种视觉提示。 系统为创建前台窗口的线程分配的优先级略高于其他线程。
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        private static Process GetRunningInstance()
        {
            Process pCurrent = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(pCurrent.ProcessName);
            foreach (Process p in processes)
            {
                if (p.Id != pCurrent.Id)
                {
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == pCurrent.MainModule.FileName)
                    {
                        return p;
                    }
                }
            }
            return null;
        }

        private static void HandleRunningInstance(Process instance)
        {
            ShowWindowAsync(instance.MainWindowHandle, WS_SHOWNORMAL);
            SetForegroundWindow(instance.MainWindowHandle);
        }

        private static Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            Process instance = GetRunningInstance();
            mutex = new Mutex(true, "ShellApp", out bool ret);
            if (ret)
            {
                // 注册变量类型处理器（扩展新类型只需 Register 新 Handler）
                VariableTypeRegistry.Register(new BooleanTypeHandler());
                VariableTypeRegistry.Register(new DoubleTypeHandler());
                VariableTypeRegistry.Register(new Int32TypeHandler());
                VariableTypeRegistry.Register(new StringTypeHandler());

                base.OnStartup(e);
            }
            else
            {
                HandleRunningInstance(instance);
                Environment.Exit(0);
            }
        }

        #endregion
    }

}
