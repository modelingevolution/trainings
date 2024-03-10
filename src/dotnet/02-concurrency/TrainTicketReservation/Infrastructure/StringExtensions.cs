using System.Security.Cryptography;
using System.Text;

namespace TrainTicketReservation.Infrastructure;

public static class StringExtensions
{
    public static Guid ToGuid(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return Guid.Empty;

        using var hash = MD5.Create();
        byte[] hashBytes = hash.ComputeHash(Encoding.Default.GetBytes(input));

        return new Guid(hashBytes);
    }
}