using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIContoller : MonoBehaviour
{

    Label title;
    Button createGame;
    Button refreshPlayer;
    Button resetGame;
    ListView roomList;
    UIDocument uIDocument;

    public static UIContoller instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        uIDocument = GetComponent<UIDocument>();
        var root = GetComponent<UIDocument>().rootVisualElement;
        title = root.Q<Label>("title");
        createGame = root.Q<Button>("createGame");
        refreshPlayer = root.Q<Button>("refreshPlayer");
        resetGame = root.Q<Button>("resetGame");
        roomList = root.Q<ListView>("roomList");

        createGame.clicked +=Net.instance.createGame;
        refreshPlayer.clicked += onRefresh;
        resetGame.clicked += Net.instance.reset;

        roomList.itemsSource = Net.instance.playerList;
        roomList.makeItem = () =>
        {
            Button button = new Button();
            return button;
        };

        roomList.bindItem = (v, i) =>
        {
            Button button = v as Button;
            button.text = Net.instance.playerList[i];
            button.clicked += () =>
            {
                Net.instance.selectedGame("selectedGame#"+Net.instance.playerList[i]);
            };
        };

    }

    public void onRefresh()
    {
        Net.instance.joinGame();
        roomList.RefreshItems();
    }
}
