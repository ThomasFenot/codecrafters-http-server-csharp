using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

//Status Line
var okResponse = "HTTP/1.1 200 OK\r\n";
var notFoundResponse = "HTTP/1.1 404 Not Found\r\n";

var response = "";

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

await RouteLogic();

async Task RouteLogic()
{
    byte[] buffer = new byte[1024];

    var socket = await server.AcceptSocketAsync();
    await socket.ReceiveAsync(buffer);
    string requestMessage = Encoding.UTF8.GetString(buffer);

    string[] request = requestMessage.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
    List<string> headers = [];

    for (int i = 1; i < request.Length - 1; i++)
    {
        headers.Add(request[i]);
    }

    List<KeyValuePair<string, string>> formatedHeaders = new List<KeyValuePair<string, string>>();

    headers.ForEach(header => formatedHeaders.Add(
        new KeyValuePair<string, string>(
            header.Split(":")[0],
            header.Split(":")[1]
            )
        ));

    string[] cutResult = request[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);
    string userAgent = formatedHeaders.Where(header => header.Key == "User-Agent").Select(header => header.Value.Trim()).FirstOrDefault();

    if (cutResult[1].Equals("/"))
    {
        response = okResponse;
        await socket.SendAsync(Encoding.ASCII.GetBytes(response + "\r\n"));
    }
    else if (cutResult[1].StartsWith("/echo/"))
    {
        var parameter = cutResult[1].Split("/")[2];

        var contentType = "Content-Type: text/plain\r\n";
        var contentLength = $"Content-Length: {parameter.Length}\r\n\r\n";

        response = okResponse + contentType + contentLength + parameter;
        await socket.SendAsync(Encoding.ASCII.GetBytes(response));
    }
    else if (cutResult[1].Equals("/user-agent"))
    {
        var contentType = "Content-Type: text/plain\r\n";
        var contentLength = $"Content-Length: {userAgent.Length}\r\n\r\n";

        response = okResponse + contentType + contentLength + userAgent;

        await socket.SendAsync(Encoding.ASCII.GetBytes(response));
    }
    else
    {
        response = notFoundResponse;
        await socket.SendAsync(Encoding.ASCII.GetBytes(response + "\r\n"));
    }
}