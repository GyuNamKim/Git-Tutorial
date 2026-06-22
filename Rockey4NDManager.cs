using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace CSharpWPFRockey
{
    public class LicenseManager
    {
        public static LicenseManager Instance { get; } = new LicenseManager();

        [DllImport("Rockey4ND_X64.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern short Rockey(short func, ref ushort handle, ref uint lp1, ref uint lp2, ref ushort p1, ref ushort p2, ref ushort p3, ref ushort p4, [In, Out] byte[] buffer);

        private const short RY_FIND = 1;
        private const short RY_OPEN = 3;
        private const short RY_CLOSE = 4;

        private readonly ushort[] _handle = new ushort[32];

        private uint _lp1 = 0;
        private uint _lp2 = 0;

        private DispatcherTimer _timer;
        private bool _isOpened;

        private LicenseManager()
        {
        }

        public bool CheckDongle()
        {
            ushort p1, p2, p3, p4;
            uint lp1 = 0;
            uint lp2 = 0;
            byte[] buffer = new byte[1024];

            InitPassword(out p1, out p2, out p3, out p4);

            short ret = Rockey(RY_FIND, ref _handle[0], ref lp1, ref lp2, ref p1, ref p2, ref p3, ref p4, buffer);

            if (ret != 0)
            {
                _isOpened = false;
                return false;
            }

            _lp1 = lp1;
            _lp2 = lp2;

            InitPassword(out p1, out p2, out p3, out p4);

            ret = Rockey(RY_OPEN, ref _handle[0], ref _lp1, ref _lp2, ref p1, ref p2, ref p3, ref p4, buffer);

            if (ret != 0)
            {
                _isOpened = false;
                return false;
            }

            _isOpened = true;
            return true;
        }

        public void StartMonitor()
        {
            if (_timer != null)
                return;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(30);
            _timer.Tick += LicenseTimer_Tick;
            _timer.Start();
        }

        public void StopMonitor()
        {
            if (_timer == null)
                return;

            _timer.Stop();
            _timer.Tick -= LicenseTimer_Tick;
            _timer = null;
        }

        public void CloseDongle()
        {
            if (!_isOpened)
                return;

            ushort p1, p2, p3, p4;
            byte[] buffer = new byte[1024];

            InitPassword(out p1, out p2, out p3, out p4);

            Rockey(RY_CLOSE, ref _handle[0], ref _lp1, ref _lp2, ref p1, ref p2, ref p3, ref p4, buffer);

            _isOpened = false;
            _handle[0] = 0;
            _lp1 = 0;
            _lp2 = 0;
        }

        private void LicenseTimer_Tick(object sender, EventArgs e)
        {
            if (CheckDongle())
                return;

            StopMonitor();

            MessageBox.Show("사용 중 라이선스 동글이 제거되었습니다.\n프로그램을 종료합니다.", "ROCKEY4ND", MessageBoxButton.OK, MessageBoxImage.Error);

            Application.Current.Shutdown();
        }

        private void InitPassword(out ushort p1, out ushort p2, out ushort p3, out ushort p4)
        {
            p1 = 0xE5F8;
            p2 = 0x262A;
            p3 = 0xEAEF;
            p4 = 0x2A15;
        }
    }
}