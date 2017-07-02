using System;
using System.Security.Cryptography;
using System.Text;

public static class StringExtend
{
    public static string ConvertToSHA512(this string value, Encoding encoding = null)
    {
        if (encoding == null) encoding = Encoding.Unicode;
        return BitConverter.ToString(new SHA512Managed().ComputeHash(encoding.GetBytes(value))).Replace("-", string.Empty);
    }

    public static string ConvertToSHA256(this string value, Encoding encoding = null)
    {
        if (encoding == null) encoding = Encoding.Unicode;
        return BitConverter.ToString(new SHA256Managed().ComputeHash(encoding.GetBytes(value))).Replace("-", string.Empty);
    }
}