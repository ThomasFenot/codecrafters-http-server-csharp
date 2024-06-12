using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
var okResponse = "HTTP/1.1 200 OK\r\n\r\n";
var notFoundResponse = "HTTP/1.1 404 Not Found\r\n\r\n";


TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var socket = server.AcceptSocket();
var acceptedSocket = socket.Accept();
byte[] buffer = new byte[] { };
int response = acceptedSocket.Receive(buffer);

Console.WriteLine($"This is the receive response = {response}");
socket.Send(Encoding.ASCII.GetBytes(okResponse));