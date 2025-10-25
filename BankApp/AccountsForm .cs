using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankApp
{
    public partial class AccountsForm : Form
    {
        public AccountsForm()
        {
            InitializeComponent();
        }

        BankAppEntities db = new BankAppEntities();
        private async void AccountsForm_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = db.Customers
    .Select(c => new 
    { 
        c.CustomerId, 
        Display = c.FirstName + " " + c.LastName 
    })
    .ToList();
            comboBox1.ValueMember = "CustomerId";
            comboBox1.DisplayMember = "Display";
            comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox1.AutoCompleteSource = AutoCompleteSource.ListItems;


            await webView21.EnsureCoreWebView2Async(null);

           
            webView21.NavigateToString(htmlContent);

           
            webView21.WebMessageReceived += WebView21_WebMessageReceived;

            LoadAccounts();

        }
        
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LoadAccounts();
            }
            catch { }
        }

        public void LoadAccounts()
        {
           
            var accountsQuery = db.Accounts
                .Where(x => x.CustomerId.ToString() == comboBox1.SelectedValue.ToString() && x.isdeleted==false)
                .ToList();

           
            var accounts = accountsQuery
                .Select(x => new
                {
                    x.AccountId,
                    Display = $"Account Name= {x.AccountName} | IBAN= {x.IBAN} |  Balance= {x.CachedBalance} | IsActiv={x.IsActive}" 
        })
                .ToList();

            listBox1.DataSource = accounts;
            listBox1.DisplayMember = "Display";
            listBox1.ValueMember = "AccountId";
        }


        private async void WebView21_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            var json = e.TryGetWebMessageAsString();
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            string action = data.action;

            if (action == "create")
            {
                // Convert OpeningBalance to string using invariant culture to avoid locale issues
                string balanceStr = data.OpeningBalance.ToString(System.Globalization.CultureInfo.InvariantCulture);

                // Compute checksum
                byte[] checksum = BalanceChecksumHelper.ComputeChecksum(decimal.Parse(balanceStr), HMACKey.key);
                string checksumHex = BalanceChecksumHelper.ToHexString(checksum);
                Account ac = new Account
                {
                    CustomerId = int.Parse(comboBox1.SelectedValue.ToString()),
                    AccountName = data.AccountName.ToString(),
                    OpeningBalance =decimal.Parse( data.OpeningBalance.ToString()),
                    CachedBalance = decimal.Parse(data.OpeningBalance.ToString()),
                    CreatedAt = DateTime.Now,
                    CreatedBy = GlobalUser.UserId,
                    IBAN = data.IBAN.ToString(),
                    BalanceChecksum = checksum
                };

                db.Accounts.Add(ac);
                await db.SaveChangesAsync();
                LoadAccounts();
                MessageBox.Show("Account created successfully.");

                await webView21.ExecuteScriptAsync("clearForm();");

            }

            if (action == "Generate")
            {


                string iban;
                bool exists;

                do
                {
                    iban = IbanHelper.GenerateIban();
                    // Check in your database if IBAN already exists
                    exists = db.Accounts.Any(a => a.IBAN == iban);
                }
                while (exists);

                // Now set it in WebView
                string script = $@"
    document.getElementById('iban').value = '{iban}';
";
                await webView21.ExecuteScriptAsync(script);
            }

            if (action == "update")
            {
                // Convert OpeningBalance to string using invariant culture to avoid locale issues
                string balanceStr = data.OpeningBalance.ToString(System.Globalization.CultureInfo.InvariantCulture);

                // Compute checksum
                byte[] checksum = BalanceChecksumHelper.ComputeChecksum(decimal.Parse(balanceStr), HMACKey.key);
                string checksumHex = BalanceChecksumHelper.ToHexString(checksum);

                int account_Id = int.Parse(data.Id.ToString());
                Account ac = db.Accounts.Where(x => x.AccountId == account_Id).FirstOrDefault();
                if(ac!=null)
                {
                   
                    ac.AccountName = data.AccountName.ToString();

                    if (!db.Transactions.AsNoTracking().Any())
                    {
                        ac.OpeningBalance = decimal.Parse(data.OpeningBalance.ToString());
                        ac.CachedBalance = decimal.Parse(data.OpeningBalance.ToString());
                        ac.BalanceChecksum = checksum;
                       }
                    else
                        MessageBox.Show("because there is existing transaction there won't be any change in opening balance");

                    ac.ModifiedAt = DateTime.Now;
                   ac.CreatedBy = GlobalUser.UserId;

                  
                }

                
                await db.SaveChangesAsync();
                LoadAccounts();
                MessageBox.Show("Account updated successfully.");

                await webView21.ExecuteScriptAsync("clearForm();");

            }

            if (action == "delete")
            {
                
                DialogResult result = MessageBox.Show(
       "Are you sure you want to update this User?",
       "Confirm update",
       MessageBoxButtons.YesNo,
       MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {

                    int account_Id = int.Parse(data.Id.ToString());
                    Account ac = db.Accounts.Where(x => x.AccountId == account_Id).FirstOrDefault();
                    if (ac != null)
                    {

                        ac.deletedby = GlobalUser.UserId;
                        ac.isdeleted = true;






                    }


                    await db.SaveChangesAsync();
                    LoadAccounts();
                    MessageBox.Show("Account Deleted successfully.");

                    await webView21.ExecuteScriptAsync("clearForm();");
                }

            }

            if (action == "lock")
            {

               
                int account_Id = int.Parse(data.Id.ToString());
                Account ac = db.Accounts.Where(x => x.AccountId == account_Id).FirstOrDefault();
                if (ac != null)
                {

                    ac.IsActive = !ac.IsActive;
                }


                await db.SaveChangesAsync();
                LoadAccounts();
                MessageBox.Show("Account updated successfully.");

                await webView21.ExecuteScriptAsync("clearForm();");

            }

            if (action == "validate")
            {


                int account_Id = int.Parse(data.Id.ToString());
                Account ac = db.Accounts.Where(x => x.AccountId == account_Id).FirstOrDefault();
                db.Entry(ac).Reload(); // Re-fetches from DB
                if (ac != null)
                {


                    string balanceStr = ac.CachedBalance.ToString(System.Globalization.CultureInfo.InvariantCulture);

                    // Compute checksum
                    byte[] checksum = BalanceChecksumHelper.ComputeChecksum(decimal.Parse(ac.CachedBalance.ToString()), HMACKey.key);
                    string acchecksumHex = BalanceChecksumHelper.ToHexString(checksum);


                    byte[] binaryValue = ac.BalanceChecksum; // e.g., varbinary column

                    // Convert to HEX string (uppercase, no dashes)
                    string hexValue = BitConverter.ToString(binaryValue).Replace("-", "");
                    string checksumToVerify = hexValue;

                    bool isValid = string.Equals(acchecksumHex, checksumToVerify, StringComparison.OrdinalIgnoreCase);
                    MessageBox.Show(isValid ? "✅ Checksum is valid!" : "❌ Checksum mismatch!");
                }



               

               // await webView21.ExecuteScriptAsync("clearForm();");

            }


        }






        string htmlContent = @"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<title>Accounts Management</title>
