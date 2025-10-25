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
    public partial class Login : Form
    {
        public Login()
        {
            this.Visible = false;
            InitializeComponent();
          
        }

        BankAppEntities db = new BankAppEntities();

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
    


        }

        private async void Login_Load(object sender, EventArgs e)
        {
            
            await webView21.EnsureCoreWebView2Async(null);

            webView21.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

           
            webView21.NavigationCompleted += (s, ev) =>
            {
                if (ev.IsSuccess)
                {
                   
                    this.Visible = true;
                }
                else
                {
                    MessageBox.Show("Failed to load WebView content.");
                }
            };

            
            string htmlPath = Path.Combine(Application.StartupPath, "loading.html");
            webView21.Source = new Uri(htmlPath);


        }

        private async void CoreWebView2_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string json = e.TryGetWebMessageAsString();
                var login = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginData>(json);

                var userRow = db.Users.FirstOrDefault(u => u.Username == login.Username && u.IsActive);

                if (userRow != null)
                {
                    if (PasswordHasher.VerifyPassword(login.Password, userRow.Password))
                    {
                       
                        GlobalUser.Username = userRow.FullName;
                        GlobalUser.UserId = userRow.UserId;
                        GlobalUser.Role = db.RoleTBLs.Where(x=>x.Id.ToString()== userRow.Role.ToString()).FirstOrDefault().RoleName;

                        
                        await webView21.ExecuteScriptAsync("showAlert('success', 'Login successful! Redirecting...');");

                      
                        await Task.Delay(2000);

                        
                        Main m = new Main();
                        m.Show();
                        this.Hide();
                    }
                    else
                    {
                       
                        await webView21.ExecuteScriptAsync("showAlert('error', 'Invalid password!');");
                    }
                }
                else
                {
                   
                    await webView21.ExecuteScriptAsync("showAlert('warning', 'User not found or inactive.');");
                }
            }
            catch (Exception ex)
            {
                await webView21.ExecuteScriptAsync($"showAlert('error', 'Unexpected error: {ex.Message}');");
            }
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }
    }

    public class LoginData
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
