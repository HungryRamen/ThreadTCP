using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;


class ThreadedTcpSrvr
{
    //
    private TcpListener client;
    public ThreadedTcpSrvr()
    {
        client = new TcpListener(IPAddress.Any, 9050);
        client.Start();
        Console.WriteLine("서버 준비완료");
        while (true)
        {
            while (!client.Pending())
            {
                Thread.Sleep(1000);
            }
            ConnectionThread newconnection = new ConnectionThread();
            newconnection.threadListener = this.client;
            Thread newthread = new Thread(new
            ThreadStart(newconnection.HandleConnection));
            newthread.Start();
        }
    }
    public static void Main()
    {
        ThreadedTcpSrvr server = new ThreadedTcpSrvr();
    }
}
class ConnectionThread
{
    //연결된 클라이언트를 관리하는 리스트
    public static List<TcpClient> TCPClientsList = new List<TcpClient>();
    public TcpListener threadListener;
    public void HandleConnection()
    {
        int recv;
        byte[] data = new byte[1024];
        TcpClient client = threadListener.AcceptTcpClient();
        TCPClientsList.Add(client);
        StringBuilder inString = new StringBuilder();
        NetworkStream ns = client.GetStream();
        inString.AppendFormat("\n현재 {0}명 접속중", TCPClientsList.Count);
        inString.Append("\n------------------------------");
        foreach(TcpClient cli in TCPClientsList)
        {
            inString.AppendFormat("\n{0} {1}",(IPEndPoint)cli.Client.RemoteEndPoint,cli == client ? "나":"상대방");
        }
        inString.Append("\n------------------------------\n");
        WriteClient(inString.ToString(), client);
        //주소 정보를 저장
        IPEndPoint ClientInfo = (IPEndPoint)client.Client.RemoteEndPoint;
        inString.Clear(); 
        inString.AppendFormat("접속아이피 >> {0}\n새로운 유저가 입장하였습니다.: 현재 {1}명 접속중", ClientInfo, TCPClientsList.Count);
        AllWriteClient(inString.ToString(), client);
        Console.WriteLine(inString);
        try
        {
            while ((recv = ns.Read(data, 0, data.Length)) > 0)
            {
                StringBuilder s = new StringBuilder(ClientInfo.ToString());
                s.AppendFormat(" : {0}", Encoding.UTF8.GetString(data, 0, recv));
                Console.WriteLine(s);
                AllWriteClient(s.ToString(), client);
            }
            inString.Clear();
            ns.Close();
            TCPClientsList.Remove(client);
            inString.AppendFormat("퇴장아이피 >> {0}\n유저가 퇴장하였습니다.: 현재 {1}명 접속중", ClientInfo, TCPClientsList.Count);
            AllWriteClient(inString.ToString(), client);
            client.Close();
            Console.WriteLine(inString);

        }
        catch (Exception e)
        {
            inString.Clear();
            ns.Close();
            TCPClientsList.Remove(client);
            inString.AppendFormat("퇴장아이피 >> {0}\n유저가 퇴장하였습니다.: 현재 {1}명 접속중", ClientInfo, TCPClientsList.Count);
            AllWriteClient(inString.ToString(), client);
            client.Close();
            Console.WriteLine(inString);
        }
    }

    //자신을 제외한 접속한 모든 클라이언트에게 스트림데이터를 보낸다.
    public void AllWriteClient(string s,TcpClient client)
    {
        byte[] data = new byte[1024];
        data = Encoding.UTF8.GetBytes(s);
        foreach (TcpClient cli in TCPClientsList)
        {
            if (cli != client)
            {
                cli.GetStream().Write(data, 0, data.Length);
            }
        }
    }

    //자신에게만 보낸다.
    public void WriteClient(string s, TcpClient client)
    {
        byte[] data = new byte[1024];
        data = Encoding.UTF8.GetBytes(s);
        client.GetStream().Write(data, 0, data.Length);
    }
}