using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankApp
{
    public static class GlobalUser
    {
        public static string Username { get; set; }   // Store the username
        public static int UserId { get; set; }        // Store user ID if needed
        public static string Role { get; set; }       // Optional: user role
    }

    public static class HMACKey
    {
        public static string key = "bankapp";
    }

    public static class TextBoxExtensions
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)]string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        public static void SetPlaceholder(this TextBox textBox, string placeholder)
        {
            SendMessage(textBox.Handle, EM_SETCUEBANNER, 0, placeholder);
        }
    }
}
