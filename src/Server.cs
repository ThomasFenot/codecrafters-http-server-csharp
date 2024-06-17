using System.Net;
using System.Net.Sockets;
using System.Text;

namespace codecrafters_http_server;

internal class Program
{
    private static async Task Main(string[] args)
    {
        const string CRLF = "\r\n";

        //Status Line
        const string OK_RESPONSE = $"HTTP/1.1 200 OK{CRLF}";
        const string NOT_FOUND_RESPONSE = $"HTTP/1.1 404 Not Found{CRLF}";
        const string CONTENT_TYPE_TEXT = $"Content-Type: text/plain{CRLF}";
        const string CONTENT_TYPE_FILE = $"Content-Type: application/octet-stream{CRLF}";

        // Uncomment this block to pass the first stage
        using TcpListener server = new TcpListener(IPAddress.Any, 4221);
        server.Start();

        while (true)
        {
            await HandleRequest().ConfigureAwait(false);
        }

        async Task HandleRequest()
        {
            string response;

            byte[] buffer = new byte[1024];

            string verb;
            string path;
            string httpVersion;

            using var socket = await server.AcceptSocketAsync();
            await socket.ReceiveAsync(buffer);
            string requestMessage = Encoding.UTF8.GetString(buffer);

            string[] request = requestMessage.Split(CRLF, StringSplitOptions.RemoveEmptyEntries);
            string[] cutRequest = request[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            verb = cutRequest[0];
            path = cutRequest[1];
            httpVersion = cutRequest[2];

            List<string> headers = [];

            for (int i = 1; i < request.Length - 1; i++)
                headers.Add(request[i]);

            List<KeyValuePair<string, string>> formatedHeaders = new List<KeyValuePair<string, string>>();

            headers.ForEach(header => formatedHeaders.Add(
                new KeyValuePair<string, string>(
                    header.Split(":")[0],
                    header.Split(":")[1]
                    )
                ));

            string userAgent = FindHeader(formatedHeaders, "User-Agent");

            if (path.Equals("/"))
                response = OK_RESPONSE + CRLF;
            else if (path.StartsWith("/echo/"))
            {
                var parameter = path.Split("/")[2];
                var contentLength = $"Content-Length: {parameter.Length}{CRLF}{CRLF}";
                response = OK_RESPONSE + CONTENT_TYPE_TEXT + contentLength + parameter;
            }
            else if (path.StartsWith("/files/"))
            {
                var parameter = path.Split("/", 3)[2]; //Here it should be a file name

                var filePath = Environment.GetCommandLineArgs()[2] + parameter;
                FileInfo file = new(filePath);

                if (!file.Exists)
                {
                    response = NOT_FOUND_RESPONSE + CRLF;
                    goto Send;
                }

                string contents = File.ReadAllText(filePath) + CRLF;

                var contentLength = $"Content-Length: {file.Length}{CRLF}{CRLF}";

                response = OK_RESPONSE + CONTENT_TYPE_FILE + contents.Length + contents;
            }
            else if (path.Equals("/user-agent"))
            {
                var contentLength = $"Content-Length: {userAgent.Length}{CRLF}{CRLF}";
                response = OK_RESPONSE + CONTENT_TYPE_TEXT + contentLength + userAgent;
            }
            else
                response = NOT_FOUND_RESPONSE + CRLF;

            Send:
            await socket.SendAsync(Encoding.ASCII.GetBytes(response));
        }

        string FindHeader(List<KeyValuePair<string, string>> formatedHeaders, string name) => formatedHeaders.FirstOrDefault(header => header.Key == name).Value?.Trim();
    }
}