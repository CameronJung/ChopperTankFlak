using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{

    [SerializeField] private RectTransform mapPanel;
    [SerializeField] private RectTransform menuPanel;

    //How proportion of the screen width the game map occupies
    private const float MAPFRACTION = 0.75f;


    


    // Start is called before the first frame update
    void Start()
    {
        int width = Screen.width;
        float mapWidth = width * MAPFRACTION;
        mapPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mapWidth);

        Vector2 mapPos = mapPanel.anchoredPosition;
        mapPos.x = mapWidth * 0.5f;
        mapPanel.anchoredPosition = mapPos;

        float menuWidth = width - mapWidth;
        menuPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, menuWidth);

        Vector2 menuPos = menuPanel.anchoredPosition;
        menuPos.x = mapWidth + (0.5f * menuPanel.rect.width);
        menuPanel.anchoredPosition = menuPos;
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
}
