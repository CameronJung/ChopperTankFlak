using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectionManager : MonoBehaviour
{

    [SerializeField] private ReconPanel Recon;
    [SerializeField] private LoadLevelButton LoadButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ChangeSelectedLevelTo(Level pick)
    {
        Recon.DisplayIntel(pick.GetTitle(), pick.GetDescription());
        LoadButton.ChangeChosenLevel(pick);
    }
}
