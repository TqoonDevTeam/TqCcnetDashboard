using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using TqLib.ccnet.Local;
using TqLib.ccnet.Local.Helper;

namespace Installer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly string downloadPath = @"C:\069d9695-3887-4a5b-86e9-d346b1099b52";
        private string installPath = string.Empty;
        private int progressPercentage;
        private string progressDesc;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            installPath = Path.Combine(new DirectoryInfo(CCNET.ServiceDirectory).Parent.FullName, "tqoondashboard");
        }

        #region Bindings

        public string InstallPath
        {
            get
            {
                return installPath;
            }

            set
            {
                installPath = value;
                OnPropertyChanged();
            }
        }

        public int ProgressPercentage
        {
            get
            {
                return progressPercentage;
            }

            set
            {
                progressPercentage = value;
                OnPropertyChanged();
            }
        }

        public string ProgressDesc
        {
            get
            {
                return progressDesc;
            }

            set
            {
                progressDesc = value;
                OnPropertyChanged();
            }
        }

        #endregion Bindings

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
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        btnConfirm.IsEnabled = false;
                    });
                    Install();
                    Install_IIS();
                    if (Directory.Exists(downloadPath)) Directory.Delete(downloadPath, true);
                    Environment.Exit(0);
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
                    });
                    if (Directory.Exists(downloadPath)) Directory.Delete(downloadPath, true);
                }
            });
            task.Start();
        }

        private void Install()
        {
            var configUpdator = new CcnetServiceConfigUpdator(CCNET.ServiceDirectory);
            configUpdator.Update();

            var updator = new TqCcnetDashboardUpdator() { DownloadFolder = downloadPath, PluginFolder = CCNET.PluginDirectory, DashboardFolder = InstallPath, ServiceFolder = CCNET.ServiceDirectory };
            updator.ProcessChanged += Updator_ProcessChanged;
            updator.Update();
        }

        private void Install_IIS()
        {
            string siteName = "TqDashboard";
            string poolName = "TqDashboardPool";
            string physicalPath = InstallPath;
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

        private void Updator_ProcessChanged(object sender, List<TqCcnetDashboardUpdator.TqCcnetDashboardUpdatorEventArgs> e)
        {
            ProgressDesc = e.LastOrDefault()?.Desc ?? string.Empty;
            ProgressPercentage = e.LastOrDefault()?.ProgressPercentage ?? 0;
        }
    }
}