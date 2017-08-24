using log4net;
using System.Linq;
using System.ServiceProcess;
using TqLib.Dashboard;

namespace Installer
{
    public class MainWindowInitializer
    {
        private MainWindow MainWindow;

        public MainWindowInitializer(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }

        public void Initialize()
        {
            Init_Log4net();
            if (CheckCCNET())
            {
                Init_Paths();
            }
        }

        private bool CheckCCNET()
        {
            if (!ServiceController.GetServices().Any(t => "CCService".Equals(t.ServiceName)))
            {
                MainWindow.btnConfirm.Content = "CCNET 이 설치되지 않았습니다.";
                MainWindow.btnConfirm.IsEnabled = false;
                return false;
            }
            return true;
        }

        private void Init_Paths()
        {
            MainWindow.DashboardPath = Properties.Settings.Default.DashboardFolder;
            MainWindow.DashboardUrl = Properties.Settings.Default.DashboardUrl;
            MainWindow.PluginUrl = Properties.Settings.Default.PluginUrl;
        }

        private void Init_Log4net()
        {
            new Log4netInitializer().Initialize();
            MainWindow.Logger = LogManager.GetLogger(typeof(MainWindow));
            MainWindow.CustomEventAppender = MainWindow.Logger.Logger.Repository.GetAppenders()[0] as CustomEventAppender;
            MainWindow.CustomEventAppender.ProcessChanged += CustomEventAppender_ProcessChanged;
        }

        private void CustomEventAppender_ProcessChanged(object sender, CustomEventAppenderArgs e)
        {
            MainWindow.ProgressDesc = e.Msg;
        }
    }
}