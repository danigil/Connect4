using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Test
{
    public static string Print2DArray<T>(T[,] matrix)
    {
        string s = "\n";
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                s += matrix[i, j] + "\t";
            }
            s += "\n";
        }

        return s;
    }

    public static void PrintActualExpected<T>(T[,] actual, T[,] expected)
    {
        string sa = Print2DArray(actual);
        string se = Print2DArray(expected);

        Debug.Log("Actual: " + sa);
        Debug.Log("Expected: " + se);
    }

    public static bool SequenceEquals<T>(T[,] a, T[,] b) => a.Rank == b.Rank
    && Enumerable.Range(0, a.Rank).All(d => a.GetLength(d) == b.GetLength(d))
    && a.Cast<T>().SequenceEqual(b.Cast<T>());

    [Test]
    public void TestCheckGrid()
    {
        
        Manager m;
        int RowAmount, ColAmount;

        RowAmount = 3;
        ColAmount = 3;
        m = new Manager(2, RowAmount, ColAmount);

        int[,] expected = { { -1, -1, -1 }, { -1, -1, -1 }, { -1, -1, -1 } };
        int[,] actual = m.GridDisks;

        Assert.True(SequenceEquals(actual, expected));

        m.PerformMove(0, 0);
        m.PerformMove(0, 1);
        m.PerformMove(1, 2);
        m.PerformMove(1, 3);

        expected = new int[,]{ { 0, 2, -1 }, { 1, 3, -1 }, { -1, -1, -1 } };

        Assert.True(SequenceEquals(actual, expected));


        m.Reset();
        m.PerformMove(0, 0);
        m.PerformMove(0, 0);

        actual = m.GridDisks;
        expected = new int[,] { { 0, -1, -1 }, { 0, -1, -1 }, { -1, -1, -1 } };

        Assert.True(SequenceEquals(actual, expected));

        m.Reset();
        m.PerformMove(0, 0);
        m.PerformMove(1, 0);

        actual = m.GridDisks;
        expected = new int[,] { { 0, 0, -1 }, { -1, -1, -1 }, { -1, -1, -1 } };

        Assert.True(SequenceEquals(actual, expected));

    }

    [Test]
    public void TestBasicWinScenarios()
    {
        int Nlimit = 10;
        int Rowlimit = 20;
        int Collimit = 20;
        for(int N=1; N < Nlimit; N++)
        {
            for (int Row = N; Row < Rowlimit; Row++)
            {
                for (int Col = N; Col < Collimit; Col++)
                {
                    BasicWinScenarios(N, Row, Col);
                }
            }
        }

    }

    private void BasicWinScenarios(int N, int RowAmount, int ColAmount)
    {
        Assert.True(N > 0 && RowAmount >= N && ColAmount >= N);

        Manager m = new Manager(N, RowAmount, ColAmount);
        int ret;

        // vertical wins
        for (int col=0; col < ColAmount; col++)
        {
            m.Reset();
            for (int n = 0; n < N - 1; n++)
            {
                ret = m.PerformMove(col, 0);
                Assert.False(m.CheckWinConditionVertical(ret, col, 0));
            }

            ret = m.PerformMove(col, 0);
            Assert.True(m.CheckWinConditionVertical(ret, col, 0));
        }

        // horizontal win
        m.Reset();
        for (int n = 0; n < N - 1; n++)
        {
            ret = m.PerformMove(n, 0);
            Assert.False(m.CheckWinConditionHorizontal(ret, n, 0, false));
        }

        ret = m.PerformMove(N - 1, 0);

        Assert.True(m.CheckWinConditionHorizontal(ret, N - 1, 0, false));


        // diagonal win
        m.Reset();
        for (int n = 0; n < N - 1; n++)
        {
            for (int i = 0; i < n; i++) 
            { 
                m.PerformMove(n, 2); 
            }
            ret = m.PerformMove(n, 0);
            
            Assert.False(m.CheckWinConditionDiagonal(ret, N - 1, 0, false));
        }

        for (int i = 0; i < N - 1; i++) {
            m.PerformMove(N - 1, i + 1); 
        }
        
        ret = m.PerformMove(N - 1, 0);

        Assert.True(m.CheckWinConditionDiagonal(ret, N - 1, 0, false));

    }
}