<style>
    body {
        font-family: 'Segoe UI', sans-serif;
        background: #f7f8fa;
        padding: 20px;
        color: #333;
    }
    h2 {
        text-align: center;
        color: #0066cc;
    }
    form {
        max-width: 450px;
        margin: 0 auto;
        background: white;
        padding: 20px;
        border-radius: 12px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }
    input {
        width: 100%;
        padding: 12px;
        margin: 6px 0;
        border: 1px solid #ccc;
        border-radius: 6px;
        box-sizing: border-box;
        font-size: 14px;
    }
    button {
        background-color: #0066cc;
        color: white;
        border: none;
        padding: 10px 16px;
        margin: 6px 4px 0 0;
        border-radius: 6px;
        cursor: pointer;
        transition: 0.2s;
        font-size: 14px;
    }
    button:hover {
        background-color: #004b99;
    }
    button:disabled {
        background-color: #ccc !important;
        color: #666 !important;
        cursor: not-allowed;
        opacity: 0.6;
    }
    .actions {
        text-align: center;
        margin-top: 12px;
    }
</style>
</head>
<body>
<h2>Accounts Management</h2>
<form id='accountForm'>
    <input type='hidden' id='accountId' value='' />
    <input type='text' id='iban' placeholder='IBAN' required />
    <button type='button' id='gen' onclick='Gen(""Generate"")'>Generate IBAN</button>
    <input type='text' id='accountName' placeholder='Account Name' />
    <input type='number' id='openingBalance' placeholder='Opening Balance' value='0.00' step='0.01' min='0' />

    <div class='actions'>
        <button type='button' id='create' onclick='sendData(""create"")'>Create</button>
 <button type='button' id='update' onclick='sendData(""update"")' disabled>Update</button>
        <button type='button' id='delete' onclick='sendData(""delete"")' disabled>Delete</button>
        <button type='button' id='lock' onclick='sendData(""lock"")' disabled>Lock/Unlock</button>
           <button type='button' onclick='clearForm()'>Clear</button>
 <button type='button' id='validate' onclick='sendData(""validate"")' disabled>Validate</button>
    </div>
