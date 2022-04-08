using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

class Server
{
    static void Main()
    {
        int NSPORT = 6545;
        UdpClient DataNode = new UdpClient();
        UTF8Encoding encoding = new UTF8Encoding();
        var data = Encoding.UTF8.GetBytes("RTR");
        DataNode.Send(data, data.Length, "255.255.255.255", NSPORT);

        Dictionary<string, string> storedNameFavorites = new Dictionary<string, string>();

        Console.WriteLine($"Started a new DataNode");
        while (true)
        {
            IPEndPoint NSTicket = new IPEndPoint(0, 0);
            byte[] ticketData = DataNode.Receive(ref NSTicket);
            string requestString = encoding.GetString(ticketData);
            Regex requestFormat1 = new Regex("^[a-z]+:[0-9]+$");
            if (requestFormat1.IsMatch(requestString))
            {
                Console.WriteLine("Writing to dictionary...");
                Console.WriteLine(requestString);
                Task.Run(() =>
                {

                    string name = requestString.Split(":")[0];
                    string number = requestString.Split(":")[1];
                    lock (storedNameFavorites)
                    {
                        storedNameFavorites.Add(name, number);
                    }
                    UdpClient responseUDP = new UdpClient();
                    byte[] response = encoding.GetBytes("Written to Database");
                    responseUDP.Send(response, response.Length, NSTicket);
                });
            }
            else
            {

                Console.WriteLine("Reading from dictionary...");
                Console.WriteLine(requestString);
                Task.Run(() =>
                {
                    string value;
                    lock (storedNameFavorites)
                    {
                        storedNameFavorites.TryGetValue(requestString, out value);
                    }
                    byte[] responseData = encoding.GetBytes(value);
                    UdpClient responseUDP = new UdpClient();
                    responseUDP.Send(responseData, responseData.Length, NSTicket);
                });
            }

        }
    }

}
