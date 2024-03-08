using System.Security.Cryptography;
using System.Text;

namespace JoinEvents.Infrastructure;

public static class ExtensionMethods
{
    public static int IndexOf(this string[] items, string searched)
    {
        for(int i = 0; i < items.Length;i++)
            if (items[i] == searched)
                return i;
        return -1;
    }

    public static IEnumerable<string> GetUnnamedArguments(this string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.StartsWith('-'))
                i += 1;
            else
                yield return arg;
        }
    }
    public static string GetRequiredArgumentFor(this string[] args, string arg)
    {
        var index = args.IndexOf(arg);
        if (index != -1 && index + 1 < args.Length)
            return args[index+1];
        throw new Exception("No argument found:" + arg);
    }
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