using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public static class IbanHelper
{
    private static readonly object _lock = new object();
    private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    /// <summary>
    /// Generates a unique, valid-format IBAN number (example: DE + 2 check digits + 18 digits)
    /// </summary>
    public static string GenerateIban(string countryCode = "DE", string bankCode = "50010517")
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            throw new ArgumentException("Country code must be 2 letters", nameof(countryCode));

        if (!Regex.IsMatch(countryCode, "^[A-Z]{2}$"))
            throw new ArgumentException("Country code must be uppercase letters A–Z", nameof(countryCode));

        lock (_lock)
        {
            // Generate a random 10-digit account number
            string accountNumber = GenerateRandomDigits(10);

            // Combine bank code + account number
            string bban = bankCode + accountNumber;

            // Compute checksum digits (mod 97 algorithm)
            string checkDigits = CalculateCheckDigits(countryCode, bban);

            // Final IBAN format
            string iban = $"{countryCode}{checkDigits}{bban}";

            // Optional: add spaces for readability
            return FormatIban(iban);
        }
    }

    private static string GenerateRandomDigits(int length)
    {
        var bytes = new byte[length];
        _rng.GetBytes(bytes);

        var sb = new StringBuilder(length);
        foreach (var b in bytes)
        {
            sb.Append((b % 10).ToString());
        }

        // Mix in current ticks for extra uniqueness
        string ticks = Math.Abs(DateTime.UtcNow.Ticks % 10000000000).ToString().PadLeft(10, '0');
        return (sb.ToString() + ticks).Substring(0, length);
    }

    private static string CalculateCheckDigits(string countryCode, string bban)
    {
        string countryDigits = "";
        foreach (char c in countryCode)
        {
            countryDigits += (c - 'A' + 10).ToString();
        }

        string rearranged = bban + countryDigits + "00";

        int mod97 = Mod97(rearranged);
        int checkDigits = 98 - mod97;

        return checkDigits.ToString("00");
    }

    private static int Mod97(string input)
    {
        string temp = input;
        while (temp.Length > 7)
        {
            var part = temp.Substring(0, 7);
            temp = (int.Parse(part) % 97).ToString() + temp.Substring(7);
        }
        return int.Parse(temp) % 97;
    }

    private static string FormatIban(string iban)
    {
        return Regex.Replace(iban, ".{4}", "$0 ").Trim();
    }
}

//class Program
//{
//    static void Main()
//    {
//        Console.WriteLine("Generating 5 unique IBAN numbers:\n");

//        for (int i = 0; i < 5; i++)
//        {
//            string iban = IbanHelper.GenerateIban();
//            Console.WriteLine(iban);
//        }

//        Console.WriteLine("\nPress any key to exit...");
//        Console.ReadKey();
//    }
//}
