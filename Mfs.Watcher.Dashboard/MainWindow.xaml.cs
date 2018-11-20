using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Mfs.Watcher.Dashboard.Entities;

namespace Mfs.Watcher.Dashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, INotification
    {
        private string foldersToLook;
        private const string filename = @"Log.txt";

        private ObservableCollection<string> logs;
        private ConnectionStatus connectionStatus;

        /// <summary>
        /// Folders To Look
        /// </summary>
        public string FoldersToLook
        {
            get { return foldersToLook; }
        }

        /// <summary>
        /// Connection Status of Configured Folders
        /// </summary>
        public ConnectionStatus ConnectionStatus
        {
            get { return connectionStatus; }
            set
            {
                connectionStatus = value;
                this.OnPropertyChanged("ConnectionStatus");
            }
        }

        /// <summary>
        /// List Of updated entries
        /// </summary>
        public ObservableCollection<string> Logs
        {
            get { return logs; }
            set
            {
                logs = value;
                this.OnPropertyChanged("Logs");
            }
        }

        /// <summary>
        /// constructor of MainWindow
        /// </summary>
        public MainWindow()
        {
            if (logs == null)
                logs = new ObservableCollection<string>();
            this.DataContext = this;
            CheckDirectoryStatus();
            ReadLog();
            foldersToLook = ConfigurationManager.AppSettings["CommaSepFolders"];
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        private void CheckDirectoryStatus()
        {
            Watcher.Instance.Subscribe(this);
            this.ConnectionStatus = Watcher.Instance.Status;
        }

        /// <summary>
        /// Helper method to read logs
        /// </summary>
        private void ReadLog()
        {
            if (System.IO.File.Exists(filename))
            {
                using (StreamReader streamReader = new StreamReader(filename))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        string newLine = String.Concat(line, Environment.NewLine);
                        logs.Add(newLine);
                    }
                }
            }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region INotification
        public void UpdateMovieList(string dataRow)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                logs.Add(dataRow);
            }), DispatcherPriority.Normal);
        }

        public void UpdateConnectionStatus()
        {
            this.ConnectionStatus = Watcher.Instance.Status;
        }
        #endregion
    }
}
