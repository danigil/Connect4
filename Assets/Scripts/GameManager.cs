using UnityEngine;
using MoonActive.Connect4;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /*
     * Disk Prefab List
     * Please add a Disk Prefab for each player in the game.
     * 
     * This should be done through the Unity UI, you can drag and drop Prefab objects into the list slots.
     */
    public List<GameObject> Prefabs = new List<GameObject>(2);
    private static List<Player> Players = new(2);
    //public GameObject diskAPrefab;
    //public GameObject diskBPrefab;
    public uint RowAmount;

    private GameObject board;
    private ConnectGameGrid ConnectGameGridObj;
    private uint ColumnAmount;
    
    private IDisk[,] GridDisks;
    private int[] ColumnCount;

    private bool isDropping = false;

    public static GameManager Instance;

    public enum GameType
    {
        COM,
        LocalPVP
    }


    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void BoardClick(int ColumnClicked)
    {
        Debug.Log($"Column Clicked: {ColumnClicked}");
        int EmptyCell = ColumnCount[ColumnClicked];
        if(EmptyCell >= RowAmount || isDropping){
            // Deny move
        }
        else{
            IDisk spawned = ConnectGameGridObj.Spawn(Prefabs[0].GetComponent<Disk>(), ColumnClicked, EmptyCell);
            isDropping = true;

            spawned.StoppedFalling += delegate () { isDropping = false; }; ;
            ColumnCount[ColumnClicked]++;

        }  
    }

    private interface Player
    {
        void act();
    }

    private class ComputerPlayer : Player
    {
        public void act()
        {
            throw new System.NotImplementedException();
        }
    }

    private class LocalPlayer : Player
    {
        public void act()
        {
            throw new System.NotImplementedException();
        }
    }

    private class TurnManager
    {
        public void NextTurn()
        {

        }
    }


    public void StartGame(GameType gt)
    {
        SceneManager.LoadScene("Connect4_Game");
        Debug.Log($"Starting a new game. GameType:{gt}");
        Players.Clear();
        if (gt == GameType.COM)
        {
            Players.Add(new LocalPlayer());
            Players.Add(new ComputerPlayer());
        }
        else if (gt == GameType.LocalPVP)
        {
            Players.Add(new LocalPlayer());
            Players.Add(new LocalPlayer());
        }

        SceneManager.sceneLoaded += InitGame;
    }

    private void InitGame(Scene scene, LoadSceneMode mode)
    {
        board = GameObject.FindGameObjectWithTag("Board");
        Debug.Log("Board found.");


        ConnectGameGridObj = board.GetComponent<ConnectGameGrid>();
        ConnectGameGridObj.ColumnClicked += BoardClick;
        Debug.Log("Click event listener added.");

        ColumnAmount = (uint)board.GetComponentInChildren<GridLayoutGroup>().constraintCount;
        Debug.Log($"Grid column size: {ColumnAmount}");

        GridDisks = new IDisk[RowAmount, ColumnAmount];
        ColumnCount = new int[ColumnAmount];
    }
}


