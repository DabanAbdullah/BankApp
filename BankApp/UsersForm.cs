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

using Newtonsoft.Json;

namespace BankApp
{
    public partial class UsersForm : Form
    {
        public UsersForm()
        {
            InitializeComponent();
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
        }

        int userId;
        private async void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return;

            var row = dataGridView1.SelectedRows[0];

            userId = int.Parse(row.Cells["userId"].Value.ToString());
            //  MessageBox.Show(selectedtransactionId+"");



            string script = $@"
(() => {{
    const init = () => {{
        document.getElementById('create').disabled = true;
        document.getElementById('delete').disabled = false;
        document.getElementById('update').disabled = false;
        document.getElementById('password').value='set new pass';
        document.getElementById('fullname').value = '{row.Cells["fullName"].Value?.ToString().Replace("'", "\\'")}';
        document.getElementById('username').value = '{row.Cells["username"].Value?.ToString().Replace("'", "\\'")}';
      document.getElementById('isactive').checked = {(row.Cells["IsActive"].Value?.ToString().ToLower() == "true" ? "true" : "false")};
     const select = document.getElementById('role');
const roleId = '{row.Cells["role"].Value?.ToString()}'; // C# injects the Id

for (let i = 0; i < select.options.length; i++) {{
    if (select.options[i].value === roleId) {{
        select.selectedIndex = i;
        break;
    }}
}}


       
    }};

    if (document.readyState === 'complete' || document.readyState === 'interactive') {{
        init();
    }} else {{
        document.addEventListener('DOMContentLoaded', init);
    }}
}})();
";
            await webView21.ExecuteScriptAsync(script);
        }

        BankAppEntities db = new BankAppEntities();

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void UsersForm_Load(object sender, EventArgs e)
        {
            // Prepare role options HTML
            var roles = db.RoleTBLs
                .Select(r => new { r.Id, r.RoleName })
                .ToList();

            dataGridView1.DataSource = db.Users.Where(x => x.Deleted == false).Select(r => new { r.UserId, r.FullName, r.Username, r.Password, r.Role, r.IsActive }).ToList();

            string optionsHtml = string.Join("",
                roles.Select(r => $"<option value='{r.Id}'>{r.RoleName}</option>")
            );

            // Full HTML content
            string htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<title>User Form</title>
<script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script>
<style>
   body {{
        font-family: 'Segoe UI', sans-serif;
        background: #f4f6f9;
        padding: 10px;
        color: #333;
    }}
    form {{
        max-width: 350px;
        margin: 0 auto;
        background: white;
        padding: 20px;
        border-radius: 12px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }}
    input, select {{
        width: 100%;
        padding: 12px;
        margin: 6px 0;
        border: 1px solid #ccc;
        border-radius: 6px;
        box-sizing: border-box;
        font-size: 14px;
    }}
    button {{
        background-color: #0066cc;
        color: white;
        border: none;
        padding: 10px 16px;
        border-radius: 6px;
        cursor: pointer;
        transition: 0.2s;
        font-size: 14px;
    }}

    button:hover {{
        background-color: #004b99;
    }}
    button:disabled {{
        background-color: #ccc !important;
        color: #666 !important;
        cursor: not-allowed;
        opacity: 0.6;
    }}
</style>
</head>
<body>

<form id='userForm' onsubmit='return false;'> <!-- prevent default submit -->
<h1>Users</h1>
<hr/>
<br/>
    <label>Username *</label>
    <input type='text' id='username' required />

    <label>Full Name</label>
    <input type='text' id='fullname' />

    <label>Password *</label>
    <input type='text' id='password' required />

    <label>Role *</label>
    <select id='role' required>
        <option value=''>Select Role</option>
        {optionsHtml}
    </select>

    <label>Active User</label>
    <input type='checkbox' id='isactive' checked />

    <button id='create' type='button' onclick='sendUserAction(""create"")'>Save</button>
    <button id='delete' type='button' onclick='sendUserAction(""delete"")' disabled>Delete</button>
    <button id='update' type='button' onclick='sendUserAction(""update"")' disabled>update</button>
<button id='update' type='button' onclick='clearform()' >Clear</button>
</form>

<script>

function clearform(){{
document.getElementById('fullname').value='';
document.getElementById('username').value='';
document.getElementById('password').value='';

 document.getElementById('create').disabled = false;
 document.getElementById('delete').disabled = true;
 document.getElementById('update').disabled = true;
  document.getElementById('role').selectedIndex = 0;
}}


function showsuccess() {{
    clearform();
    Swal.fire({{
        icon: 'success',
        title: 'Success',
        text: 'Operation completed successfully!',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    }});
}}



function sendUserAction(action) {{
const username = document.getElementById('username').value.trim();
    const fullname = document.getElementById('fullname').value.trim();
    const password = document.getElementById('password').value.trim();
    const role = document.getElementById('role').value;
    const isActive = document.getElementById('isactive').checked;
    


    // Validation
    if (!username) {{
        Swal.fire({{ icon: 'warning', title: 'Missing Username', text: 'Please enter a username.' }});
        return;
    }}
    if (!fullname) {{
        Swal.fire({{ icon: 'warning', title: 'Missing Full Name', text: 'Please enter a full name.' }});
        return;
    }}
    if (!password) {{
        Swal.fire({{ icon: 'warning', title: 'Missing Password', text: 'Please enter a password.' }});
        return;
    }}
    if (!role || role === '') {{
        Swal.fire({{ icon: 'warning', title: 'Missing Role', text: 'Please select a role from the list.' }});
        return;
    }}




    const data = {{
        action: action,
        username: username,
        fullName: fullname,
        password: password,
        role: role,
        isActive: isActive
    }};

    if (window.chrome?.webview) {{
        window.chrome.webview.postMessage(JSON.stringify(data));
    }} else {{
        console.log('WebView2 not available', data);
    }}
}}
</script>
</body>
</html>
";

            // Initialize WebView2
            await webView21.EnsureCoreWebView2Async(null);

            // Attach event handler BEFORE loading content
            webView21.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            // Load HTML
            webView21.NavigateToString(htmlContent);
        }



