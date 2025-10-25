using System;
using System.Security.Cryptography;
using System.Text;

public static class BalanceChecksumHelper
{
    /// <summary>
    /// Compute HMAC-SHA256 checksum for a balance using a secret key
    /// </summary>
    public static byte[] ComputeChecksum(decimal balance, string secretKey)
    {
        if (secretKey == null) throw new ArgumentNullException(nameof(secretKey));

        // Convert balance to string with fixed format
        string balanceStr = balance.ToString("F2"); // e.g., "1234.56"

        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
        byte[] balanceBytes = Encoding.UTF8.GetBytes(balanceStr);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            return hmac.ComputeHash(balanceBytes);
        }
    }

    /// <summary>
    /// Optional: convert checksum to hex string for storage/logging
    /// </summary>
    public static string ToHexString(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "");
    }
}

//class Program
//{
//    static void Main()
//    {
//        Console.WriteLine("=== Balance Checksum Demo ===\n");

//        // Ask user for balance
//        Console.Write("Enter account balance: ");
//        if (!decimal.TryParse(Console.ReadLine(), out decimal balance))
//        {
//            Console.WriteLine("Invalid balance input!");
//            return;
//        }

//        // Ask user for secret key
//        Console.Write("Enter secret key: ");
//        string secretKey = Console.ReadLine();
//        if (string.IsNullOrEmpty(secretKey))
//        {
//            Console.WriteLine("Secret key cannot be empty!");
//            return;
//        }

//        // Compute checksum
//        byte[] checksum = BalanceChecksumHelper.ComputeChecksum(balance, secretKey);
//        string checksumHex = BalanceChecksumHelper.ToHexString(checksum);

//        Console.WriteLine($"\nBalance: {balance:F2}");
//        Console.WriteLine($"Checksum (hex): {checksumHex}");

//        // Optional: verify checksum
//        Console.Write("\nVerify checksum? (y/n): ");
//        string input = Console.ReadLine();
//        if (input.Trim().ToLower() == "y")
//        {
//            Console.Write("Enter checksum to verify (hex): ");
//            string checksumToVerify = Console.ReadLine();

//            bool isValid = string.Equals(checksumHex, checksumToVerify, StringComparison.OrdinalIgnoreCase);
//            Console.WriteLine(isValid ? "✅ Checksum is valid!" : "❌ Checksum mismatch!");
//        }

//        Console.WriteLine("\nPress any key to exit...");
//        Console.ReadKey();
//    }
//}
