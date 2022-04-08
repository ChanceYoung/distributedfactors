using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static Dictionary<string, string> cache = new Dictionary<string, string>();

    public static string checkCache(string requestString)
    {
        lock (cache)
        {
            if (cache.ContainsKey(requestString))
            {
                return cache[requestString];
            }
            else
            {
                return string.Empty;
            }
        }

    }
    static void Main()
    {
        int NSPORT = 6545;
        UdpClient requestsClient = new UdpClient(6544);
        UTF8Encoding encoding = new UTF8Encoding();
        var data = Encoding.UTF8.GetBytes("LBRTR");
        requestsClient.Send(data, data.Length, "255.255.255.255", NSPORT);

        Console.WriteLine($"Started the LoadBalancer");
        Console.WriteLine($"Listening for Requests on Port: {NSPORT}");

        //TODO:
        //take in a client request
        //take in a worker request
        //assign workers to workerqueue
        //assign request processing to a worker in the workerqueue
        List<IPEndPoint> workers = new List<IPEndPoint>();
        int clientRequestCount = 0;
        while (true)
        {
            IPEndPoint requester = new IPEndPoint(IPAddress.Any, 0);
            byte[] requestData = requestsClient.Receive(ref requester);


            string requestString = encoding.GetString(requestData);

            if (requestString == "RTR")
            {
                workers.Add(requester);
                Console.WriteLine($"Added Worker to list. Current worker list length: {workers.Count}");
            }
            else
            {
                Console.WriteLine("Processing Client Request...");
                clientRequestCount += 1;
                Task.Run(() =>
                    {
                        string response;
                        byte[] responseData;
                        string cacheCheck = checkCache(requestString);
                        if (cacheCheck != string.Empty)
                        {
                            Console.WriteLine("...retrieving from cache");
                            response = cacheCheck;
                            responseData = encoding.GetBytes(response);
                        }
                        else
                        {
                            Console.WriteLine("...sending to worker");
                            IPEndPoint worker = workers[clientRequestCount % workers.Count];
                            Console.WriteLine(worker);
                            UdpClient workerUDP = new UdpClient();
                            workerUDP.Send(requestData, requestData.Length, worker);
                            responseData = workerUDP.Receive(ref worker);
                            string workerResponse = encoding.GetString(responseData);
                            lock (cache)
                            {
                                cache.Add(requestString, workerResponse);
                            }
                        }

                        UdpClient toClient = new UdpClient();
                        toClient.Send(responseData, responseData.Length, requester);
                    });
            }
        }
    }

}