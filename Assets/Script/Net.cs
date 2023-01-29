using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Net
{
    public static void Main(string[] args)
    {
        var net = new Net();
        net.OnStartButtonClick();
    }
    public void OnStartButtonClick()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Any, 8080));
        socket.Listen(1024);
        while (true)
        {
            Debug.Log("�ȴ�����");
            Socket client = socket.Accept();
            Console.WriteLine("connected: " + client.RemoteEndPoint.ToString());
            Accept_Complete(client);
        }
    }

    void Accept_Complete(Socket client)
    {
        SocketAsyncEventArgs recv = new SocketAsyncEventArgs();
        SocketAsyncEventArgs send = new SocketAsyncEventArgs();

        ConnectInfo info = new ConnectInfo();
        info.tmpList = new ArrayList();
        info.SendArg = send;
        info.ReceiveArg = recv;

        byte[] sendBuffers = Encoding.UTF8.GetBytes("Helo world");
        send.SetBuffer(sendBuffers, 0, sendBuffers.Length);

        sendBuffers = new byte[1024];
        recv.SetBuffer(sendBuffers, 0, 1024);
        recv.UserToken = info;
        recv.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);

        client.SendAsync(send);
        client.ReceiveAsync(recv);
    }

    void Receive_Completed(object sender, SocketAsyncEventArgs e)
    {
        ConnectInfo info = e.UserToken as ConnectInfo;

        if (info == null) return;
        Socket client = sender as Socket;
        if (client == null) return;

        if (e.SocketError == SocketError.Success)
        {
            int rec = e.BytesTransferred;
            if (rec == 0)
            {
                Console.WriteLine("closed: " + client.RemoteEndPoint.ToString());

                client.Close();
                client.Dispose();

                info.ReceiveArg.Dispose();
                info.SendArg.Dispose();

                return;
            }

            byte[] datas = e.Buffer;
            if (client.Available > 0)
            {
                for (int i = 0; i < rec; i++)
                {
                    info.tmpList.Add(datas[i]);
                }
                Array.Clear(datas, 0, datas.Length);
                datas = new byte[client.Available];
                e.SetBuffer(datas, 0, datas.Length);
                client.ReceiveAsync(e);
            }
            else
            {
                if (info.tmpList.Count > 0)
                {
                    for (int i = 0; i < rec; i++)
                        info.tmpList.Add(datas[i]);
                    datas = info.tmpList.ToArray(typeof(byte)) as byte[]; ;
                    rec = datas.Length;
                }

                string msg = Encoding.UTF8.GetString(datas).Trim('\0');
                if (msg.Length > 10) msg = msg.Substring(0, 10) + "...";
                msg = string.Format("rec={0}\r\nmessage={1}", rec, msg);
                info.SendArg.SetBuffer(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
                client.SendAsync(info.SendArg);
                info.tmpList.Clear();
                if (e.Buffer.Length > 1024)
                {
                    datas = new byte[1024];
                    e.SetBuffer(datas, 0, datas.Length);
                }
                client.ReceiveAsync(e);
            }
        }
    }
}