        private async void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string json = e.TryGetWebMessageAsString();
                if (string.IsNullOrWhiteSpace(json)) return;

                // Deserialize JSON into UserModel
                var user = JsonConvert.DeserializeObject<UserModel>(json);

                if (user.action == "create")
                {

                    // Example: show message
                   
                    User us = new User
                    {
                        FullName = user.FullName,
                        Username = user.Username,
                        Role = user.Role,
                        IsActive = user.IsActive,
                        Password = PasswordHasher.HashPassword(user.Password)
                    };

                    db.Users.Add(us);
                    await db.SaveChangesAsync();

                    await webView21.CoreWebView2.ExecuteScriptAsync("showsuccess();");




                }

                if (user.action == "update")
                {

                    DialogResult result = MessageBox.Show(
       "Are you sure you want to update this User?",
       "Confirm update",
       MessageBoxButtons.YesNo,
       MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        var us = db.Users.Where(x => x.UserId == userId).FirstOrDefault();
                        if (us != null)

                        {
                            us.FullName = user.FullName;
                            us.Username = user.Username;
                            us.Role = user.Role;
                            us.IsActive = user.IsActive;
                            us.Password = PasswordHasher.HashPassword(user.Password);
                        }


                        await db.SaveChangesAsync();
                        userId = 0;
                        await webView21.CoreWebView2.ExecuteScriptAsync("showsuccess();");
                    }

                   



                }


                if (user.action == "delete")
                {
                    DialogResult result = MessageBox.Show(
        "Are you sure you want to delete this User?",
        "Confirm Delete",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {

                        var us = db.Users.Where(x => x.UserId == userId).FirstOrDefault();
                        if (us != null)

                        {
                            us.Deleted = true;
                            us.deletedby = GlobalUser.UserId;
                           

                            userId = 0;
                            await db.SaveChangesAsync();
                            
                          await webView21.CoreWebView2.ExecuteScriptAsync("showsuccess();");
                        }
                    }

                }


                dataGridView1.DataSource = db.Users.Where(x=>x.Deleted==false).Select(r => new { r.UserId, r.FullName, r.Username, r.Password, r.Role, r.IsActive }).ToList();

              
                // TODO: Insert into database here
                // InsertUser(user);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving message: " + ex.Message);
            }
        }

        // Model class
        public class UserModel
        {

          public  string action { get; set; }
            public string Username { get; set; }
            public string FullName { get; set; }
            public string Password { get; set; }
            public short Role { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
