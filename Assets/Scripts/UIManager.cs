using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    TextMeshProUGUI GameOverText;
    private const string TieMSG = "It’s a Draw";
    private const string WinnerMSGFormat = "Player {0} Wins!";

    private void Awake()
    {
        Debug.Log("UIManager awoken");

        GameOverText = GameObject.FindGameObjectWithTag("GameOverText").GetComponent<TextMeshProUGUI>();
        Debug.Assert(GameOverText != null);

        Debug.Log("GameOverText found");
        Reset();
    }

    public void Reset()
    {
        GameOverText.text = "";
    }

    public void DisplayGameOver(int winner)
    {
        if(winner < 0)
        {
            GameOverText.text = TieMSG;
        }
        else
        {
            GameOverText.text = string.Format(WinnerMSGFormat, winner + 1);
        }
    }
}
