using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


/*
 * This component is built to provide reusable handeling of loading game levels
 * - it can be provided a specific level at runtime or in the editor
 * - This component MUST share its Gameobject with a button component 
 */
public class LoadLevelButton : MonoBehaviour
{

    private Button MyButton;
    [SerializeField] Level ChosenLevel = null;

    // Start is called before the first frame update
    void Start()
    {
        MyButton = gameObject.GetComponent<Button>();
        if(MyButton != null)
        {
            MyButton.interactable = ChosenLevel != null;
        }
        else
        {
            Debug.LogError("A LoadLevelButton is in a GameObject without a button component");
            gameObject.SetActive(false);
        }
    }

    public void ChangeChosenLevel(Level picked)
    {
        MyButton.interactable = picked != null;
        this.ChosenLevel = picked;
    }


    public void LoadLevel()
    {
        if(ChosenLevel != null)
        {
            SceneManager.LoadScene(ChosenLevel.GetLevelID());
        }
    }


    
}
