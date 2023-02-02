using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Server : MonoBehaviour
{

    // 1 black -1 white

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
    enum turn { black, white };

    public Texture2D white;
    public Texture2D black;
    public Texture2D blackWin;
    public Texture2D whiteWin;

    int winner = 0;

    private Button btn_Reset;
    public static bool isStop = true;



    void Start()
    {
        // 跨模块调用方法
        //GameObject.Find("Canvas").SendMessage("isActive", false)

        chessPos = new Vector2[15, 15];

        btn_Reset = GameObject.Find("reset").GetComponent<Button>();
        btn_Reset.onClick.AddListener(() =>
        {
            isStop = !isStop;
        });

        Net.instance.init();
    }

    void Update()
    {
        UIContoller.instance.gameObject.SetActive(isStop);

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

        if (Net.instance.isPlaying && Input.GetMouseButtonDown(0))
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
                if (Net.instance.chessState[i, j] == 0)
                    Net.instance.chessState[i, j] = Net.instance.isBlack ? 1 : -1;
                Net.instance.sendGameData();
                int re = result();
                if (re == 1)
                {
                    winner = 1;
                    Net.instance.isPlaying = false;
                }
                else if (re == -1)
                {
                    winner = -1;
                    Net.instance.isPlaying = false;
                }
                else
                    winner = 0;
            }
        }
        if (Net.instance.isPlaying && Input.GetKeyDown(KeyCode.Space))
        {
            Net.instance.reset();
        }
    }

    void OnGUI()
    {
        GUI.depth = -1;
        GUIStyle bb = new GUIStyle();
        bb.normal.background = null;
        bb.normal.textColor = new Color(1, 0, 0); 
        bb.fontSize = 30; 
        if (!Net.instance.isPlaying)
            GUI.Label(new Rect(0, 40, 100, 100), "等待中",bb);
        else
            GUI.Label(new Rect(0, 40, 100, 100), "请落子",bb);

        if (!Net.instance.isConnected)
            GUI.Label(new Rect(0, 120, 100, 100), "等待连接",bb);

        else
            GUI.Label(new Rect(0, 120, 100, 100), "已连接: " + Net.instance.client.RemoteEndPoint.ToString(),bb);

        if (!isStop)
        {
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (Net.instance.chessState[i, j] == 0) continue;
                    float posX = chessPos[i, j].x - gridWidth / 2;
                    float posY = Screen.height - chessPos[i, j].y - gridHeight / 2;
                    Texture2D texture2D = Net.instance.chessState[i, j] == 1 ? black : white;
                    GUI.DrawTexture(new Rect(posX, posY, gridWidth, gridHeight), texture2D);

                }
            }
            if (winner == 1)
                GUI.DrawTexture(new Rect(Screen.width * 0.25f, Screen.height * 0.25f, Screen.width * 0.5f, Screen.height * 0.25f), blackWin);
            if (winner == -1)
                GUI.DrawTexture(new Rect(Screen.width * 0.25f, Screen.height * 0.25f, Screen.width * 0.5f, Screen.height * 0.25f), whiteWin);
        }



    }

    int result()
    {
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (Net.instance.chessState[i, j] == 0) continue;
                bool flag = Net.instance.chessState[i, j] == 1;
                if (front(i, j, flag) || below(i, j, flag) || leftOlique(i, j, flag) || rightOlique(i, j, flag))
                    return Net.instance.chessState[i, j];
            }
        }
        return 0;
    }

    bool front(int i, int j, bool isBlack)
    {
        if (j > 10) return false;
        int tmp = isBlack ? 1 : -1;
        return Net.instance.chessState[i, j] == tmp &&
                Net.instance.chessState[i, j + 1] == tmp &&
                Net.instance.chessState[i, j + 2] == tmp &&
                Net.instance.chessState[i, j + 3] == tmp &&
                Net.instance.chessState[i, j + 4] == tmp;
    }

    bool below(int i, int j, bool isBlack)
    {
        if (i > 10) return false;
        int tmp = isBlack ? 1 : -1;
        return Net.instance.chessState[i, j] == tmp &&
                Net.instance.chessState[i + 1, j] == tmp &&
                Net.instance.chessState[i + 2, j] == tmp &&
                Net.instance.chessState[i + 3, j] == tmp &&
                Net.instance.chessState[i + 4, j] == tmp;
    }

    bool leftOlique(int i, int j, bool isBlack)
    {
        if (i > 10 || j < 4) return false;
        int tmp = isBlack ? 1 : -1;
        return Net.instance.chessState[i, j] == tmp &&
                Net.instance.chessState[i + 1, j - 1] == tmp &&
                Net.instance.chessState[i + 2, j - 2] == tmp &&
                Net.instance.chessState[i + 3, j - 3] == tmp &&
                Net.instance.chessState[i + 4, j - 4] == tmp;
    }

    bool rightOlique(int i, int j, bool isBlack)
    {
        if (i > 10 || j > 10) return false;
        int tmp = isBlack ? 1 : -1;
        return Net.instance.chessState[i, j] == tmp &&
                Net.instance.chessState[i + 1, j + 1] == tmp &&
                Net.instance.chessState[i + 2, j + 2] == tmp &&
                Net.instance.chessState[i + 3, j + 3] == tmp &&
                Net.instance.chessState[i + 4, j + 4] == tmp;
    }

    public void OnDestroy()
    {
        Net.instance.closeMe();
    }
}
