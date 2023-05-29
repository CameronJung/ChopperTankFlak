using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class BuildingOverlay : HexOverlay
{

    [SerializeField] private Faction owner;

    [SerializeField] private SpriteRenderer facilitySprite;

    [SerializeField] private GameObject captureSprite;

    private GameManager manager;

    private int security = 2;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        facilitySprite.color = TeamColours.GetValueOrDefault(owner);
        manager = GameObject.Find(MANAGERPATH).GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public override bool TravelGuide(Unit unit)
    {
        bool result = base.TravelGuide(unit);


        if(CanCapture(unit))
        {
            this.MarkCapture();
        }

        return result;
    }

    public override List<HexOverlay> BeginExploration(Unit unit)
    {
        List<HexOverlay> affected = base.BeginExploration(unit);

        if (CanCapture(unit))
        {
            MarkCapture();
        }

        return affected;
    }



    //This function is called when infantry capture a building
    public void Capture(Unit unit)
    {
        if(CanCapture(unit))
        {
            security--;

            if(security == 0)
            {
                //The building has been captured
                owner = unit.GetAllegiance();
                facilitySprite.color = TeamColours.GetValueOrDefault(owner);
                Debug.Log("Building Captured");
                manager.ReportHQCapture(owner);
            }
        }
    }

    private bool CanCapture(Unit unit)
    {
        bool can = (unit.GetUnitType() == UnitType.InfantrySquad) && (unit.GetAllegiance() != owner);
        return can;
    }

    public void CaptureInterrupted()
    {
        security = BUILDINGCAPFRESH;
    }



    //These functions are used to change an overlay's state
    protected override void MarkMove()
    {
        base.MarkMove();
        captureSprite.SetActive(false);
    }

    protected override void MarkCombat()
    {
        base.MarkCombat();
        captureSprite.SetActive(false);
    }

    protected override void MarkHold()
    {
        base.MarkHold();
        captureSprite.SetActive(false);
    }

    protected void MarkCapture()
    {
        moveSprite.SetActive(false);
        combatSprite.SetActive(false);
        holdSprite.SetActive(false);
        captureSprite.SetActive(true);
        currState = HexState.capture;
    }

    public override void SetBlank()
    {
        base.SetBlank();
        captureSprite.SetActive(false);
    }
}
