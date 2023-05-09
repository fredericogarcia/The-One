using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    private EventSystem eSystem;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject controls;
    [SerializeField] private GameObject mainMenuButton;
    [SerializeField] private GameObject controlsButton;
    private bool controlsShown;

    private void Awake()
    {
        Application.targetFrameRate = 120;
        eSystem = EventSystem.current;
    }

    public void ShowAndHideControls() {
        if (mainMenu == null || controls == null) return;

        switch (controlsShown) {
            case false:
                eSystem.SetSelectedGameObject(null);
                eSystem.SetSelectedGameObject(controlsButton);
                mainMenu.SetActive(false);
                controls.SetActive(true);
                controlsShown = true;
            break;
            case true:
                eSystem.SetSelectedGameObject(null);
                eSystem.SetSelectedGameObject(mainMenuButton);
                mainMenu.SetActive(true);
                controls.SetActive(false);
                controlsShown = false;
            break;
        }
    }

}
