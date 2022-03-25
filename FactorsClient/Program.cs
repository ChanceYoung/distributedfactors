using System.Net;
using System.Net.Sockets;
using System.Text;
class Program
{
    static void Main()
    {
        int PORT = 6544;
        UdpClient udpClient = new UdpClient();

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

