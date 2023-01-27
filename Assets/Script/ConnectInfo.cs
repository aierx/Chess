using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;


class ConnectInfo
{
    public ArrayList tmpList { get; set; }
    public SocketAsyncEventArgs SendArg { get; set; }
    public SocketAsyncEventArgs ReceiveArg { get; set; }
}
