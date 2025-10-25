using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace BankApp
{
    public partial class Main : Form
    {
        public Main()
        {
          
            InitializeComponent();
        }
        private CustomerForm customerForm;
        private AccountsForm accountsForm;
        private TransactionForm transactionForm;

        private UsersForm usersforms;

        private void OpenOrBringToFront<T>(ref T form) where T : Form, new()
        {
            if (form == null || form.IsDisposed)
            {
                form = new T();
                form.TopLevel = false;
                
                form.WindowState = FormWindowState.Maximized;
               
                panel1.Controls.Add(form);
                form.Show();
            }
            form.BringToFront(); 
        }
        private async void Main_Load(object sender, EventArgs e)
        {
            await webView21.EnsureCoreWebView2Async(null);

            webView21.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
          
            string htmlPath = Path.Combine(Application.StartupPath, "menu.html");
            webView21.Source = new Uri(htmlPath);
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string action = e.TryGetWebMessageAsString();

            switch (action)
            {
                case "Customers":
                    OpenOrBringToFront(ref customerForm);
                   
                    break;
                case "Account":
                    OpenOrBringToFront(ref accountsForm);
                    break;
                case "Transactions":
                    OpenOrBringToFront(ref transactionForm);
                    break;
                case "users":
                    if (GlobalUser.Role == "admin")
                    {
                        OpenOrBringToFront(ref usersforms);
                    }
                    else
                        MessageBox.Show("Only Admin can open this form");
                    break;
                case "close":
                    Application.Exit();
                    break;

                case "Logout":
                    var loginForm = Application.OpenForms
        .OfType<Login>()
        .FirstOrDefault();

                    if (loginForm != null)
                    {
                        loginForm.Show(); 
                        this.Close();
                    }
                    break;
            }
        }

       
    }
}
