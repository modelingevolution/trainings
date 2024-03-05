using System.Security.Cryptography;
using System.Text;

namespace Send;

public static class ExtensionMethods
{
    public static Guid ToGuid(this string input)
    {
        // Check for null to avoid exceptions.
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentNullException(nameof(input));
        }

        // Use SHA256 to create the hash from the string.
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Convert the first 16 bytes of the hash to a Guid.
            byte[] guidBytes = new byte[16];
            Array.Copy(hashBytes, guidBytes, 16);

            return new Guid(guidBytes);
        }
    }
}