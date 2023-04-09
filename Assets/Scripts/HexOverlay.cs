using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class HexOverlay : MonoBehaviour
{
    [SerializeField] private Unit occupiedBy { get; set; }
    public bool hasUnit = false;

    [SerializeField] private GameObject availSprite;

    //this flag is used to mark tiles that have already been visited by a recursive algorythm
    public bool visited = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Unit GetOccupiedBy()
    {
        return this.occupiedBy;
    }

    public void SetOccupiedBy(Unit unit)
    {
        hasUnit = unit != null;
        availSprite.SetActive(true);
        this.occupiedBy = unit;
    }
}
