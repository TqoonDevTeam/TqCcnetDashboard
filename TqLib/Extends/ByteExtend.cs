using System.Text;

public static class ByteExtend
{
    public static string ConvertToHex(this byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < bytes.GetLength(0); i++)
        {
            sb.AppendFormat("{0:x2}", bytes[i]);
        }
        return sb.ToString();
    }
}