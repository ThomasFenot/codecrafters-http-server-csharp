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
        const string CREATED_RESPONSE = $"HTTP/1.1 201 Created{CRLF}";
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
            string route;
            string httpVersion;

            using var socket = await server.AcceptSocketAsync();
            int bytesRead = await socket.ReceiveAsync(buffer);

            // Convert received bytes to a string, removing null bytes
            string requestMessage = Encoding.ASCII.GetString(buffer).Replace("\0", "");

            string[] request = requestMessage.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string[] cutRequest = request[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            verb = cutRequest[0];
            route = cutRequest[1];
            httpVersion = cutRequest[2];

            List<string> headers = [];

            for (int i = 1; i < request.Length - 1; i++)
            {
                if (!string.IsNullOrWhiteSpace(request[i]))
                    headers.Add(request[i]);
            }

            List<KeyValuePair<string, string>> formatedHeaders = [];

            headers.ForEach(header =>
            {
                Console.Error.WriteLine($"Header is : {header}");

                if (!string.IsNullOrWhiteSpace(header))
                {
                    formatedHeaders.Add(
                    new KeyValuePair<string, string>(
                        header.Split(":")[0],
                        header.Split(":")[1]
                        )
                    );
                }
            });

            string userAgent = FindHeader(formatedHeaders, "User-Agent");

            if (route.Equals("/"))
                response = OK_RESPONSE + CRLF;
            else if (route.StartsWith("/echo/"))
            {
                var parameter = route.Split("/")[2];
                var contentLength = $"Content-Length: {parameter.Length}{CRLF}{CRLF}";
                response = OK_RESPONSE + CONTENT_TYPE_TEXT + contentLength + parameter;
            }
            else if (route.StartsWith("/files/"))
            {
                var fileName = route.Split("/", 3)[2]; //Here it should be a file name
                var path = Environment.GetCommandLineArgs()[2];
                var filePath = path + fileName; 

                FileInfo file = new(filePath);
                DirectoryInfo directory = new(path);
                string contentLength;

                switch (verb)
                {
                    case "POST":

                        contentLength = FindHeader(formatedHeaders, "Content-Length");
                        var body = request[request.Length -1];

                        if (!directory.Exists)
                            directory.Create();

                        if (!file.Exists)
                        {
                            using (FileStream stream = file.Create())
                            {
                                AddText(stream, body);
                            }
                        }

                        response = CREATED_RESPONSE + CRLF;

                        break;
                    case "GET":

                        FileInfo fileWithPath = new(filePath);
                        if (!fileWithPath.Exists)
                        {
                            response = NOT_FOUND_RESPONSE + CRLF;
                            goto Send;
                        }

                        string contents = File.ReadAllText(filePath);

                        contentLength = $"Content-Length: {contents.Length}{CRLF}{CRLF}";

                        contents = contents.Trim() + CRLF;

                        response = OK_RESPONSE + CONTENT_TYPE_FILE + contentLength + contents;

                        break;
                    default:
                        response = NOT_FOUND_RESPONSE + CRLF;
                        goto Send;
                }
            }
            else if (route.Equals("/user-agent"))
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

    private static void AddText(FileStream fs, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }
}