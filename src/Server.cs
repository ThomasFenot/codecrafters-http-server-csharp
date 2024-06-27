using codecrafters_http_server.Constants;
using codecrafters_http_server.src;
using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string HttpResponseHeader = $"HTTP/1.1 ";
        string ContentTypeHeader = $"Content-Type: ";
        string ContentEncodingHeader = $"Content-Encoding: ";

        string OkResponse = HttpResponseHeader.FormatToHeader(StatusCodes.OK);
        string CreatedResponse = HttpResponseHeader.FormatToHeader(StatusCodes.Created);
        string NotFoundResponse = HttpResponseHeader.FormatToHeader(StatusCodes.NotFound);

        string TextContentType = ContentTypeHeader.FormatToHeader(ContentTypes.Text);
        string ApplicationContentType = ContentTypeHeader.FormatToHeader(ContentTypes.Application);

        string GzipEncoding = ContentEncodingHeader.FormatToHeader(ValidEncodings.Values[0]);

        // Uncomment this block to pass the first stage
        using TcpListener server = new(IPAddress.Any, 4221);
        server.Start();

        while (true)
            await HandleRequest().ConfigureAwait(false);

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

            string[] request = requestMessage.Split(new[] { Controls.CRLF}, StringSplitOptions.RemoveEmptyEntries);
            string[] cutRequest = request[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            verb = cutRequest[0];
            route = cutRequest[1];
            httpVersion = cutRequest[2];

            List<string> headers = [];

            for (int i = 1; i < request.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(request[i]))
                    headers.Add(request[i]);
            }

            List<KeyValuePair<string, string>> formatedHeaders = [];

            headers.ForEach(header =>
            {
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

            string? userAgent = formatedHeaders.FindHeader("User-Agent");
            string? contentLength = formatedHeaders.FindHeader("Content-Length");
            string? acceptEncoding = formatedHeaders.FindHeader("Accept-Encoding");

            if (route.Equals("/"))
                response = OkResponse + Controls.CRLF;
            else if (route.StartsWith("/echo/"))
            {
                var parameter = route.Split("/")[2] + Controls.CRLF;
                contentLength = $"Content-Length: {parameter.Length}{Controls.CRLF}{Controls.CRLF}";



                response = string.IsNullOrEmpty(acceptEncoding) ?
                    OkResponse + TextContentType + contentLength + parameter
                    :
                    ValidEncodings.Values.Contains(acceptEncoding) ?
                        OkResponse + TextContentType + GzipEncoding + contentLength + parameter
                        :
                        OkResponse + TextContentType + Controls.CRLF;

            }
            else if (route.StartsWith("/files/"))
            {
                var fileName = route.Split("/", 3)[2]; //Here it should be a file name
                var path = Environment.GetCommandLineArgs()[2];
                var filePath = path + fileName;

                FileInfo file = new(filePath);
                DirectoryInfo directory = new(path);

                switch (verb)
                {
                    case "POST":

                        var body = request[request.Length - 1];

                        if (!directory.Exists)
                            directory.Create();

                        if (!file.Exists)
                        {
                            using FileStream stream = file.Create();
                            stream.AddText(body);
                        }

                        response = CreatedResponse + Controls.CRLF;

                        break;
                    case "GET":

                        FileInfo fileWithPath = new(filePath);
                        if (!fileWithPath.Exists)
                        {
                            response = NotFoundResponse + Controls.CRLF;
                            goto Send;
                        }

                        string contents = File.ReadAllText(filePath);

                        contentLength = $"Content-Length: {contents.Length}{Controls.CRLF}{Controls.CRLF}";

                        contents = contents.Trim() + Controls.CRLF;

                        response = OkResponse + ApplicationContentType + contentLength + contents;

                        break;
                    default:
                        response = NotFoundResponse + Controls.CRLF;
                        goto Send;
                }
            }
            else if (route.Equals("/user-agent"))
            {
                contentLength = $"Content-Length: {userAgent.Length}{Controls.CRLF}{Controls.CRLF}";
                response = OkResponse + TextContentType + contentLength + userAgent;
            }
            else
                response = NotFoundResponse + Controls.CRLF;

            Send:
            await socket.SendAsync(Encoding.ASCII.GetBytes(response));
        }
    }
}