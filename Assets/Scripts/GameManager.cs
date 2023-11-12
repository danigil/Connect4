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

    public int N = 4;

    private GameObject board;
    private static ConnectGameGrid ConnectGameGridObj;
    private uint ColumnAmount;


    private TurnManager tm;
    private Manager m;

    private UIManager uim;
    private AudioManager am;

    public static GameManager Instance;

    public enum GameType
    {
        COM,
        LocalPVP
    }

    public enum AIDifficulty
    {
        Random,
        SemiRandom
    }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);


        Disks.Clear();
        Disks.AddRange(Prefabs.Select(prefab =>  prefab.GetComponent<Disk>()).ToList());

        am = gameObject.GetComponent<AudioManager>();
    }


    public interface Player
    {
        Task<int> act(Manager m);
    }

    public class ComputerPlayer : Player
    {
        int idx;
        AIDifficulty diff;

        public ComputerPlayer(int idx)
        {
            this.idx = idx;
            this.diff = AIDifficulty.Random;
        }

        public ComputerPlayer(int idx, AIDifficulty diff)
        {
            this.idx = idx;
            this.diff = diff;
        }

        public Task<int> act(Manager m)
        {
            switch (diff)
            {
                case (AIDifficulty.Random):
                    return Task.FromResult(m.RetRandCol());
                case (AIDifficulty.SemiRandom):
                    IEnumerable<int> ValidMoves = m.RetValidMoves();
                    foreach (int move in ValidMoves)
                    {
                        if (m.CheckWinCondition(m.RetRow(move), move, idx))
                            return Task.FromResult(move);
                    }
                    return Task.FromResult(m.RetRandCol());
                default:
                    return Task.FromResult(m.RetRandCol());

            }

        }
    }

    public class LocalPlayer : Player
    {
        public async Task<int> act(Manager m)
        {
            int ret = -1;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Action<int> handler = (col) => {
                ret = col;
                tcs.SetResult(true);
            };

            ConnectGameGridObj.ColumnClicked += handler;
            do
            {
                await tcs.Task;
                tcs = new TaskCompletionSource<bool>();
            } while (!m.IsValidMove(ret));
            ConnectGameGridObj.ColumnClicked -= handler;

            return ret;
        }
    }

    private class TurnManager
    {
        private List<Player> Players = new List<Player>();

        public TurnManager(List<Player> Players)
        {
            this.Players.Clear();
            this.Players.AddRange(Players);
        }

        public int CurrPlayerIndex { get; set; }
        private int PlayerAmount = 0;
        public void Reset()
        {
            CurrPlayerIndex = 0;
            PlayerAmount = Players.Count;

            Debug.Assert(PlayerAmount > 0);
        }
        public async Task<int> NextTurn(Manager m)
        {
            Debug.Assert(0 <= CurrPlayerIndex && CurrPlayerIndex < Players.Count);

            Player CurrPlayer = Players[CurrPlayerIndex];
            int CurrMove = await CurrPlayer.act(m);

            CurrPlayerIndex++;
            CurrPlayerIndex %= PlayerAmount;

            return CurrMove;
        }
    }

    public void StartGame(GameType gt)
    {
        SceneManager.LoadScene("Connect4_Game", LoadSceneMode.Single);
        Debug.Log($"Starting a new game. GameType:{gt}");
        Players.Clear();
        if (gt == GameType.COM)
        {
            Players.Add(new ComputerPlayer(0, AIDifficulty.SemiRandom));
            Players.Add(new ComputerPlayer(1, AIDifficulty.SemiRandom));
            //Players.Add(new ComputerPlayer(2, AIDifficulty.SemiRandom));
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
        SceneManager.sceneLoaded -= InitGame;

        if (uim == null)
            uim = gameObject.AddComponent<UIManager>();

        board = GameObject.FindGameObjectWithTag("Board");
        Debug.Log("Board found.");


        ConnectGameGridObj = board.GetComponent<ConnectGameGrid>();

        ColumnAmount = (uint)board.GetComponentInChildren<GridLayoutGroup>().constraintCount;
        Debug.Log($"Grid column size: {ColumnAmount}");

        m = new Manager(N, (int)RowAmount, (int)ColumnAmount);
        tm = new TurnManager(Players);

        GameLoop();
    }

    private async void GameLoop()
    {
        Debug.Log($"Running Game Loop");

        tm.Reset();
        int CurrPlayer = 0;
        int CurrMove;
        int CurrRow;

        bool win, tie;

        do
        {
            CurrPlayer = tm.CurrPlayerIndex;
            CurrMove = await tm.NextTurn(m);

            CurrRow = m.PerformMove(CurrMove, CurrPlayer);
            
            Debug.Log($"Player {CurrPlayer+1} move: Col {CurrMove} Row {CurrRow}");
            
            IDisk spawned = ConnectGameGridObj.Spawn(Disks[CurrPlayer], CurrMove, CurrRow);

            Debug.Log($"Disk Spawned");

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            
            AutoResetEvent evt = new AutoResetEvent(false);
            Action handler = () => {
                tcs.SetResult(true);
                am.PlaySFX("drop");
            };
            

            Debug.Log($"Waiting for disk to stop falling");
            
            spawned.StoppedFalling += handler;
            await tcs.Task;
            spawned.StoppedFalling -= handler;
            

            Debug.Log($"Disk stopped falling");
            

        } while (!(win = m.CheckWinCondition(CurrRow, CurrMove, CurrPlayer)) && !(tie = m.CheckTieCodition()));
        if (win)
        {
            uim.DisplayGameOver(CurrPlayer);
            Debug.Log($"Game Over: Player {CurrPlayer + 1} Won!");
        }
        else
        {
            uim.DisplayGameOver(-1);
            Debug.Log($"Game Over: TIE");
        }

        Cleanup();
    }

    private void Cleanup()
    {
        Destroy(uim);
        uim = null;
    }
}



