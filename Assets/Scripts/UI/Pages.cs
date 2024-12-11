using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Pages : MonoBehaviour
{

    [SerializeField] private GameObject[] MouseSheets;
    [SerializeField] private GameObject[] TouchSheets;
    [SerializeField] private TextMeshProUGUI label = null;
    [SerializeField] private Slider slider;

    private GameObject[][] Sheets;
    

    private int currPage = 1;
    //0 for mouse controls, 1 for touch screen controls
    private int ControlSet = 0;

    // Start is called before the first frame update
    void Start()
    {
        Sheets = new GameObject[][]{ MouseSheets, TouchSheets};
#if PLATFORM_ANDROID
        ControlSet = 1;
#endif

        if (ControlSet == 1)
        {
            //ControlSet = 1;
            slider.maxValue = TouchSheets.Length - 1;
            Debug.Log("application is a mobile device");
        }
        else
        {
            //ControlSet = 0;
            slider.maxValue = MouseSheets.Length - 1;
            Debug.Log("Application is not a mobile device");
        }

        TurnToPage();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //Changes the displayed page
    public void TurnToPage()
    {
        int pageNum = Mathf.RoundToInt(slider.value);
        

        for(int i = Mathf.FloorToInt(slider.minValue); i <= slider.maxValue; i++)
        {
            Sheets[ControlSet][i].SetActive(i == Mathf.RoundToInt(slider.value));
        }

        if (label != null)
        {
            label.text = (pageNum + 1) + " / " + MouseSheets.Length;
        }
    }

    public void ToTitle()
    {
        SceneManager.LoadScene(0);
    }

}
