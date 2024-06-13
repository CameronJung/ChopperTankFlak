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
    [SerializeField] private TextMeshProUGUI label;
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
            slider.maxValue = TouchSheets.Length;
            Debug.Log("application is a mobile device");
        }
        else
        {
            //ControlSet = 0;
            slider.maxValue = MouseSheets.Length;
            Debug.Log("Application is not a mobile device");
        }

        //TurnToPage();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //Changes the displayed page
    public void TurnToPage()
    {
        int pageNum = Mathf.RoundToInt(slider.value) - 1;

        //Deactivate all sheets
        foreach (GameObject sheet in MouseSheets)
        {
            sheet.SetActive(false);
        }
        foreach (GameObject sheet in TouchSheets)
        {
            sheet.SetActive(false);
        }

        
        //And just turn the selected sheet back on
        Sheets[ControlSet][pageNum].SetActive(true);

        currPage = pageNum;


        label.text = (currPage + 1) + " / " + MouseSheets.Length;
    }

    public void ToTitle()
    {
        SceneManager.LoadScene(0);
    }

}
