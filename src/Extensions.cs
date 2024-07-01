using codecrafters_http_server.Constants;
using System.IO.Compression;
using System.Text;

namespace codecrafters_http_server.src;

public static class Extensions
{
    /// <summary>
    /// Find the header by 'name' in a List of KeyValuePair
    /// </summary>
    /// <param name="formatedHeaders">The aleady formated headers</param>
    /// <param name="name">Name of the header to be found</param>
    /// <returns>A nullable string with the full header if it found one</returns>
    public static string? FindHeader(this List<KeyValuePair<string, string>> formatedHeaders, string name)
       => formatedHeaders.FirstOrDefault(header => header.Key == name).Value?.Trim();

    /// <summary>
    /// Format the current header with it's value (This is a bit overkill tbh but why not afterall)
    /// </summary>
    /// <param name="header">String representing the header which you want some more data added to it</param>
    /// <param name="value">String representing the value you want to add to the targeted header</param>
    /// <returns>A string representing a complete header with a CRLF at its end</returns>
    public static string FormatToHeader(this string header, string value)
      => $"{header + value + Controls.CRLF}";

    /// <summary>
    /// Add some text to a FileStream
    /// </summary>
    /// <param name="fs">Targeted FileStream where you want to add some content</param>
    /// <param name="value">the string you want to add to the FileStream</param>
    public static void AddText(this FileStream fs, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }

    /// <summary>
    /// Compress the string with Gzip
    /// </summary>
    /// <param name="str">String to be encoded</param>
    /// <returns>A memory stream representing the compressed string</returns>
    public static byte[] Compress(this string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);

        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
        {
            msi.CopyTo(gs);
        }

        return mso.ToArray();
    }
}
