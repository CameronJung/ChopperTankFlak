using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReconPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private string noTitle;
    private string noInfo;

    public bool hasSelection { get; private set; } = false;
    public bool unitSelected { get; private set; } = false;
    private Vector3Int selectedTile;

    // Start is called before the first frame update
    void Start()
    {
        noTitle = titleText.text;
        noInfo = descriptionText.text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayIntel(string title, string info)
    {
        titleText.text = title;
        descriptionText.text = info;
    }

    public void DisplayIntelAbout(Object subject, Vector3Int tilePos)
    {
        if(subject is ISelectable)
        {
            ISelectable selectable = (ISelectable)subject;
            titleText.text = selectable.GetTitle();
            descriptionText.text = selectable.GetDescription();
        }
        unitSelected = subject is Unit;
        
    }

    public void DisplayNone()
    {
        titleText.text = noTitle;
        descriptionText.text = noInfo;
    }

}
