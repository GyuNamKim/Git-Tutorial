using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSharpWPFRockey
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("Rockey4ND_X64.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern short Rockey(short func, ref ushort handle, ref uint lp1, ref uint lp2, ref ushort p1, ref ushort p2, ref ushort p3, ref ushort p4, [In, Out] byte[] buffer);

        private const short RY_FIND = 1;
        private const short RY_FIND_NEXT = 2;
        private const short RY_OPEN = 3;
        private const short RY_CLOSE = 4;
        private const short RY_READ = 5;
        private const short RY_WRITE = 6;
        private const short RY_RANDOM = 7;
        private const short RY_SEED = 8;
        private const short RY_WRITE_USERID = 9;
        private const short RY_READ_USERID = 10;
        private const short RY_SET_MOUDLE = 11;
        private const short RY_CHECK_MOUDLE = 12;
        private const short RY_WRITE_ARITHMETIC = 13;
        private const short RY_CALCULATE1 = 14;
        private const short RY_CALCULATE2 = 15;
        private const short RY_CALCULATE3 = 16;
        private const short RY_DECREASE = 17;

        private readonly ushort[] m_HIndex = new ushort[32];
        private int m_HandleNum = 0;

        private uint m_lp1 = 0;
        private uint m_lp2 = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitPassword(out ushort p1, out ushort p2, out ushort p3, out ushort p4)
        {
            // p1 = Basic Password1
            // p2 = Basic Password2
            // p3 = Advanced Password1
            // p4 = Advanced Password2
            p1 = 0xE5F8;
            p2 = 0x262A;
            p3 = 0xEAEF;
            p4 = 0x2A15;
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            ushort p1, p2, p3, p4;
            uint lp1 = 0;
            uint lp2 = 0;
            byte[] buffer = new byte[1024];

            InitPassword(out p1, out p2, out p3, out p4);

            short ret = Rockey(RY_FIND, ref m_HIndex[0], ref lp1, ref lp2, ref p1, ref p2, ref p3, ref p4, buffer);

            if (!CheckRockeyResult("RY_FIND", ret))
                return;

            m_HandleNum = 1;
            m_lp1 = lp1;
            m_lp2 = lp2;

            Log($"RY_FIND 성공");
            Log($"HID : {lp1:X}");
            Log($"Handle : {m_HIndex[0]}");
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (m_HandleNum <= 0)
            {
                MessageBox.Show("먼저 FIND를 실행해야 합니다.", "ROCKEY4ND", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ushort p1, p2, p3, p4;
            uint lp1 = m_lp1;
            uint lp2 = m_lp2;
            byte[] buffer = new byte[1024];

            InitPassword(out p1, out p2, out p3, out p4);

            short ret = Rockey(RY_OPEN, ref m_HIndex[0], ref lp1, ref lp2, ref p1, ref p2, ref p3, ref p4, buffer);

            if (!CheckRockeyResult("RY_OPEN", ret))
                return;

            Log("RY_OPEN 성공");
            Log($"Handle : {m_HIndex[0]}");
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (m_HandleNum <= 0)
            {
                MessageBox.Show("Open 또는 Find된 ROCKEY4ND 정보가 없습니다.", "ROCKEY4ND", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ushort p1, p2, p3, p4;
            uint lp1 = m_lp1;
            uint lp2 = m_lp2;
            byte[] buffer = new byte[1024];

            InitPassword(out p1, out p2, out p3, out p4);

            short ret = Rockey(RY_CLOSE, ref m_HIndex[0], ref lp1, ref lp2, ref p1, ref p2, ref p3, ref p4, buffer);

            if (!CheckRockeyResult("RY_CLOSE", ret))
                return;

            Log("RY_CLOSE 성공");

            m_HandleNum = 0;
            m_HIndex[0] = 0;
            m_lp1 = 0;
            m_lp2 = 0;
        }

        private void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            txtLog.ScrollToEnd();
        }

        private static bool CheckRockeyResult(string functionName, short ret)
        {
            if (ret == 0)
                return true;

            string message = $"ROCKEY4ND 함수 실행 실패\n\n" + $"Function : {functionName}\n" + $"Return Code : {ret}\n" + $"Error : {GetRockeyErrorMessage(ret)}";

            MessageBox.Show(message, "ROCKEY4ND Error", MessageBoxButton.OK, MessageBoxImage.Error);

            return false;
        }

        private static string GetRockeyErrorMessage(short errorCode)
        {
            switch (errorCode)
            {
                case 0:
                    return "ERR_SUCCESS : Success";

                case 3:
                    return "ERR_NO_ROCKEY : ROCKEY4ND를 찾지 못했습니다.";

                case 4:
                    return "ERR_INVALID_PASSWORD : Basic Password가 맞지 않습니다.";

                case 5:
                    return "ERR_INVALID_PASSWORD_OR_ID : Password 또는 HID가 잘못되었습니다.";

                case 6:
                    return "ERR_SETID : ROCKEY4ND HID를 Setting하지 못했습니다.";

                case 7:
                    return "ERR_INVALID_ADDR_OR_SIZE : Read/Write 시 Address 또는 Length가 잘못되었습니다.";

                case 8:
                    return "ERR_UNKNOWN_COMMAND : 명령어를 찾지 못했습니다.";

                case 9:
                    return "ERR_NOTBELEVEL3 : Internal Error";

                case 10:
                    return "ERR_READ : Read Error";

                case 11:
                    return "ERR_WRITE : Write Error";

                case 12:
                    return "ERR_RANDOM : Random Number Error";

                case 13:
                    return "ERR_SEED : Seed Code Error";

                case 14:
                    return "ERR_CALCULATE : Calculate Error";

                case 15:
                    return "ERR_NO_OPEN : 사용하려는 ROCKEY4ND를 Open하지 못했습니다.";

                case 16:
                    return "ERR_OPEN_OVERFLOW : 많은 ROCKEY4ND가 Open되어 있습니다. 최대 16개까지 Open 가능합니다.";

                case 17:
                    return "ERR_NOMORE : ROCKEY4ND가 더 이상 없습니다.";

                case 18:
                    return "ERR_NEED_FIND : FindNext 하기 전에 ROCKEY4ND를 찾지 못했습니다.";

                case 19:
                    return "ERR_DECREASE : Decrease Error";

                case 20:
                    return "ERR_AR_BADCOMMAND : Arithmetic 구조 Error";

                case 21:
                    return "ERR_AR_UNKNOWN_OPCODE : Arithmetic Operator Error";

                case 22:
                    return "ERR_AR_WRONGBEGIN : 설정한 Arithmetic 명령의 첫 부분을 사용할 수 없습니다.";

                case 23:
                    return "ERR_AR_WRONG_END : 설정한 Arithmetic 명령의 마지막 부분을 사용할 수 없습니다.";

                case 24:
                    return "ERR_AR_VALUEOVERFLOW : 사용할 Module Number가 63을 초과했습니다. Module Value는 0~63까지입니다.";

                case 25:
                    return "ERR_TOOMUCHTHREAD : ROCKEY4ND에서 사용하는 Single Process의 Open한 Thread가 너무 많습니다. 최대 100개까지 사용 가능합니다.";

                case 0x100:
                    return "ERR_RECEIVE_NULL : Null값이 Return되었습니다.";

                case 0x102:
                    return "ERR_UNKNOWN_SYSTEM : 운영체제에서 발생한 Error입니다.";

                case unchecked((short)0xFFFF):
                    return "ERR_UNKNOWN : 선언된 Error Code가 없습니다.";

                default:
                    return $"알 수 없는 ROCKEY4ND Error Code입니다. Code = {errorCode}";
            }
        }
    }
}
