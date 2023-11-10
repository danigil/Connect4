using UnityEngine;
using MoonActive.Connect4;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Threading;
using System;
using System.Linq;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    /*
     * Disk Prefab List
     * Please add a Disk Prefab for each player in the game.
     * 
     * This should be done through the Unity UI, you can drag and drop Prefab objects into the list slots.
     */
    public List<GameObject> Prefabs = new List<GameObject>(2);
    private static List<Disk> Disks = new List<Disk>(2);
    private static List<Player> Players = new(2);

    [SerializeField]
    public uint RowAmount = 6;
    private static uint RowAmountStatic = 0;

    public int N = 4;

    private GameObject board;
    private static ConnectGameGrid ConnectGameGridObj;
    private uint ColumnAmount;
    
    private int[,] GridDisks;
    private static int[] ColumnCount;

    private bool isDropping = false;

    private static TurnManager tm = new TurnManager();

    public static GameManager Instance;

    public enum GameType
    {
        COM,
        LocalPVP
    }


    void Awake()
    {
        RowAmountStatic = RowAmount;

        Disks.Clear();
        Disks.AddRange(Prefabs.Select(prefab =>  prefab.GetComponent<Disk>()).ToList());

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void BoardClick(int ColumnClicked)
    {
        Debug.Log($"Column Clicked: {ColumnClicked}");
        int EmptyCell = ColumnCount[ColumnClicked];
        if(EmptyCell >= RowAmountStatic || isDropping){
            // Deny move
        }
        else{
            IDisk spawned = ConnectGameGridObj.Spawn(Prefabs[0].GetComponent<Disk>(), ColumnClicked, EmptyCell);
            isDropping = true;

            spawned.StoppedFalling += delegate () { isDropping = false; }; ;
            ColumnCount[ColumnClicked]++;

        }  
    }

    private static bool IsValidMove(int ColumnClicked)
    {
        int FilledAmount = ColumnCount[ColumnClicked];

        Debug.Log($"Checking move validity: col {ColumnClicked}, FilledAmount {FilledAmount}, Row Amount {RowAmountStatic}");
        return FilledAmount < RowAmountStatic;
    }

    private static int RetRandCol()
    {
        var filteredIdxs = ColumnCount.Select((e, index) => index).Where(x => IsValidMove(x));
        Debug.Assert(filteredIdxs.Count() > 0);

        int RandIndex = new System.Random().Next(0, filteredIdxs.Count());
        return filteredIdxs.ElementAt(RandIndex);
    }

    private interface Player
    {
        Task<int> act();
    }

    private class ComputerPlayer : Player
    {
        public async Task<int> act()
        {
            return RetRandCol();
        }
    }

    private class LocalPlayer : Player
    {
        public async Task<int> act()
        {
            int ret = -1;
            //AutoResetEvent evt = new AutoResetEvent(false);
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Action<int> handler = (col) => {
                ret = col;
                //evt.Set();
                tcs.SetResult(true);
            };

            ConnectGameGridObj.ColumnClicked += handler;
            do
            {
                //evt.WaitOne();
                await tcs.Task;
                tcs = new TaskCompletionSource<bool>();
            } while (!IsValidMove(ret));
            ConnectGameGridObj.ColumnClicked -= handler;

            return ret;
        }
    }
    /*
    private bool CheckWinFlat(int headPrimary, int headSecondary, bool sub, char mode, int player)
    {
        /*
         * mode = 'v' / 'h' / 'd' - vertical, horizontal, diagonal
         */ /*
        bool flag = true;
        int target;
        int amount = mode == 'v' ? (int) RowAmount : (int) ColumnAmount;
        bool inBounds;
        if (sub)
        {
            target = headPrimary - N + 1;
            targetSecondary = 
            inBounds = target >= 0;
        }
        else
        {
            target = headPrimary + N - 1;
            inBounds = target < amount;
        }

        if (inBounds)
        {
            bool mismatch;
            for (int i = 1; i < N; i++)
            {
                if (row)
                    mismatch = GridDisks[headPrimary - i, headSecondary] != player;
                else
                    mismatch = GridDisks[headPrimari, headSecondary] != player;
            }
        }


    }
    */

    private bool CheckWinConditionVertical(int row, int col, int player)
    {
        // Check Vertical
        bool vertical = true;
        int targetRow = row - N + 1;
        if (targetRow >= 0)
        {
            for (int i = 1; i < N; i++)
                if (GridDisks[row - i, col] != player)
                {
                    vertical = false;
                    break;
                }
        }
        else
            vertical = false;

        return vertical;
    }

    private bool CheckWinConditionHead(int row, int col, int player)
    {
        /*
         * Check Win Condition, return -1 if win condition didn't happen yet - otherwise return winner index
         */

        int targetRow = row - N + 1;

        // Check Horizontal Left
        bool horizontalLeft = true;
        int targetCol = col + N - 1;

        if (targetCol < ColumnAmount)
        {
            for (int i = 0; i < N; i++)
                if (GridDisks[row, col+i] != player)
                {
                    horizontalLeft = false;
                    break;
                }
        }
        else
            horizontalLeft = false;

        if (horizontalLeft)
        {
            Debug.Log($"horizontalLeft Win Condition met: row {row}, col {col}");
            return horizontalLeft;
        }

        // Check Horizontal Right
        bool horizontalRight = true;
        targetCol = col - N + 1;

        if (targetCol >= 0)
        {
            for (int i = 0; i < N; i++)
                if (GridDisks[row, col - i] != player)
                {
                    horizontalRight = false;
                    break;
                }
        }
        else
            horizontalRight = false;

        if (horizontalRight)
        {
            Debug.Log($"horizontalRight Win Condition met: row {row}, col {col}");
            return horizontalRight;
        }

        // Check Diagonal Left
        bool diagonalLeft = true;
        targetRow = row - N + 1;
        targetCol = col - N + 1;
        if (targetRow >= 0 && targetCol >= 0)
        {
            for (int i = 0; i < N; i++)
                if (GridDisks[row - i, col -i] != player)
                {
                    diagonalLeft = false;
                    break;
                }
        }
        else
            diagonalLeft = false;

        if (diagonalLeft)
        {
            Debug.Log($"diagonalLeft Win Condition met: row {row}, col {col}");
            return diagonalLeft;
        }

        // Check Diagonal right
        bool diagonalRight = true;
        targetRow = row - N + 1;
        targetCol = col + N + 1;
        if (targetRow >= 0 && targetCol < ColumnAmount)
        {
            for (int i = 0; i < N; i++)
                if (GridDisks[row - i, col + i] != player)
                {
                    diagonalRight = false;
                    break;
                }
        }
        else
            diagonalRight = false;

        if (diagonalRight)
        {
            Debug.Log($"diagonalRight Win Condition met: row {row}, col {col}");
            return diagonalRight;
        }

        return false;

        //Debug.Log($"Checking win condition: v {vertical}, hl {horizontalLeft}, hr {horizontalRight}, dl {diagonalLeft}, dr {diagonalRight}");



        //return vertical || horizontalLeft || horizontalRight || diagonalLeft || diagonalRight;
    }

    private bool CheckWinCondition(int row, int col, int player)
    {
        bool vertical = CheckWinConditionVertical(row, col, player);
        if (vertical)
        {
            Debug.Log($"Vertical Win Condition met: row {row}, col {col}");
            return true;
        }

        int margin = (int) Math.Ceiling((double) N / 2);

        int MarginColPos = (int) Math.Min(col + margin, ColumnAmount - 1);
        int MarginColNeg = (int) Math.Max(col - margin, 0);

        int MarginRowPos = (int)Math.Min(row + margin, RowAmount - 1);
        int MarginRowNeg = (int)Math.Max(row - margin, 0);

        bool flag;

        Debug.Log($"Checking margin box for win condition: MarginRowNeg {MarginRowNeg}, MarginRowPos {MarginRowPos}, MarginColNeg {MarginColNeg}, MarginColPos {MarginColPos} ");

        for (int i= MarginRowPos; i > MarginRowNeg; i--)
        {
            for (int j = MarginColPos; j > MarginColNeg; j--)
            {
                if( CheckWinConditionHead(i, j, player))
                {
                    return true;
                }
            }
        }

        return false;

    }

    private bool CheckTieCodition()
    {
        bool AllColumnsFull = ColumnCount.Select(x => x == RowAmount).Aggregate((x, y) => x && y);
        Debug.Log($"Checking tie condition: all columns full? {AllColumnsFull}");

        return AllColumnsFull;
    }

    private class TurnManager
    {
        public int CurrPlayerIndex { get; set; }
        private int PlayerAmount = 0;
        public void Reset()
        {
            CurrPlayerIndex = 0;
            PlayerAmount = Players.Count;

            Debug.Assert(PlayerAmount > 0);
        }
        public async Task<int> NextTurn()
        {
            Debug.Assert(0 <= CurrPlayerIndex && CurrPlayerIndex < Players.Count);

            Player CurrPlayer = Players[CurrPlayerIndex];
            int CurrMove = await CurrPlayer.act();

            CurrPlayerIndex++;
            CurrPlayerIndex %= PlayerAmount;

            return CurrMove;
        }
    }


    public void StartGame(GameType gt)
    {
        SceneManager.LoadScene("Connect4_Game");
        Debug.Log($"Starting a new game. GameType:{gt}");
        Players.Clear();
        if (gt == GameType.COM)
        {
            Players.Add(new ComputerPlayer());
            Players.Add(new ComputerPlayer());
        }
        else if (gt == GameType.LocalPVP)
        {
            Players.Add(new LocalPlayer());
            Players.Add(new LocalPlayer());
        }

        Debug.Assert(Players.Count <= Prefabs.Count);

        SceneManager.sceneLoaded += InitGame;
    }

    private void InitGame(Scene scene, LoadSceneMode mode)
    {
        board = GameObject.FindGameObjectWithTag("Board");
        Debug.Log("Board found.");


        ConnectGameGridObj = board.GetComponent<ConnectGameGrid>();
        /*
        ConnectGameGridObj.ColumnClicked += BoardClick;
        Debug.Log("Click event listener added.");
        */

        ColumnAmount = (uint)board.GetComponentInChildren<GridLayoutGroup>().constraintCount;
        Debug.Log($"Grid column size: {ColumnAmount}");

        GridDisks = new int[RowAmountStatic, ColumnAmount];
        for (int i = 0; i < RowAmountStatic * ColumnAmount; i++) GridDisks[i % RowAmountStatic, i / RowAmountStatic] = -1;
        ColumnCount = new int[ColumnAmount];

        GameLoop();
        //Task.Run(() => GameLoop());

        //Thread _thread = new Thread(GameLoop);
        //_thread.Start();
    }

    private async void GameLoop()
    {
        Debug.Log($"Running Game Loop");

        tm.Reset();
        int CurrPlayer = 0;
        int CurrCol;

        int CurrRow;

        bool win, tie;

        do
        {
            CurrPlayer = tm.CurrPlayerIndex;

            CurrCol = await tm.NextTurn();
            CurrRow = ColumnCount[CurrCol]++;

            GridDisks[CurrRow, CurrCol] = CurrPlayer;
            
            Debug.Log($"Player {CurrPlayer+1} move: Col {CurrCol} Row {CurrRow}");
            
            IDisk spawned = ConnectGameGridObj.Spawn(Disks[CurrPlayer], CurrCol, CurrRow);

            Debug.Log($"Disk Spawned");

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            
            AutoResetEvent evt = new AutoResetEvent(false);
            Action handler = () => {
                //evt.Set();
                tcs.SetResult(true);
            };
            

            Debug.Log($"Waiting for disk to stop falling");
            
            spawned.StoppedFalling += handler;
            //evt.WaitOne();
            await tcs.Task;
            spawned.StoppedFalling -= handler;
            

            Debug.Log($"Disk stopped falling");
            

        } while (!(win = CheckWinCondition(CurrRow, CurrCol, CurrPlayer)) && !(tie = CheckTieCodition()));
        if (win)
        {
            Debug.Log($"Game Over: Player {CurrPlayer + 1} Won!");
        }
        else
        {
            Debug.Log($"Game Over: TIE");
        }
        
    }
}