</form>

<script>
function sendData(action) {
    const requiredFields = [
        { id: 'iban', name: 'IBAN' },
        { id: 'accountName', name: 'accountName' },
        { id: 'openingBalance', name: 'openingBalance' },
    ];

    for (let field of requiredFields) {
        const value = document.getElementById(field.id).value.trim();
        if (!value) {
            alert(field.name + ' is required.');
            document.getElementById(field.id).focus();
            return;
        }
    }

    const data = {
        action: action,
        Id: document.getElementById('accountId').value,
        IBAN: document.getElementById('iban').value.trim(),
        AccountName: document.getElementById('accountName').value.trim(),
        OpeningBalance: parseFloat(document.getElementById('openingBalance').value)
    };

    window.chrome.webview.postMessage(JSON.stringify(data));
}

function Gen(action) {
   

   
    const data = {
        action: action
       
    };

    window.chrome.webview.postMessage(JSON.stringify(data));
}

function clearForm() {
    document.getElementById('accountId').value = '';
    document.getElementById('iban').value = '';
    document.getElementById('accountName').value = '';
    document.getElementById('openingBalance').value = '0.00';
    document.getElementById('create').disabled = false;
 document.getElementById('gen').disabled = false;
    document.getElementById('delete').disabled = true;
    document.getElementById('lock').disabled = true;
   document.getElementById('lock').disabled = true;
   document.getElementById('update').disabled = true;
document.getElementById('validate').disabled = true;
}
</script>
</body>
</html>
";

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
          
        }

        private async void listBox1_DoubleClick(object sender, EventArgs e)
        {
          

            var account = db.Accounts.Where(x => x.AccountId.ToString() == listBox1.SelectedValue.ToString()).FirstOrDefault();
           
            if (account!=null)
            {
                
                string oldJson = Newtonsoft.Json.JsonConvert.SerializeObject(
     account,
     new Newtonsoft.Json.JsonSerializerSettings
     {
         ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
     });

                
                db.Entry(account).Reload();

               
                string newJson = Newtonsoft.Json.JsonConvert.SerializeObject(
     account,
     new Newtonsoft.Json.JsonSerializerSettings
     {
         ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
     });

                if (oldJson != newJson)
                {
                    MessageBox.Show("Account data has changed in the database! simply chanage the customer and back again to see update");
                }
               
                string script = $@"
        
 document.getElementById('update').disabled = false;
document.getElementById('delete').disabled = false;
 document.getElementById('lock').disabled = false;

document.getElementById('accountId').value ={account.AccountId};
document.getElementById('accountName').value = '{account.AccountName}';
document.getElementById('openingBalance').value = '{account.OpeningBalance}';
 document.getElementById('iban').value = '{account.IBAN}';
document.getElementById('create').disabled = true;
document.getElementById('gen').disabled = true;
document.getElementById('validate').disabled = false;
";

                await webView21.ExecuteScriptAsync(script);

            }

        }
    }
}
