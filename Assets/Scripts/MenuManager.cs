using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
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
