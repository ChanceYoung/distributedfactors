using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static void Main()
    {
        int LBPORT = 6544;
        UdpClient Worker = new UdpClient();
        UTF8Encoding encoding = new UTF8Encoding();
        var data = Encoding.UTF8.GetBytes("RTR");
        Worker.Send(data, data.Length, "255.255.255.255", LBPORT);

        Console.WriteLine($"Started a new Worker");
        Console.WriteLine($"with Encoding UTF8");
        while (true)
        {
            IPEndPoint LBTicket = new IPEndPoint(0, 0);
            byte[] ticketData = Worker.Receive(ref LBTicket);

            Task.Run(() =>
            {
                string requestString = encoding.GetString(ticketData);
                string response;
                byte[] responseData;
                long request = long.Parse(requestString);
                List<long> answer = GetPrimeFactors(request);
                response = String.Join(',', answer.Select(p => p.ToString()));
                responseData = encoding.GetBytes(response);
                UdpClient toLoadBalancer = new UdpClient();
                toLoadBalancer.Send(responseData, responseData.Length, LBTicket);
            });

        }
    }

    static List<long> GetPrimeFactors(long product)
    {
        if (product <= 1)
        {
            throw new ArgumentException();
        }
        List<long> factors = new List<long>();
        // divide out factors in increasing order until we get to 1
        while (product > 1)
        {
            for (long i = 2; i <= product; i++)
            {
                if (product % i == 0)
                {
                    product /= i;
                    factors.Add(i);
                    break;
                }
            }
        }
        return factors;
    }

}