using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Pages : MonoBehaviour
{

    [SerializeField] private GameObject[] sheets;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Slider slider;


    private int currPage = 1;

    // Start is called before the first frame update
    void Start()
    {
        TurnToPage();
        slider.maxValue = sheets.Length;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //Changes the displayed page
    public void TurnToPage()
    {
        int pageNum = Mathf.RoundToInt(slider.value) - 1;
        for(int p = 0; p < sheets.Length; p++)
        {
            sheets[p].SetActive(pageNum == p);
        }
        currPage = pageNum;
        label.text = (currPage + 1) + " / " + sheets.Length;
    }

    public void ToTitle()
    {
        SceneManager.LoadScene(0);
    }

}
