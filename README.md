

## Demo Video
[Watch Video](https://dabansalahaddin.com/BankApp.mp4)](https://dabansalahaddin.com/BankApp.mp4)





# BankApp - User & Role Management System

BankApp is a Windows Forms application built with C# and WebView2, designed for managing users, roles, and authentication in a banking context. It provides a clean and modern interface for user management, including password security with hashing, role assignment, and interaction using WebView2 for forms.

---

## Getting Started

### Prerequisites

- Windows OS
- Visual Studio 2017 or later
- .NET Framework 4.7 or above
- SQL Server 2014 or above

### Database Setup

You have two options to set up the database:

#### 1. Restore from Backup

- The project includes a `.bak` backup file.
- Restore it in SQL Server 2014 or above using SQL Server Management Studio.
- The database contains a default admin user:

| Username   | Password |
|-----------|----------|
| Daban.cs  | 123      |

#### 2. Generate Database from Script

- Alternatively, you can generate your own database using the included `DBGGeneration.txt` script file.
- Open SQL Server Management Studio, create a new database, and run the script.

> **Note:** After restoring or generating the database, make sure to **update the connection string** in the application to point to your local SQL Server instance.

---

## Features

- **User Management**
  - Add, edit, and delete users.
  - Assign roles to users.
  - Enable/disable users.
- **Role Management**
  - Predefined roles in the database.
  - Select roles dynamically in forms.
- **Secure Passwords**
  - Uses `HashPassword`.
  - Passwords are never stored in plain text.
- **WebView2 Forms**
  - Modern and responsive UI for forms inside WinForms.
  - SweetAlert2 feedback for user input validation.
- **Customer Management**
  - Add, edit, and delete customers.
  - Soft delete support.
  - Customer info plus Map integration.
- **Accounts Management**
  - CRUD accounts
  - Opening balance and cached balance
  - IBAN generation
  - HMAC-SHA256 checksum for balance using a secret key
  - Once there is a transaction, the opening balance cannot be changed
- **Transaction Management**
  - Create transactions
  - View account balance (withdraw/deposit/transfer)

> All records are marked with who created or deleted them.

---

## How to Run

1. Open the project in Visual Studio.
2. Restore the database from backup or run the script file.
3. Update the connection string in `App.config` or directly in your code:

```xml
<connectionStrings>
  <add name="BankAppEntities" connectionString="Your_Connection_String_Here" providerName="System.Data.SqlClient" />
</connectionStrings>






