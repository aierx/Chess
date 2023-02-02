using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Net
{

    private Net() { }

    private static Net _instance;

    public static Net instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Net();
            }
            return _instance;
        }
    }

    public Socket client { get; set; }

    public int[,] chessState = new int[15, 15];

    public List<string> playerList = new List<string>();

    public bool close = false;

    public bool isPlaying = false;

    public string ipAddress;
    public bool isConnected = false;

    public bool isBlack = false;


    public void init()
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(new IPEndPoint(IPAddress.Parse("124.221.118.223"), 9999));
    }

    public SocketAsyncEventArgs initSendArgs(string data)
    {
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        byte[] vaule = Encoding.UTF8.GetBytes(data);
        args.SetBuffer(vaule, 0, vaule.Length);
        return args;
    }


    public void createGame()
    {
        Debug.Log("createGame");
        client.SendAsync(initSendArgs("createGame"));
        joinGame();
        isBlack = true;
    }

    public void joinGame()
    {
        List<string> result = new List<string>();
        client.SendAsync(initSendArgs("joinGame"));

        SocketAsyncEventArgs recv = new SocketAsyncEventArgs();

        recv.SetBuffer(new byte[1024], 0, 1024);
        recv.Completed += (c, o) =>
        {
            Socket socket = c as Socket;
            if (o.SocketError == SocketError.Success)
            {
                if(o.BytesTransferred == 0)
                {
                    socket.Close();
                    socket.Dispose();
                    close = true;
                }

                string tmp = Encoding.UTF8.GetString(o.Buffer).Trim('\0');
                if (tmp.StartsWith("#"))
                {
                    playerList.Clear();
                    string[] strList = tmp.Split("#");
                    foreach(string s in strList)
                    {
                        if(!s.Equals(""))
                            playerList.Add(s);
                    }
                }else if (tmp.Equals("joined"))
                {
                    Server.isStop = !Server.isStop;
                    if (isBlack)
                    {
                        isPlaying = true;
                    }
                    isConnected = true;
                }
                else{
                    string[] stringArr = tmp.Split("#");
                    isPlaying = true;
                    ipAddress = socket.RemoteEndPoint.ToString();
                    for (int i = 0; i < stringArr.Length; i++)
                    {
                        int x = i / 15;
                        int y = i % 15;
                        if (x < 15 && y < 15)
                        {
                            try
                            {
                                chessState[x, y] = int.Parse(stringArr[i]);
                            }
                            catch (Exception)
                            {
                                Debug.Log("aaaa");
                            }
                        }
                    }
                }
                o.SetBuffer(new byte[1024],0,1024);
                socket.ReceiveAsync(o);
            }
            else
            {
                socket.Close();
                socket.Dispose();
                close = true;
            }
        };
        client.ReceiveAsync(recv);
    }

    public void selectedGame(string str)
    {
        Debug.Log("selectedGame");
        client.SendAsync(initSendArgs(str));
    }


    public void sendGameData()
    {
        isPlaying = false;
        StringBuilder value = new StringBuilder();
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                value.Append(chessState[i, j]);
                value.Append("#");
            }
        }
        isPlaying = false;
        client.SendAsync(initSendArgs(value.ToString()));
    }


    public void reset()
    {
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                chessState[i, j] = 0;
            }
        }
        isPlaying = true;
    }

    public void closeMe()
    {
        client.Close();
        client.Dispose();
    }
}
