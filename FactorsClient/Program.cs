using System.Net;
using System.Net.Sockets;
using System.Text;
class Program
{
    static void Main()
    {
        int PORT = 6545;
        UdpClient udpClient = new UdpClient();
        Console.WriteLine("Started new Client");
        while (true)
        {
            string request = Console.ReadLine();
            var data = Encoding.UTF8.GetBytes(request);
            udpClient.Send(data, data.Length, "255.255.255.255", PORT);
            var from = new IPEndPoint(0, 0);
            byte[] recvBuffer = udpClient.Receive(ref from);
            string message = Encoding.UTF8.GetString(recvBuffer);
            Console.WriteLine(message);
        }
    }
}

