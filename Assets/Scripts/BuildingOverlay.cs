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


    public override bool TravelGuide(Unit unit, bool doShow = true)
    {
        bool result = base.TravelGuide(unit, doShow);


        if(CanCapture(unit))
        {
            this.MarkCapture(doShow);
        }

        return result;
    }

    public override List<HexOverlay> BeginExploration(Unit unit, bool doShow = true)
    {
        List<HexOverlay> affected = base.BeginExploration(unit, doShow);

        if (CanCapture(unit))
        {
            MarkCapture(doShow);
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
    protected override void MarkMove(bool doShow = true)
    {
        base.MarkMove(doShow);
        captureSprite.SetActive(false);
    }

    protected override void MarkCombat(bool doShow = true)
    {
        base.MarkCombat(doShow);
        captureSprite.SetActive(false);
    }

    protected override void MarkHold(bool doShow = true)
    {
        base.MarkHold(doShow);
        captureSprite.SetActive(false);
    }

    protected void MarkCapture(bool doShow = true)
    {
        moveSprite.SetActive(false);
        combatSprite.SetActive(false);
        holdSprite.SetActive(false);
        captureSprite.SetActive(doShow);
        currState = HexState.capture;
    }

    public override void SetBlank()
    {
        base.SetBlank();
        captureSprite.SetActive(false);
    }
}
