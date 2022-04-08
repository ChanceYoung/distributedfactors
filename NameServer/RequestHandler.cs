using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

class RequestHandler
{
    public List<IPEndPoint> availableDataNodes;
    public byte[] requestData;
    public IPEndPoint requester;
    private List<string> requestFormats = new List<string> { "^[a-z]+:[0-9]+$", "^[a-z]+$", "^[0-9]+$" };
    private IPEndPoint loadBalancer;
    public RequestHandler(List<IPEndPoint> AvailableDataNodes, byte[] RequestData, IPEndPoint Requester, IPEndPoint LoadBalancer)
    {
        availableDataNodes = AvailableDataNodes;
        requestData = RequestData;
        requester = Requester;
        loadBalancer = LoadBalancer;
    }

    private int getHashFromRequest(string request){
        string name = request.Split(':')[0];
        int hash = name.GetHashCode();
        return Math.Abs(hash)
    }

    public Func<int> buildTaskFunction(string request)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        for (int i = 0; i < requestFormats.Count; i++)
        {
            Regex check = new Regex(requestFormats[i]);
            if (check.IsMatch(request))
            {
                switch (i)
                {
                    case 0:
                        return () =>
                        {
                            byte[] responseData;
                            Console.WriteLine("...sending Query to DataNode");
                            getHashFromRequest(request);
                            Console.WriteLine("DataNode Index");
                            Console.WriteLine(Math.Abs(hash) % availableDataNodes.Count);
                            IPEndPoint dataNode = availableDataNodes[ % availableDataNodes.Count];
                            UdpClient dataNodeUDP = new UdpClient();
                            dataNodeUDP.Send(requestData, requestData.Length, dataNode);
                            responseData = dataNodeUDP.Receive(ref dataNode);
                            UdpClient toClient = new UdpClient();
                            toClient.Send(responseData, responseData.Length, requester);
                            return 0;
                        };

                    case 1:
                        return () =>
                        {

                            // SEND to requester
                            byte[] responseData;
                            Console.WriteLine("...sending Query to DataNode");
                            string name = request.Split(':')[0];
                            int hash = name.GetHashCode();
                            Console.WriteLine("DataNode Index");
                            Console.WriteLine(Math.Abs(hash) % availableDataNodes.Count);
                            IPEndPoint dataNode = availableDataNodes[Math.Abs(hash) % availableDataNodes.Count];
                            UdpClient dataNodeUDP = new UdpClient();
                            dataNodeUDP.Send(requestData, requestData.Length, dataNode);
                            responseData = dataNodeUDP.Receive(ref dataNode);
                            //send to LB as new UDPCLIENT
                            //RECIEVE from LB on LBUDPCLIENT                            
                            UdpClient LoadBalancerUDP = new UdpClient();
                            LoadBalancerUDP.Send(requestData, requestData.Length, dataNode);
                            responseData = LoadBalancerUDP.Receive(ref dataNode);
                            UdpClient toClient = new UdpClient();
                            toClient.Send(responseData, responseData.Length, requester);
                            return 1;
                        };

                    case 2:
                        return () =>
                        {
                            //send to LB as new UDPCLIENT
                            //RECIEVE from LB on LBUDPCLIENT
                            // SEND to requester
                            return 2;
                        };
                }

            }
        }
        return () => { return -1; };
    }
}