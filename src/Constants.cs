namespace codecrafters_http_server.src;

public class Constants
{
    public const string CRLF = "\r\n";

    //Status Line
    public const string OK_RESPONSE = $"HTTP/1.1 200 OK{CRLF}";
    public const string CREATED_RESPONSE = $"HTTP/1.1 201 Created{CRLF}";
    public const string NOT_FOUND_RESPONSE = $"HTTP/1.1 404 Not Found{CRLF}";
    public const string CONTENT_TYPE_TEXT = $"Content-Type: text/plain{CRLF}";
    public const string CONTENT_TYPE_FILE = $"Content-Type: application/octet-stream{CRLF}";
}
