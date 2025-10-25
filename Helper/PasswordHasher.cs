using System;
using System.Security.Cryptography;

public static class PasswordHasher
{
    private const int SaltSize = 16; // 128 bit
    private const int HashSize = 32; // 256 bit
    private const int Iterations = 100_000; // Strong default

    // Hash password
    public static string HashPassword(string password)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));

        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        byte[] hash;
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
        {
            hash = pbkdf2.GetBytes(HashSize);
        }

        string saltB64 = Convert.ToBase64String(salt);
        string hashB64 = Convert.ToBase64String(hash);
        return $"{Iterations}.{saltB64}.{hashB64}";
    }

    // Verify password
    public static bool VerifyPassword(string password, string storedHash)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));
        if (storedHash == null) throw new ArgumentNullException(nameof(storedHash));

        var parts = storedHash.Split('.');
        if (parts.Length != 3) return false;

        if (!int.TryParse(parts[0], out int iterations)) return false;
        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] storedHashBytes = Convert.FromBase64String(parts[2]);

        byte[] computedHash;
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
        {
            computedHash = pbkdf2.GetBytes(storedHashBytes.Length);
        }

        return AreEqualConstantTime(storedHashBytes, computedHash);
    }

    private static bool AreEqualConstantTime(byte[] a, byte[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        int diff = 0;
        for (int i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }
        return diff == 0;
    }
}


class Program
{
    static void Main()
    {
        Console.Write("Enter password to hash: ");
        string password = Console.ReadLine();

        // Hash password
        string hashed = PasswordHasher.HashPassword(password);
        Console.WriteLine("\nHashed Password:");
        Console.WriteLine(hashed);

        // Verify password
        Console.Write("\nEnter password again to verify: ");
        string verifyPassword = Console.ReadLine();

        bool isMatch = PasswordHasher.VerifyPassword(verifyPassword, hashed);
        Console.WriteLine(isMatch ? "\n✅ Password verified successfully!" : "\n❌ Password verification failed!");

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}