using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class EndGamePanel : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private TextMeshProUGUI defeatText;
    [SerializeField] private AudioClip celebrationNoise;
    [SerializeField] private AudioClip saddnessNoise;
    [SerializeField] private Button NextLevelButton;
    private AudioSource noiseMaker;

    private void Awake()
    {
        noiseMaker = gameObject.GetComponent<AudioSource>();
    }

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
        
        if (victorious)
        {
            noiseMaker.PlayOneShot(celebrationNoise);
        }
        else
        {
            noiseMaker.PlayOneShot(saddnessNoise);
            
        }

        

        if (NextLevelButton.GetComponent<LoadLevelButton>().HasLevel())
        {
            Debug.Log("Next level button has a level");
            NextLevelButton.interactable = victorious;
            Debug.Assert(NextLevelButton.interactable == victorious, "interactability was not set properly");
        }
        else
        {
            Debug.Log("Next level button does not has a level"); 
            NextLevelButton.gameObject.SetActive(false);
        }
        
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
