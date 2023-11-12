using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject GameOverMenu;
    private MenuManager mm;

    TextMeshProUGUI GameOverText;
    private const string TieMSG = "It’s a Draw";
    private const string WinnerMSGFormat = "Player {0} Wins!";

    private void Awake()
    {
        Debug.Log("UIManager awoken");

        GameOverText = GameObject.FindGameObjectWithTag("GameOverText").GetComponent<TextMeshProUGUI>();
        Debug.Assert(GameOverText != null);
        Debug.Log("GameOverText found");

        mm = GameObject.Find("MenuController").GetComponent<MenuManager>();

        GameOverMenu = mm.Menus["Connect4GameOver"];
        Debug.Assert(GameOverMenu != null);
        Debug.Log("GameOverMenu found");



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

        GameOverMenu.SetActive(true);
    }
}
