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
    public partial class CustomerForm : Form
    {

        BankAppEntities db = new BankAppEntities();
        public CustomerForm()
        {
            GlobalUser.UserId = 1;
            InitializeComponent();
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            textBox1.SetPlaceholder("Search by FirstName,LastName, Phobne, Email");
        }

        public void loadgridview()
        {
            dataGridView1.DataSource = db.Customers.Where(x => x.deleted == false).ToList();
        }

        private async void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return;

            var row = dataGridView1.SelectedRows[0];

           
            string script = $@"
        document.getElementById('create').disabled = true;
 document.getElementById('update').disabled = false;
document.getElementById('delete').disabled = false;
        document.getElementById('firstName').value = '{row.Cells["FirstName"].Value}';
        document.getElementById('lastName').value = '{row.Cells["LastName"].Value}';
        document.getElementById('street').value = '{row.Cells["Street"].Value}';
        document.getElementById('house').value = '{row.Cells["HouseNumber"].Value}';
        document.getElementById('zip').value = '{row.Cells["ZipCode"].Value}';
        document.getElementById('city').value = '{row.Cells["City"].Value}';
        document.getElementById('phone').value = '{row.Cells["PhoneNumber"].Value}';
        document.getElementById('email').value = '{row.Cells["EmailAddress"].Value}';
document.getElementById('tax').value = '{row.Cells["TaxId"].Value}';
document.getElementById('customerId').value='{row.Cells["CustomerId"].Value}'
setMarker(document.getElementById('street').value, document.getElementById('city').value);
    ";

            await webView21.ExecuteScriptAsync(script);
        }

     

        private async void CustomerForm_Load(object sender, EventArgs e)
            {
                

                await webView21.EnsureCoreWebView2Async();
                webView21.NavigateToString(htmlContent);

                // Handle messages from HTML
                webView21.CoreWebView2.WebMessageReceived += WebView21_WebMessageReceived;

            loadgridview();
        }


       

            private async void WebView21_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
            {
                var json = e.TryGetWebMessageAsString();
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                string action = data.action;

                if (action == "create")
                {
                try
                {
                    Customer cust = new Customer();
                    cust.FirstName = data.firstName.ToString();
                    cust.LastName = data.lastName.ToString();
                    cust.Street = data.street.ToString();
                    cust.HouseNumber = data.house.ToString();
                    cust.ZipCode = data.zip.ToString();
                    cust.City = data.city.ToString();
                    cust.PhoneNumber = data.phone.ToString();
                    cust.EmailAddress = data.email.ToString();
                    cust.TaxId = data.tax.ToString();
                    cust.CreatedBy = GlobalUser.UserId;
                    cust.CreatedAt = DateTime.Now;
                    cust.deleted = false;
                    db.Customers.Add(cust);
                   
                    db.SaveChanges();
                    MessageBox.Show("Customer created successfully.");
                   
                    await webView21.ExecuteScriptAsync("clearForm();");
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
               
                else if (action == "delete")
                {

                DialogResult result = MessageBox.Show(
           "Are you sure you want to delete this customer?",
           "Confirm Delete",
           MessageBoxButtons.YesNo,
           MessageBoxIcon.Warning );

                if (result == DialogResult.Yes)
                {
                   
                    int Id = int.Parse(data.Id.ToString());
                    Customer cust = db.Customers.Where(x => x.CustomerId == Id).FirstOrDefault();
                    if (cust != null)
                    {
                        try
                        {
                            cust.deleted = true;
                            cust.deletedby = GlobalUser.UserId;
                            db.SaveChanges();
                            await webView21.ExecuteScriptAsync("clearForm();");
                          
                            string script = $@"
        document.getElementById('create').disabled = false;
 document.getElementById('update').disabled = true;
document.getElementById('delete').disabled = true;
";

                            await webView21.ExecuteScriptAsync(script);
                            MessageBox.Show("Customer deleted.");
                        }catch(Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }


                    
            }

            else if (action == "update")
            {

                DialogResult result = MessageBox.Show(
           "Are you sure you want to update this customer?",
           "Confirm update",
           MessageBoxButtons.YesNo,
           MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {

                    int Id = int.Parse(data.Id.ToString());
                    Customer cust = db.Customers.Where(x => x.CustomerId == Id).FirstOrDefault();
                    if (cust != null)
                    {
                        try
                        {
                            cust.FirstName = data.firstName.ToString();
                            cust.LastName = data.lastName.ToString();
                            cust.Street = data.street.ToString();
                            cust.HouseNumber = data.house.ToString();
                            cust.ZipCode = data.zip.ToString();
                            cust.City = data.city.ToString();
                            cust.PhoneNumber = data.phone.ToString();
                            cust.EmailAddress = data.email.ToString();
                            cust.TaxId = data.tax.ToString();
                            cust.CreatedBy = GlobalUser.UserId;
                            cust.ModifiedAt = DateTime.Now;
                            db.SaveChanges();
                            await webView21.ExecuteScriptAsync("clearForm();");
                           

                            string script = $@"
        document.getElementById('create').disabled = false;
 document.getElementById('update').disabled = true;
document.getElementById('delete').disabled = true;
";

                            await webView21.ExecuteScriptAsync(script);
                            MessageBox.Show("Customer Updated.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }



            }


            loadgridview();
        }









        string htmlContent = @"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8' />
<title>Customer Address Management</title>
<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
<style>
.form-grid {
    display: grid;
    grid-template-columns: 1fr 1fr; /* two equal columns */
    gap: 10px;
}
.form-group input {
    width: 100%;
    padding: 10px;
    border-radius: 6px;
    border: 1px solid #ccc;
    box-sizing: border-box;
}
    body {
        font-family: 'Segoe UI', sans-serif;
        background: #f7f8fa;
        padding: 20px;
        color: #333;
    }
    h2 { text-align: center; color: #0066cc; }
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
        padding: 10px;
        margin: 6px 0;
        border: 1px solid #ccc;
        border-radius: 6px;
        box-sizing: border-box;
    }
    button {
        background-color: #0066cc;
        color: white;
        border: none;
        padding: 10px 16px;
        margin-right: 8px;
        border-radius: 6px;
        cursor: pointer;
        transition: 0.2s;
    }
    button:hover { background-color: #004b99; }
    .actions { text-align: center; margin-top: 12px; }
    button:disabled { background-color: #ccc !important; color: #666 !important; cursor: not-allowed; opacity: 0.6; }
    #map { height: 300px; max-width: 450px; margin: 20px auto; border-radius: 12px; }
</style>
<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
</head>
<body>


<form id='customerForm'>
    <input type='hidden' id='customerId' value='' />

    <div class='form-grid'>
        <div class='form-group'>
            <input type='text' id='firstName' placeholder='First Name' required />
        </div>
        <div class='form-group'>
            <input type='text' id='lastName' placeholder='Last Name' required />
        </div>
        <div class='form-group'>
            <input type='text' id='street' placeholder='Street' />
        </div>
        <div class='form-group'>
            <input type='text' id='house' placeholder='House Number' />
        </div>
        <div class='form-group'>
            <input type='text' id='zip' placeholder='Zip Code' />
        </div>
        <div class='form-group'>
            <input type='text' id='city' placeholder='City' />
        </div>
        <div class='form-group'>
            <input type='text' id='phone' placeholder='Phone Number' />
        </div>
        <div class='form-group'>
            <input type='email' id='email' placeholder='Email Address' />
        </div>
        <div class='form-group'>
            <input type='text' id='tax' placeholder='Tax Id' />
        </div>
    </div>

    <div class='actions'>
        <button type='button' id='create' onclick='sendData(""create"")'>Create</button>
        <button type='button' id='update' onclick='sendData(""update"")' disabled>Update</button>
        <button type='button' id='delete' onclick='sendData(""delete"")' disabled>Delete</button>
        <button type='button' onclick='clearForm()'>Clear</button>
    </div>
</form>

<div id='map'></div>

<script>
// Send form data to WinForms
function sendData(action) {
    const requiredFields = [
        { id: 'firstName', name: 'First Name' },
        { id: 'lastName', name: 'Last Name' },
        { id: 'tax', name: 'Tax Id' },
        { id: 'phone', name: 'Phone Number' }
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
        Id: document.getElementById('customerId').value.trim(),
        firstName: document.getElementById('firstName').value.trim(),
        lastName: document.getElementById('lastName').value.trim(),
        street: document.getElementById('street').value.trim(),
        house: document.getElementById('house').value.trim(),
        zip: document.getElementById('zip').value.trim(),
        city: document.getElementById('city').value.trim(),
        phone: document.getElementById('phone').value.trim(),
        email: document.getElementById('email').value.trim(),
        tax: document.getElementById('tax').value.trim()
    };

    window.chrome.webview.postMessage(JSON.stringify(data));
}

// Clear form
function clearForm() {
    ['customerId','firstName','lastName','street','house','zip','city','phone','email','tax'].forEach(id => {
        document.getElementById(id).value = '';
    });
    document.getElementById('create').disabled = false;
}

// Initialize Leaflet map
var map = L.map('map').setView([51.505, -0.09], 13);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '&copy; OpenStreetMap contributors' }).addTo(map);

var marker;

// Click map to add marker and reverse geocode
map.on('click', function(e) {
    if (marker) map.removeLayer(marker);
    marker = L.marker(e.latlng).addTo(map);

    // Use Nominatim reverse geocoding
    fetch('https://nominatim.openstreetmap.org/reverse?lat=' + e.latlng.lat + '&lon=' + e.latlng.lng + '&format=json')
        .then(response => response.json())
        .then(data => {
            if (data.address) {
                if (data.address.road) document.getElementById('street').value = data.address.road;
                if (data.address.city) document.getElementById('city').value = data.address.city;
                else if (data.address.town) document.getElementById('city').value = data.address.town;
                else if (data.address.village) document.getElementById('city').value = data.address.village;
            }
        });
});



// Function to set a marker based on street and city
function setMarker(street, city) {
    if (!street && !city) return; // nothing to geocode

    const query = encodeURIComponent((street || "") + ', ' + (city || ""));
    
    // Use Nominatim API for geocoding
    fetch('https://nominatim.openstreetmap.org/search?q=' + query + '&format=json&limit=1')
        .then(response => response.json())
        .then(data => {
            if (data && data.length > 0) {
                const loc = data[0];

                // Remove previous marker if it exists
                if (marker) {
                    map.removeLayer(marker);
                }

                // Add new marker
                marker = L.marker([loc.lat, loc.lon]).addTo(map);

                // Move map to marker location
                map.setView([loc.lat, loc.lon], 16);
            }
        })
        .catch(err => console.error('Geocoding error:', err));
}
</script>
</body>
</html>";


        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; 
                SearchInGridView(textBox1.Text.Trim());
            }
        }

        private void SearchInGridView(string query)
        {
            if (string.IsNullOrEmpty(query)) return;

           
            dataGridView1.ClearSelection();

            bool found = false;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue; 

                
                if ((row.Cells["FirstName"].Value?.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (row.Cells["LastName"].Value?.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (row.Cells["PhoneNumber"].Value?.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (row.Cells["EmailAddress"].Value?.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (row.Cells["TaxId"].Value?.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    row.Selected = true;  
                    dataGridView1.FirstDisplayedScrollingRowIndex = row.Index; 
                    found = true;
                    break; 
                }
            }

            if (!found)
                MessageBox.Show("No matching record found.");
        }
    }


   
}
