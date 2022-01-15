using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public enum MenuType
    {
        Dead,
        End,
        Pause,
    }

    public MenuType menuType;

    private void OnEnable()
    {
        Time.timeScale = 0;
        Time.fixedDeltaTime = 0;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (menuType == MenuType.Dead)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            } else if (menuType == MenuType.End)
            {
                if (SceneManager.sceneCountInBuildSettings == SceneManager.GetActiveScene().buildIndex + 1)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
