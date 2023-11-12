using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private GameObject PauseMenu;
    public GameObject GameOverMenu;
    private MenuManager mm;
    private GameManager gm;

    TextMeshProUGUI GameOverText;
    private const string TieMSG = "It’s a Draw";
    private const string WinnerMSGFormat = "Player {0} Wins!";

    private GameObject SFXToggle;
    private GameObject MusicToggle;

    private void Awake()
    {
        Debug.Log("UIManager awoken");

        GameOverText = GameObject.FindGameObjectWithTag("GameOverText").GetComponent<TextMeshProUGUI>();
        Debug.Assert(GameOverText != null);
        Debug.Log("GameOverText found");

        gm = GameManager.Instance;
        mm = GameObject.Find("MenuController").GetComponent<MenuManager>();

        GameOverMenu = mm.Menus["Connect4GameOver"];
        Debug.Assert(GameOverMenu != null);
        Debug.Log("GameOverMenu found");

        PauseMenu = mm.Menus["Connect4PauseMenu"];

        SFXToggle = PauseMenu.transform.Find("SFXSlider").gameObject;
        MusicToggle = PauseMenu.transform.Find("MusicSlider").gameObject;

        Slider s, m;

        SFXToggle.TryGetComponent(out s);
        if (s != null)
        {
            Debug.Log("Found SFX slider");
            s.onValueChanged.AddListener(SetLevelSFX);
        }

        MusicToggle.TryGetComponent(out m);
        if (m != null)
        {
            Debug.Log("Found Music slider");
            m.onValueChanged.AddListener(SetLevelMusic);
        }

        Reset();
    }

    public void Reset()
    {
        Debug.Log("Resetting UIManager");
        GameOverText.text = "";

        Debug.Log($"GameOverText after reset {GameOverText.text}");

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

    public void SetLevelSFX(float level)
    {
        gm.am.SetSFX(level);
    }

    public void SetLevelMusic(float level)
    {
        gm.am.SetMusic(level);
    }
}
