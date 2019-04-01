using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpClinetProject
{
    class TcpClinetProject
    {
        static void Main(string[] args)
        {
            // serverInfo에 입력받은 IPv4와 포트번호를 넣음
            IPEndPoint serverInfo;
            Console.Write("서버주소 : ");
            string server = Console.ReadLine();
            Console.Write("포트번호 : ");
            string port = Console.ReadLine();
            serverInfo = new IPEndPoint(IPAddress.Parse(server), int.Parse(port));

            // 클라이언트 객채 생성
            TcpClient client = new TcpClient();
            //서버와 연결할 동안 대기
            while (!client.Connected)
            {
                try
                {
                    // serverInfo의 주소와 포트번호로 서버에 연결 요청
                    client.Connect(serverInfo.Address.ToString(), serverInfo.Port);
                }
                catch (Exception e)
                {
                    //연결을 못하였다면 0.1초 대기시킴
                    Console.WriteLine(e.Message);
                    Thread.Sleep(100);
                }
            }

            // 서버로 부터 메세지를 받을때
            Thread receive = new Thread(() => Receive(client));
            // 서버에게 메세지를 보낼때
            Thread send = new Thread(() => Send(client));

            receive.Start();
            send.Start();

            // 서버에게 메세지를 보내는 스레드가 끝난다면
            send.Join();
            // 서버로 부터 메세지를 받는 스레드 강제종료 
            receive.Abort();
            // 클라이언트 스트림 종료
            client.GetStream().Close();
            // 클라이언트 종료
            client.Close();
        }

        // 스레드를 돌리기 위한 함수를 정적 함수로 만듦
        static void Send(TcpClient _client)
        {
            TcpClient client = _client;
            NetworkStream netStream = client.GetStream();
            IPEndPoint ClientInfo = (IPEndPoint)client.Client.RemoteEndPoint;
            do
            {
                string s = Console.ReadLine();

                // exit를 입력받으면 루프에서 빠져나옴
                if (s.ToString() == "exit")
                    break;

                byte[] data = new byte[1024];
                // 입력한 문자열이 0이 아니라면
                if (s.Length != 0)
                {
                    data = Encoding.UTF8.GetBytes(s.ToString());
                    // 스트림으로 데이터를 보내고
                    netStream.Write(data, 0, data.Length);
                    // 스트림을 비운다.
                    netStream.Flush();
                }

            } while (true);
        }

        // 스레드를 돌리기 위한 함수를 정적 함수로 만듦
        static void Receive(TcpClient _client)
        {
            TcpClient client = _client;
            NetworkStream netStream = client.GetStream();

            try
            {
                do
                {
                    byte[] readData = new byte[1024];
                    int length;
                    // 서버로 부터 받은 데이터의 길이가 0이 아니라면
                    if ((length = netStream.Read(readData, 0, readData.Length)) != 0)
                    {
                        /// <summary>
                        /// 데이터를 문자열로 변환시킴 - - - 서버로 부터 받은 데이터의 길이 만큼
                        /// 만약 데이터를 저장한 readData를 할 경우 readData[1024]크기만큼 변환되기 때문에
                        /// 빈 공간이 생기게 된다.
                        /// </summary>
                        string msg = Encoding.UTF8.GetString(readData, 0, length);
                        Console.WriteLine("{0}", msg);
                    }
                } while (true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}