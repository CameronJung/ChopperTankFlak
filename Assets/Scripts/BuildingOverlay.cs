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
    private MilitaryManager militaryManager;

    private int security = 2;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        facilitySprite.color = TeamColours.GetValueOrDefault(owner);
        manager = GameObject.Find(MANAGERPATH).GetComponent<GameManager>();
        militaryManager = GameObject.Find(MANAGERPATH).GetComponent<MilitaryManager>();
        militaryManager.RegisterBuildingOwnership(this);
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
                militaryManager.ChangeBuildingOwnershipRegistration(this, unit.GetAllegiance());
                owner = unit.GetAllegiance();
                facilitySprite.color = TeamColours.GetValueOrDefault(owner);
                Debug.Log("Building Captured");
                manager.ReportHQCapture(owner);
            }
        }
    }

    public bool CanCapture(Unit unit)
    {
        bool can = (unit.GetUnitType() == UnitType.InfantrySquad) && (unit.GetAllegiance() != owner);
        return can;
    }

    public void CaptureInterrupted()
    {
        security = BUILDINGCAPFRESH;
    }

    protected override void ChangeState(HexState state, bool doShow)
    {
        base.ChangeState(state, doShow);

        if(state == HexState.capture)
        {
            this.MarkCapture();
        }
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
        rangeSprite.SetActive(false);
        snipeSprite.SetActive(false);
        currState = HexState.capture;
    }

    public override void SetBlank()
    {
        base.SetBlank();
        captureSprite.SetActive(false);
    }


    public Faction GetAllegiance()
    {
        return this.owner;
    }
}
