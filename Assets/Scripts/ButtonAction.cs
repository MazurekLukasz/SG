using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonAction : MonoBehaviour
{
    public static ButtonAction Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void TooglePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf);
        }

        if (GameObject.FindGameObjectWithTag("Master") != null)
        {
            GameObject.FindGameObjectWithTag("Master").GetComponent<GameLogic>().Pause = !GameObject.FindGameObjectWithTag("Master").GetComponent<GameLogic>().Pause;

            if (!panel.activeInHierarchy)
                GameObject.FindGameObjectWithTag("Master").GetComponent<GameLogic>().UnselectSelectedObject();
        }

        if (Camera.main.GetComponent<CameraMovement>())
        {
            Camera.main.GetComponent<CameraMovement>().Pause = !Camera.main.GetComponent<CameraMovement>().Pause;
        }
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void PlayMenu()
    {

    }
}
