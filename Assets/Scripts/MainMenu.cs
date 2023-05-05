using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject controls;
    private bool controlsShown;

    public void ShowAndHideControls() {
        if (mainMenu == null || controls == null) return;

        switch (controlsShown) {
            case false:
                mainMenu.SetActive(false);
                controls.SetActive(true);
                controlsShown = true;
            break;
            case true:
                mainMenu.SetActive(true);
                controls.SetActive(false);
                controlsShown = false;
            break;
        }
    }

}
