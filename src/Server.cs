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

byte[] buffer = new byte[1024];

var socket = server.AcceptSocket();
socket.Receive(buffer);
string requestMessage = Encoding.UTF8.GetString(buffer);

string[] request = requestMessage.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
string[] cutResult = request[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);
string[] userAgent = request[8].Split(":", StringSplitOptions.RemoveEmptyEntries);

if (cutResult[1].Equals("/"))
{
    response = okResponse;
    socket.Send(Encoding.ASCII.GetBytes(response + "\r\n"));
}
else if (cutResult[1].StartsWith("/echo/"))
{
    var parameter = cutResult[1].Split("/")[2];

    var contentType = "Content-Type: text/plain\r\n";
    var contentLength = $"Content-Length: {parameter.Length}\r\n\r\n";

    response = okResponse + contentType + contentLength + parameter;

    socket.Send(Encoding.ASCII.GetBytes(response));
}
else if (cutResult[1].Equals("/user-agent"))
{
    var contentType = "Content-Type: text/plain\r\n";
    var contentLength = $"Content-Length: {userAgent[1].Length}\r\n\r\n";

    response = okResponse + contentType + contentLength + userAgent[2];

    socket.Send(Encoding.ASCII.GetBytes(response));
}
else
{
    response = notFoundResponse;
    socket.Send(Encoding.ASCII.GetBytes(response + "\r\n"));
}
