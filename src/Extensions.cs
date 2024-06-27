using codecrafters_http_server.Constants;
using System.Text;

namespace codecrafters_http_server.src;

public static class Extensions
{
    public static string? FindHeader(this List<KeyValuePair<string, string>> formatedHeaders, string name)
       => formatedHeaders.FirstOrDefault(header => header.Key == name).Value?.Trim();

    public static string FormatToHeader(this string header, string value)
      => $"{header + value + Controls.CRLF}";

    public static void AddText(this FileStream fs, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }

    public static List<string> ToList(this ValidEncodings encodings)
    {
        var toto = encodings.GetType().GetProperties();
        return new List<string>();
    }
}
