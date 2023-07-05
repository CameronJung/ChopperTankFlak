using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TitleMenuManager : MonoBehaviour
{

    [SerializeField] private GameObject exitButton;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_WEBGL
    exitButton.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowInstructions()
    {
        SceneManager.LoadScene(2);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#endif
        Application.Quit();
    }
}
