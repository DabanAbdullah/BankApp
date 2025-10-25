using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
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
    public partial class TransactionForm : Form
    {
        public TransactionForm()
        {
            InitializeComponent();
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;

        }

        int selectedtransactionId;
        private async void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return;

            var row = dataGridView1.SelectedRows[0];

            selectedtransactionId = int.Parse(row.Cells["TransactionId"].Value.ToString());
           

            string script = $@"
(() => {{
    const init = () => {{
        document.getElementById('create').disabled = true;
        document.getElementById('delete').disabled = false;
        document.getElementById('IBAN').value = '{row.Cells["CounterpartyIBAN"].Value?.ToString().Replace("'", "\\'")}';
        document.getElementById('purpose').value = '{row.Cells["Purpose"].Value?.ToString().Replace("'", "\\'")}';
        document.getElementById('amount').value = '{row.Cells["amount"].Value?.ToString().Replace("'", "\\'")}';
 document.getElementById('balance').value = {currentbalance};
        const select = document.getElementById('transactionTypeId');
        if (!select) return;

        for (let i = 0; i < select.options.length; i++) {{
            if (select.options[i].text.trim().toLowerCase() === '{row.Cells["Name"].Value?.ToString().Replace("'", "\\'")}'.trim().toLowerCase()) {{
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





        private async void TransactionForm_Load(object sender, EventArgs e)
        {
            var transactionTypes = db.TransactionTypes.Where(x=>x.Name!= "Incoming")
    .Select(t => new { t.TransactionTypeId, t.Name })
    .ToList();

            string optionsHtml = string.Join("",
                transactionTypes.Select(t =>
                    $"<option value='{t.TransactionTypeId}'>{t.Name}</option>")
            );


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







            string htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<title>Transaction Entry</title>
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
<h2>New Transaction</h2>
<form id='transactionForm'>
   

   <input type='hidden' id='accountId' value='' />

<label>Current Balance </label>
    <input type='number' id='balance' disabled />

    <label>Amount *</label>
    <input type='number' id='amount' placeholder='Amount' step='0.01' min='0' required />

    <label>Purpose </label>
    <input type='text' id='purpose' placeholder='Purpose of transaction' required />

      <label>Counter Part IBAN (only if you send money)</label>
    <input type='text' id='IBAN' placeholder='IBAN of Counter Part' required />

    <label>Transaction Type *</label>
    <select id='transactionTypeId' required>
        <option value=''>Select Type</option>
        {optionsHtml}
    </select>

    <div style='text-align:center; margin-top:10px;'>
        <button type='button' id='create' onclick='submitTransaction(""create"")'>Add New</button>
       <button type='button' id='delete' onclick='submitTransaction(""delete"")' disabled>Delete</button>
 <button type='button' id='clear' onclick='clearform()'>Clear</button>
    </div>
</form>

<script>


function clearform(){{
document.getElementById('balance').value='';
document.getElementById('accountId').value='';
document.getElementById('IBAN').value='';
document.getElementById('amount').value='';
document.getElementById('purpose').value='';
  document.getElementById('create').disabled = false;
  document.getElementById('delete').disabled = true;
 document.getElementById('transactionTypeId').selectedIndex =0;
}}

function submitTransaction(action) {{
const accountId = document.getElementById('accountId').value;
    const iban = document.getElementById('IBAN').value.trim();
    const transactionType = document.getElementById('transactionTypeId').value.trim();
const amount = document.getElementById('amount').value.trim();
    // Check if Account ID is missing
    if (!accountId || accountId.trim() === '') {{
        Swal.fire({{
            icon: 'warning',
            title: 'Missing Account ID',
            text: 'Please select or provide an account before submitting.',
            confirmButtonColor: '#0066cc'
        }});
        return;
    }}

    // Check if IBAN is required for transfers
    if (transactionType.toLowerCase() === '3' && (iban === '' ||!iban) ) {{
        Swal.fire({{
            icon: 'warning',
            title: 'Missing IBAN',
            text: 'Please provide the counter party IBAN for a transfer transaction.',
            confirmButtonColor: '#0066cc'
        }});
        return;
    }}


    if (!amount || amount.trim() === '') {{
        Swal.fire({{
            icon: 'warning',
            title: 'Missing Amount',
            text: 'Please Enter Amount before submitting.',
            confirmButtonColor: '#0066cc'
        }});
        return;
    }}

 if (!transactionType || transactionType.trim() === '') {{
        Swal.fire({{
            icon: 'warning',
            title: 'Missing transactionType',
            text: 'Please select or transactionType before submitting.',
            confirmButtonColor: '#0066cc'
        }});
        return;
    }}

    const data = {{
        action: action,
        AccountID: document.getElementById('accountId').value,
        IBAN:document.getElementById('IBAN').value,
        Amount: document.getElementById('amount').value,
        Purpose: document.getElementById('purpose').value,
        TransactionTypeId: document.getElementById('transactionTypeId').value
    }};

    // send JSON to C#
    window.chrome.webview.postMessage(JSON.stringify(data));
}}

function showsuccess() {{
    clearform();
    Swal.fire({{
        icon: 'success',
        title: 'Success',
        text: 'Transaction completed successfully!',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    }});
}}
</script>
</body>
</html>
";


            await webView21.EnsureCoreWebView2Async(null);
            webView21.NavigateToString(htmlContent);


        }



        private async void webView21_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.WebMessageAsJson;

          
            string innerJson = JsonConvert.DeserializeObject<string>(json);

           
            TransactionMessage data = JsonConvert.DeserializeObject<TransactionMessage>(innerJson);

          
           
            string action = data.action;

            if (action == "create")
            {
                Transaction tr = new Transaction
                {
                    AccountId = int.Parse(data.AccountID),
                    Amount = int.Parse(data.Amount),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = GlobalUser.UserId,
                    TransactionDate=DateTime.Now,
                    TransactionTypeId = int.Parse(data.TransactionTypeId),
                    Purpose = data.Purpose
                };
                db.Transactions.Add(tr);
                int result = await db.SaveChangesAsync();

                if (result > 0)
                {
                    if (!string.IsNullOrWhiteSpace(data.IBAN))
                    {
                        var myiban = db.Accounts.Where(x => x.AccountId.ToString() == data.AccountID).FirstOrDefault();
                        Transactioncounterpart trcp = new Transactioncounterpart
                        {
                            CounterpartyIBAN = data.IBAN,
                            yourIBAN = myiban.IBAN,
                            TransactionId = tr.TransactionId,
                            TransactionType = int.Parse(data.TransactionTypeId)
                            
                        };
                        db.Transactioncounterparts.Add(trcp);
                        await db.SaveChangesAsync();
                        

                    }
                    await webView21.CoreWebView2.ExecuteScriptAsync("showsuccess();");
                    loadgrid(int.Parse(comboBox2.SelectedValue.ToString()));
                }
               

            }
            else if (action == "delete")
            {

                DialogResult result = MessageBox.Show(
         "Are you sure you want to delete this Transaction?",
         "Confirm Delete",
         MessageBoxButtons.YesNo,
         MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var transaction = db.Transactions.Where(x => x.TransactionId == selectedtransactionId).FirstOrDefault();
                    if (transaction != null)
                    {
                        transaction.IsDeleted = true;
                        transaction.DeletedByUserId = GlobalUser.UserId;
                       int TR= await db.SaveChangesAsync();
                        if (TR > 0)
                        {
                            await webView21.CoreWebView2.ExecuteScriptAsync("showsuccess();");
                            loadgrid(int.Parse(comboBox2.SelectedValue.ToString()));
                        }


                    }
                }
            }

            currentbalance = db.AccountBalances.AsNoTracking().Where(x => x.AccountId.ToString() == data.AccountID).FirstOrDefault().TotalBalance;
            var account = db.Accounts.Where(x => x.AccountId.ToString() == data.AccountID).FirstOrDefault();
            if (account != null)
            {
                account.CachedBalance= decimal.Parse( currentbalance.ToString());
               
                string balanceStr = account.CachedBalance.ToString(System.Globalization.CultureInfo.InvariantCulture);

               
                byte[] checksum = BalanceChecksumHelper.ComputeChecksum(decimal.Parse(balanceStr), HMACKey.key);
                string checksumHex = BalanceChecksumHelper.ToHexString(checksum);
                account.BalanceChecksum = checksum;

                await db.SaveChangesAsync();

            }



           
        }

        public class TransactionData
        {
            public int AccountID { get; set; }
          public string IBAN { get; set; }
            public decimal Amount { get; set; }
            public string Purpose { get; set; }
            public int TransactionTypeId { get; set; }
        }


        decimal? currentbalance;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
              

               

                var accounts = (from a in db.Accounts
                                join b in db.AccountBalances
                                    on a.AccountId equals b.AccountId into ab
                                from b in ab.DefaultIfEmpty()  // left join to include accounts without balance
                                where a.CustomerId.ToString() == comboBox1.SelectedValue.ToString()
                                      && a.IsActive 
                                select new ComboBoxItem
                                {
                                    Id = a.AccountId,
                                    Display = a.AccountName + " | Balance=" + b.TotalBalance
                                })
                .ToList();

               
              
                accounts.Insert(0, new ComboBoxItem
                {
                    Id = 0, 
                    Display = "Please select account"
                });

                comboBox2.DataSource = accounts;
                comboBox2.ValueMember = "Id";       
                comboBox2.DisplayMember = "Display";
                comboBox2.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox2.AutoCompleteSource = AutoCompleteSource.ListItems;
            }
            catch(Exception ex)
            {
               // MessageBox.Show(ex.Message);
            }
        }

      
        private async void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                



                var account = db.Accounts.Where(x => x.AccountId.ToString() == comboBox2.SelectedValue.ToString()).FirstOrDefault();
                if (account != null)
                {
                  
                    string script = $@"
    document.getElementById('accountId').value ={account.AccountId};

";
                    loadgrid(account.AccountId);
                    currentbalance = db.AccountBalances.Where(x => x.AccountId == account.AccountId).FirstOrDefault().TotalBalance;

                    await webView21.ExecuteScriptAsync(script);

                 

                }
                else
                {
                    dataGridView1.DataSource = null;
                }


            }
            catch
            {
                dataGridView1.DataSource = null;
            }
        }

        public void loadgrid(int accountid)
        {
          
          
            dataGridView1.DataSource = db.finalTransactions.AsNoTracking().Where(x => x.AccountId == accountid && x.IsDeleted==false).OrderByDescending(x=>x.TransactionDate).ToList();

        }
    }


    

    public class ComboBoxItem
    {
        public int Id { get; set; }
        public string Display { get; set; }
    }

    public class TransactionMessage
    {
        public string action { get; set; }
        public string AccountID { get; set; }
        public string IBAN { get; set; }
        public string Amount { get; set; }
        public string Purpose { get; set; }
        public string TransactionTypeId { get; set; }
    }
}
