using System;
using System.Configuration;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Mfs.Watcher.Dashboard.Entities;

namespace Mfs.Watcher.Dashboard
{
    public class Watcher
    {
        #region Locals
        private string processName;
        private Timer timer;
        private const string filename = @"Log.txt";
        private TaskbarIcon taskbarIcon;
        private INotification subscriber;
        private static Watcher instance;
        private ConnectionStatus status;
        private static readonly object locker = new object();
        #endregion

        #region Constructor
        private Watcher()
        {
            status = new ConnectionStatus() { Status = ConnectionState.NotConnected };
            timer = new Timer();
            timer.Interval = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["PollingInterval"]) ? Convert.ToDouble(ConfigurationManager.AppSettings["PollingInterval"]) : 10000;
            timer.Elapsed += TimeElapsed;
        }
        #endregion

        #region Properties
        public ConnectionStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }

        public static Watcher Instance
        {
            get
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new Watcher();
                    }
                    return instance;
                }
            }
        }
        #endregion

        #region Methods

        public void SubscribeForTaskbarNotification(TaskbarIcon icon)
        {
            this.taskbarIcon = icon;
        }

        public void Subscribe(INotification subscriber)
        {
            this.subscriber = subscriber;
        }

        public void InitializeWatchers()
        {
            processName = ConfigurationManager.AppSettings["processName"];
            try
            {
                CreateWatcher(processName);
            }
            catch (Exception ex)
            {
                AddUpdateStatus(processName, ConnectionState.NotConnected, ex.Message);
            }
            timer.Enabled = true;
        }

        private void TimeElapsed(object sender, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            try
            {
                try
                {
                    if (Status != null && Status.Status == ConnectionState.NotConnected)
                    {
                        CreateWatcher(processName);
                    }
                }
                catch (Exception ex)
                {
                    AddUpdateStatus(processName, ConnectionState.NotConnected, ex.Message);
                }
            }
            finally
            {
                timer.Enabled = true;
                //subscriber.UpdateConnectionStatus();
            }
        }

        private void CreateWatcher(string AppName)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(AppName))
                {
                    clsProcess.EnableRaisingEvents = true;
                    clsProcess.Exited += ClsProcess_Exited;
                    AddUpdateStatus(processName, ConnectionState.Connected, "Process Started at " + DateTime.Now);
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ((NotifyIconViewModel)this.taskbarIcon.DataContext).IconPath = "/Active.ico";
                    }), DispatcherPriority.Normal);

                   
                    return;
                }
            }
            this.taskbarIcon.ShowBalloonTip("Error", string.Format("{0} is not running", processName), BalloonIcon.Error);
        }

        private void ClsProcess_Exited(object sender, EventArgs e)
        {
            AddUpdateStatus(processName, ConnectionState.NotConnected, "Process Exited at " + DateTime.Now);
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ((NotifyIconViewModel)this.taskbarIcon.DataContext).IconPath = "/Inactive.ico";
            }), DispatcherPriority.Normal);
        }

        private void AddUpdateStatus(string processName, ConnectionState status, string error)
        {
            Status = new ConnectionStatus { ProcessName = processName, Status = status, Error = error };
        }

        #endregion
    }
}
