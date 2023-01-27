using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Server : MonoBehaviour
{

    Mutex mutex =  new Mutex();
    public GameObject LeftTop;
    public GameObject RightTop;
    public GameObject LeftBottom;
    public GameObject RightBottom;
    public Camera cam;

    Vector3 LTPos;
    Vector3 RTPos;
    Vector3 LBPos;

    Vector3 PointPos;
    float gridWidth = 1;
    float gridHeight = 1;
    Vector2[,] chessPos;
    int[,] chessState;
    enum turn { black, white };
    turn chessTurn;
    public Texture2D white;
    public Texture2D black;
    public Texture2D blackWin;
    public Texture2D whiteWin;
    int winner = 0;
    bool isPlaying = false;

    private Button btn_Create;
    private Button btn_Join;

    private InputField in_ipAddress;
    private string ipAddress;

    private Socket client;

    private bool isConnected = false;

    private bool isBlack = false;

    void Start()
    {
        chessPos = new Vector2[15, 15];
        chessState = new int[15, 15];
        chessTurn = turn.black;

        btn_Create = GameObject.Find("btn_Create").GetComponent<Button>();
        btn_Join = GameObject.Find("btn_Join").GetComponent<Button>();
        btn_Create.onClick.AddListener(createGame);
        btn_Join.onClick.AddListener(joinGame);
    }

    void joinGame()
    {
        in_ipAddress = GameObject.Find("ipAddress").GetComponent<InputField>();
        ipAddress = in_ipAddress.text;
        new Thread(() =>
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), 8080));
            SocketAsyncEventArgs recv = new SocketAsyncEventArgs();
            recv.SetBuffer(new byte[1024], 0, 1024);
            recv.Completed += new EventHandler<SocketAsyncEventArgs>(recvMessage);
            isPlaying = true;
            isConnected = true;
            isBlack = true;
            client.ReceiveAsync(recv);
        }).Start();
        GameObject.Find("Canvas").SendMessage("isActive", false);
    }

    void createGame()
    {
        new Thread(() =>
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 8080));
            socket.Listen(1024);

            Debug.Log("connecting");
            client = socket.Accept();
            isConnected = true;
            Debug.Log("connected: " + client.RemoteEndPoint.ToString());
            SocketAsyncEventArgs recv = new SocketAsyncEventArgs();

            byte[] sendBuffers = new byte[1024];
            recv.SetBuffer(sendBuffers, 0, 1024);
            recv.Completed += new EventHandler<SocketAsyncEventArgs>(recvMessage);
            client.ReceiveAsync(recv);
        }).Start();
        GameObject.Find("Canvas").SendMessage("isActive", false);
    }

    void recvMessage(object o, SocketAsyncEventArgs e)
    {
        client = o as Socket;
        if (e.SocketError == SocketError.Success)
        {
            int flag = e.BytesTransferred;
            if (flag == 0)
            {
                client.Close();
                client.Dispose();
            }
            byte[] buffer = e.Buffer;
            string v = Encoding.UTF8.GetString(buffer);
            string[] stringArr = v.Split("#");
            Debug.Log("recv!!!");
            isPlaying = true;
            for (int i = 0; i < stringArr.Length; i++)
            {
                int x = i / 15;
                int y = i % 15;
                if (x < 15 && y < 15)
                {
                    try
                    {
                        chessState[x, y] = int.Parse(stringArr[i]);
                    }catch (Exception)
                    {
                        Debug.Log("aaaa");
                    }
                }
            }
            Array.Clear(buffer, 0, buffer.Length);
            client.ReceiveAsync(e);
        }
    }

    void sendData()
    {
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
        client.Send(Encoding.UTF8.GetBytes(value.ToString()));
    }

    void Update()
    {
        LTPos = cam.WorldToScreenPoint(LeftTop.transform.position);
        RTPos = cam.WorldToScreenPoint(RightTop.transform.position);
        LBPos = cam.WorldToScreenPoint(LeftBottom.transform.position);

        gridWidth = (RTPos.x - LTPos.x) / 14;
        gridHeight = (LTPos.y - LBPos.y) / 14;

        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                chessPos[i, j] = new Vector2(LBPos.x + gridWidth * i, LBPos.y + gridHeight * (14 - j));
            }
        }
        if (isPlaying && Input.GetMouseButtonDown(0))
        {
            PointPos = Input.mousePosition;
            float x = PointPos.x;
            float y = PointPos.y;
            float x1 = LTPos.x;
            float y1 = LTPos.y;
            int i = Mathf.RoundToInt((x - x1) / gridWidth);
            int j = Mathf.RoundToInt((y1 - y) / gridHeight);
            if (!(i < 0 || j < 0 || i > 14 || j > 14))
            {
                if (chessState[i, j] == 0)
                    chessState[i, j] = isBlack ? 1 : -1;
                sendData();
                int re = result();
                if (re == 1)
                {
                    winner = 1;
                    isPlaying = false;
                }
                else if (re == -1)
                {
                    winner = -1;
                    isPlaying = false;
                }
            }
        }
        if (isPlaying && Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    chessState[i, j] = 0;
                }
            }
            isPlaying = true;
            chessTurn = turn.black;
            winner = 0;
        }
    }

    void OnGUI()
    {
        GUIStyle bb = new GUIStyle();
        bb.normal.background = null; //这是设置背景填充的
        bb.normal.textColor = new Color(1, 0, 0); //设置字体颜色的
        bb.fontSize = 30; //当然，这是字体颜色
        if (!isPlaying)
            GUI.Label(new Rect(0, 40, 100, 100), "等待中",bb);
        else
            GUI.Label(new Rect(0, 40, 100, 100), "赶紧落子",bb);

        if (!isConnected)
            GUI.Label(new Rect(0, 120, 100, 100), "未连接",bb);

        else
            GUI.Label(new Rect(0, 120, 100, 100), "已连接: " + client.RemoteEndPoint.ToString(),bb);

        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (chessState[i, j] == 0) continue;
                float posX = chessPos[i, j].x - gridWidth / 2;
                float posY = Screen.height - chessPos[i, j].y - gridHeight / 2;
                Texture2D texture2D = chessState[i, j] == 1 ? black : white;
                GUI.DrawTexture(new Rect(posX, posY, gridWidth, gridHeight), texture2D);

            }
        }
        if (winner == 1)
            GUI.DrawTexture(new Rect(Screen.width * 0.25f, Screen.height * 0.25f, Screen.width * 0.5f, Screen.height * 0.25f), blackWin);
        if (winner == -1)
            GUI.DrawTexture(new Rect(Screen.width * 0.25f, Screen.height * 0.25f, Screen.width * 0.5f, Screen.height * 0.25f), whiteWin);

    }

    int result()
    {
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (chessState[i, j] == 0) continue;
                bool flag = chessState[i, j] == 1;
                if (front(i, j, flag) || below(i, j, flag) || leftOlique(i, j, flag) || rightOlique(i, j, flag))
                    return chessState[i, j];
            }
        }
        return 0;
    }

    bool front(int i, int j, bool isBlack)
    {
        if (j > 10) return false;
        int tmp = isBlack ? 1 : -1;
        return chessState[i, j] == tmp &&
                chessState[i, j + 1] == tmp &&
                chessState[i, j + 2] == tmp &&
                chessState[i, j + 3] == tmp &&
                chessState[i, j + 4] == tmp;
    }

    bool below(int i, int j, bool isBlack)
    {
        if (i > 10) return false;
        int tmp = isBlack ? 1 : -1;
        return chessState[i, j] == tmp &&
                chessState[i + 1, j] == tmp &&
                chessState[i + 2, j] == tmp &&
                chessState[i + 3, j] == tmp &&
                chessState[i + 4, j] == tmp;
    }

    bool leftOlique(int i, int j, bool isBlack)
    {
        if (i > 10 || j < 4) return false;
        int tmp = isBlack ? 1 : -1;
        return chessState[i, j] == tmp &&
                chessState[i + 1, j - 1] == tmp &&
                chessState[i + 2, j - 2] == tmp &&
                chessState[i + 3, j - 3] == tmp &&
                chessState[i + 4, j - 4] == tmp;
    }

    bool rightOlique(int i, int j, bool isBlack)
    {
        if (i > 10 || j > 10) return false;
        int tmp = isBlack ? 1 : -1;
        return chessState[i, j] == tmp &&
                chessState[i + 1, j + 1] == tmp &&
                chessState[i + 2, j + 2] == tmp &&
                chessState[i + 3, j + 3] == tmp &&
                chessState[i + 4, j + 4] == tmp;
    }
}
