using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static void Main()
    {

        int requestsPort = 6545;
        UdpClient requestsClient = new UdpClient(requestsPort);
        UTF8Encoding encoding = new UTF8Encoding();
        Console.WriteLine($"Started the NameServer");
        Console.WriteLine($"Listening for Requests on Port: {requestsPort}");

        List<IPEndPoint> datanodes = new List<IPEndPoint>();
        IPEndPoint LoadBalancer = new IPEndPoint(0, 0);
        int clientRequestCount = 0;
        while (true)
        {
            IPEndPoint requester = new IPEndPoint(IPAddress.Any, 0);
            byte[] requestData = requestsClient.Receive(ref requester);
            string requestString = encoding.GetString(requestData);

            if (requestString == "RTR")
            {
                datanodes.Add(requester);
                Console.WriteLine($"Added Datanode to list. Current datanode list length: {datanodes.Count}");
            }
            else if (requestString == "LBRTR")
            {
                LoadBalancer = requester;
                Console.WriteLine($"Set LoadBalancer to {requester}.");
            }
            else
            {
                Console.WriteLine("Processing Client Request...");
                RequestHandler handler = new RequestHandler(datanodes, requestData, requester, LoadBalancer);
                Func<int> taskFunction = handler.buildTaskFunction(requestString);
                Task.Run(taskFunction);
                clientRequestCount += 1;
            }
        }
    }

}