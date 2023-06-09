using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndGamePanel : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private TextMeshProUGUI defeatText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleGameEnd(bool victorious)
    {
        victoryText.gameObject.SetActive(victorious);
        defeatText.gameObject.SetActive(!victorious);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
