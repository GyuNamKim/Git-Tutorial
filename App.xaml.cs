using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace CSharpWPFRockey
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!LicenseManager.Instance.CheckDongle())
            {
                MessageBox.Show("라이선스 동글이 없습니다.\n프로그램을 종료합니다.", "ROCKEY4ND", MessageBoxButton.OK, MessageBoxImage.Error);

                Shutdown();
                return;
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            LicenseManager.Instance.StartMonitor();
        }
    }
}
