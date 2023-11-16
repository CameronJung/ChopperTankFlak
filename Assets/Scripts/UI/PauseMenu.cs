using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{

    [SerializeField] private GameObject ConfirmationPanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ShowConfirmation()
    {
        ConfirmationPanel.SetActive(true);
    }

    public void CloseConfirmation()
    {
        ConfirmationPanel.SetActive(false);
    }
}
