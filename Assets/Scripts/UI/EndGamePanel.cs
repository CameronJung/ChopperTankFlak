using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
}