public class Manager
{
    private int RowAmount, ColumnAmount;

    private int N;
    public int[,] GridDisks { get; }
    private int[] ColumnCount;

    public Manager(int N, int RowAmount, int ColumnAmount)
    {
        this.N = N;
        this.RowAmount = RowAmount;
        this.ColumnAmount = ColumnAmount;

        GridDisks = new int[RowAmount, ColumnAmount];
        ColumnCount = new int[ColumnAmount];

        this.Reset();
    }

    public void Reset()
    {
        for (int i = 0; i < RowAmount * ColumnAmount; i++) GridDisks[i % RowAmount, i / RowAmount] = -1;
        for (int i = 0; i < ColumnAmount; i++) ColumnCount[i] = 0;
    }

    public int RetRow(int col)
    {
        return ColumnCount[col];
    }

    public bool IsValidMove(int ColumnClicked)
    {
        int FilledAmount = ColumnCount[ColumnClicked];

        Debug.Log($"Checking move validity: col {ColumnClicked}, FilledAmount {FilledAmount}, Row Amount {RowAmount}");
        return FilledAmount < RowAmount;
    }

    public IEnumerable<int> RetValidMoves()
    {
        return ColumnCount.Select((e, index) => index).Where(x => IsValidMove(x));
    }

    public int RetRandCol()
    {
        var ValidMoves = RetValidMoves();
        Debug.Assert(ValidMoves.Count() > 0);

        int RandIndex = new System.Random().Next(0, ValidMoves.Count());
        return ValidMoves.ElementAt(RandIndex);
    }

    public bool CheckWinConditionVertical(int row, int col, int player)
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

    public bool CheckWinConditionHorizontal(int row, int col, int player, bool left)
    {
        bool horizontal = true;
        int targetCol = (left) ? col + N - 1 : col - N + 1;

        if ((left) ? targetCol < ColumnAmount : targetCol>=0)
        {
            for (int i = 0; i < N; i++)
                if (GridDisks[row, (left) ? col + i : col - i] != player)
                {
                    horizontal = false;
                    break;
                }
        }
        else
            horizontal = false;

        return horizontal;
    }

    public bool CheckWinConditionDiagonal(int row, int col, int player, bool left)
    {
        int idx;
        bool diagonal = true;
        int targetRow = row - N + 1 ;

        int targetCol = (left) ? col + N - 1 : col - N + 1;
        bool InBounds = (left) ? targetCol < ColumnAmount : targetCol >= 0;

        if (targetRow >= 0 && InBounds)
        {
            for (int i = 0; i < N; i++) {
                idx = (left) ? col + i : col - i;
                if (GridDisks[row - i, idx] != player)
                {
                    diagonal = false;
                    break;
                }
            }
        }
        else
            diagonal = false;

        return diagonal;
    }

    public bool CheckWinConditionHead(int row, int col, int player)
    {
        /*
         * Check Win Condition, return -1 if win condition didn't happen yet - otherwise return winner index
         */

        if (CheckWinConditionHorizontal(row, col, player, true))
        {
            Debug.Log($"horizontalLeft Win Condition met: row {row}, col {col}");
            return true;
        }


        if (CheckWinConditionHorizontal(row, col, player, false))
        {
            Debug.Log($"horizontalRight Win Condition met: row {row}, col {col}");
            return true;
        }

        
        if (CheckWinConditionDiagonal(row, col, player, true))
        {
            Debug.Log($"diagonalLeft Win Condition met: row {row}, col {col}");
            return true;
        }
        
        
        if (CheckWinConditionDiagonal(row, col, player, false))
        {
            Debug.Log($"diagonalRight Win Condition met: row {row}, col {col}");
            return true;
        }

        return false;
    }

    public bool CheckWinCondition(int row, int col, int player)
    {
        bool vertical = CheckWinConditionVertical(row, col, player);
        if (vertical)
        {
            Debug.Log($"Vertical Win Condition met: row {row}, col {col}");
            return true;
        }

        int margin = (int)Math.Ceiling((double)N / 2);

        int MarginColPos = (int)Math.Min(col + margin, ColumnAmount - 1);
        int MarginColNeg = (int)Math.Max(col - margin, 0);

        int MarginRowPos = (int)Math.Min(row + margin, RowAmount - 1);
        int MarginRowNeg = (int)Math.Max(row - margin, 0);

        Debug.Log($"Checking margin box for win condition: MarginRowNeg {MarginRowNeg}, MarginRowPos {MarginRowPos}, MarginColNeg {MarginColNeg}, MarginColPos {MarginColPos} ");

        for (int i = MarginRowPos; i >= MarginRowNeg; i--)
        {
            for (int j = MarginColPos; j >= MarginColNeg; j--)
            {
                if (CheckWinConditionHead(i, j, player))
                {
                    return true;
                }
            }
        }

        return false;

    }

    public bool CheckTieCodition()
    {
        bool AllColumnsFull = ColumnCount.Select(x => x == RowAmount).Aggregate((x, y) => x && y);
        Debug.Log($"Checking tie condition: all columns full? {AllColumnsFull}");

        return AllColumnsFull;
    }

    public int PerformMove(int move, int player)
    {
        int row = ColumnCount[move]++;
        GridDisks[row, move] = player;

        return row;
    }

}
