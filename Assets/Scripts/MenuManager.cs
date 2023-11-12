using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.SceneManagement;
using System;

public class MenuManager : MonoBehaviour
{
    public bool CloseOnAwake = false;

    public Dictionary<string, GameObject> Menus;
    private GameObject[] MenuRadioButtons;
    private static string MenuRadioButtonTag = "MenuRadioButton";

    private Button MenuPlayButton;
    private static string MenuPlayButtonTag = "MenuPlayButton";

    private string ChosenGameMode;

    private void Awake()
    {
        MenuRadioButtons = GameObject.FindGameObjectsWithTag(MenuRadioButtonTag);
        Debug.Assert(MenuRadioButtons != null);
        Debug.Log($"Successfully detected {MenuRadioButtons.Length} menu radio buttons by '{MenuRadioButtonTag}' tag");

        MenuPlayButton = GameObject.FindGameObjectWithTag(MenuPlayButtonTag).GetComponent<Button>();
        Debug.Assert(MenuPlayButton != null);
        MenuPlayButton.onClick.AddListener(HandlePlayButton);
        MenuPlayButton.interactable = false;

        foreach (GameObject btn in MenuRadioButtons)
            btn.GetComponent<Button>().onClick.AddListener(() => { HandleMenuRadioButton(btn); });

        Menus = GameObject.FindGameObjectsWithTag("Menu").ToDictionary(x => x.name);
        Debug.Log($"Menus dictionary constructed - {string.Join(",", Menus.Select(kvp => $"{kvp.Key}"))}");

        Button[] CloseButtons = GameObject.FindGameObjectsWithTag("CloseButton").Select<GameObject, Button>(x => x.GetComponent<Button>()).ToArray();
        Debug.Log($"Found {CloseButtons.Count()} Close Buttons");

        foreach (Button b in CloseButtons)
        {
            b.onClick.AddListener(() => { CloseWindow(b); });
        }


        if (CloseOnAwake)
        {
            foreach(GameObject Menu in GameObject.FindGameObjectsWithTag("Menu"))
            {
                Menu.SetActive(false);
            }
        }
    }

    public void GoToMainMenu()
    {
        Destroy(GameManager.Instance.gameObject);
        SceneManager.LoadScene("Connect4_Menu");
    }

    public void ShowChooseMenu()
    {
        Menus["Connect4GameOver"].SetActive(false);
        Menus["Connect4ChoosePlayer"].SetActive(true);
    }

    public void CloseWindow(Button b)
    {
        b.transform.parent.gameObject.SetActive(false);
    }

    private void HandleMenuRadioButton(GameObject button)
    {
        Debug.Log($"Menu Radio Button {button.name} was selected.");
        MenuPlayButton.interactable = true;

        ChosenGameMode = button.name.ToLower();
    }

    private void HandlePlayButton()
    {
        GameManager gm = GameManager.Instance;
        if (ChosenGameMode.Contains("pvp"))
        {
            gm.StartGame(GameManager.GameType.LocalPVP);
        }
        else if (ChosenGameMode.Contains("com"))
        {
            gm.StartGame(GameManager.GameType.COM);
        }
        else
        {
            Debug.Log($"Unrecognized gamemode {ChosenGameMode}");
        }
    }
}
