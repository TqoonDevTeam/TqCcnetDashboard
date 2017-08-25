using log4net;
using Microsoft.Web.Administration;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TqLib.ccnet.Local;
using TqLib.ccnet.Utils;
using TqLib.Dashboard;

namespace Installer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        internal ILog Logger { get; set; }
        internal CustomEventAppender CustomEventAppender { get; set; }
        private MainWindowInitializer MainWindowInitializer;

        #region Bindings

        public string DashboardPath { get { return dashboardPath; } set { dashboardPath = value; OnPropertyChanged(); } }
        private string dashboardPath = string.Empty;

        public string DashboardUrl { get { return dashboardUrl; } set { dashboardUrl = value; OnPropertyChanged(); } }
        private string dashboardUrl = string.Empty;

        public string PluginUrl { get { return pluginUrl; } set { pluginUrl = value; OnPropertyChanged(); } }
        private string pluginUrl = string.Empty;

        public string ProgressDesc { get { return progressDesc; } set { progressDesc = value; OnPropertyChanged(); } }
        private string progressDesc;

        #endregion Bindings

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            MainWindowInitializer = new MainWindowInitializer(this);
            MainWindowInitializer.Initialize();
        }

        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion OnPropertyChanged

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var task = new Task(() =>
            {
                bool wasSuccess = false;
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        btnConfirm.IsEnabled = false;
                        prgs.Visibility = Visibility.Visible;
                    });

                    Install_Dashboard();
                    Install_DefaultPlugin();
                    Install_IIS();

                    wasSuccess = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.InnerException?.Message ?? string.Empty);
                }
                finally
                {
                    Dispatcher.Invoke(() =>
                    {
                        btnConfirm.IsEnabled = true;
                        prgs.Visibility = Visibility.Hidden;
                    });
                    if (Directory.Exists(Properties.Settings.Default.DownloadFolder)) Directory.Delete(Properties.Settings.Default.DownloadFolder, true);

                    if (wasSuccess) Environment.Exit(0);
                }
            });
            task.Start();
        }

        private void Install_Dashboard()
        {
            Logger.Info("install dashboard");

            new CcnetServiceConfigInitializer(CCNET.ServiceDirectory).Initialize();

            string downloadUrl;
            if (Properties.Settings.Default.DashboardUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var version = new DashboardVersionChecker().GetRemoteVersion();
                downloadUrl = string.Format(Properties.Settings.Default.DashboardUrl, version);
            }
            else
            {
                downloadUrl = Properties.Settings.Default.DashboardUrl;
            }

            new DashboardUpdator()
            {
                DownloadFolder = Properties.Settings.Default.DownloadFolder,
                DashboardFolder = Properties.Settings.Default.DashboardFolder,
                DownloadUrl = downloadUrl,
                Logger = Logger,
                SystemLogger = Logger
            }.Update();
        }

        private void Install_DefaultPlugin()
        {
            Logger.Info("install plugin");

            string downloadUrl;
            if (Properties.Settings.Default.DashboardUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var version = new DashboardVersionChecker().GetRemoteVersion();
                downloadUrl = string.Format(Properties.Settings.Default.PluginUrl, version);
            }
            else
            {
                downloadUrl = Properties.Settings.Default.PluginUrl;
            }

            new PluginUpdator()
            {
                DownloadFolder = Properties.Settings.Default.DownloadFolder,
                PluginDirectory = CCNET.PluginDirectory,
                ServiceDirecotry = CCNET.ServiceDirectory,
                DownloadUrl = downloadUrl,
                Logger = Logger,
                SystemLogger = Logger
            }.Update();
        }

        private void Install_IIS()
        {
            string siteName = "TqDashboard";
            string poolName = "TqDashboardPool";
            string physicalPath = DashboardPath;
            using (var iis = new ServerManager())
            {
                Site site = iis.Sites[siteName];
                if (site == null)
                {
                    site = iis.Sites.Add(siteName, physicalPath, 0);
                    site.Bindings.Clear();
                }
                else
                {
                    return;
                }
                site.Name = siteName;

                site.ApplicationDefaults.ApplicationPoolName = poolName;

                ApplicationPool pool = iis.ApplicationPools[poolName];
                if (pool == null)
                {
                    pool = iis.ApplicationPools.Add(poolName);
                }

                pool.Name = poolName;
                pool.Enable32BitAppOnWin64 = false;
                pool.ManagedRuntimeVersion = "v4.0";
                pool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                pool.Failure.OrphanWorkerProcess = false;
                pool.ProcessModel.MaxProcesses = 1;
                pool.ProcessModel.LoadUserProfile = true;
                pool.ProcessModel.IdentityType = ProcessModelIdentityType.LocalSystem;

                // set binginds
                site.Bindings.Clear();
                site.Bindings.Add("*:80:local.ccnet", "http");

                iis.CommitChanges();
            }
        }
    }
}