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
        Console.WriteLine($"with Encoding UTF8");
        Console.WriteLine($"Listening for Requests on Port: {requestsPort}");

        //TODO:
        //judge based on request if 
        // name:number
        // OR
        // name
        // OR
        // number
        List<IPEndPoint> datanodes = new List<IPEndPoint>();
        int clientRequestCount = 0;
        while (true)
        {
            IPEndPoint requester = new IPEndPoint(IPAddress.Any, 0);
            byte[] requestData = requestsClient.Receive(ref requester);
            string requestString = encoding.GetString(requestData);

            if (requestString == "RTR")
            {
                datanodes.Add(requester);
                Console.WriteLine($"Added Datanode to list. Current worker list length: {datanodes.Count}");
            }
            else
            {
                Console.WriteLine("Processing Client Request...");
                RequestHandler handler = new RequestHandler(datanodes, requestData, requester);
                Func<int> taskFunction = handler.buildTaskFunction(requestString);
                Task.Run(taskFunction);
                clientRequestCount += 1;
            }
        }
    }

}