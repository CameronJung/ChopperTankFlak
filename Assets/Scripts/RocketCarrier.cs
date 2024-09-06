using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class RocketCarrier : RangedUnit
{

    [SerializeField] private GameObject Rocket;

     private float RocketSpeed = 4.5f;

    readonly private float IMPACTDISTANCE = 0.5f;

    private Vector3 RocketPositionOnGantry;

    override public void Start()
    {
        base.Start();

        if(Rocket != null)
        {
            RocketPositionOnGantry = Rocket.transform.localPosition;
        }
    }


    public override bool Revitalize()
    {
        if(Rocket != null)
        {
            Rocket.SetActive(true);
        }

        return base.Revitalize();
    }



    public override UniversalConstants.UnitType GetUnitType()
    {
        return UnitType.Artillery;
    }

    /*
     * The rocket carrier is defeated by any unit in standard combat
     */
    public override bool IsWeakTo(Unit foe)
    {
        return true;
    }


    public override void ResolveCombat(Unit other)
    {
        //Rocket carriers do not expect a response because they always destroy their target
        //Further Rocket Carriers are always destroyed when attacked
        other.BeEngaged(this);
    }



    //POLYMORPHISM
    public override void BeEngaged(Unit assailant)
    {

        base.Die();

        

    }


    /*
     * Execute Attack Order
     * 
     * This unit has special handeling for the attack order, the rocket visibly flies toward the targer and impacts it
     */
    public override IEnumerator ExecuteAttackOrder(Vector3 origin, Vector3 destination)
    {
        Vector3 displacement = destination - origin;


        //Attack orders always come at the end of a stack so we should update the board ASAP
        this.FinalizeMovement();

        yield return AimTurret(displacement);


        HexOverlay hex = map.GetInstantiatedObject(map.WorldToCell(destination)).GetComponent<HexOverlay>();
        Unit target = hex.GetOccupiedBy();

        //Play attack animation
        if (puppeteer != null && Rocket != null)
        {
            puppeteer.SetTrigger("Attack");
            soundMaker.PlayOneShot(attackSound);
            yield return new WaitForSeconds(0.4f);

            while(Vector3.Distance(Rocket.transform.position, destination) > IMPACTDISTANCE * IMPACTDISTANCE)
            {
                Rocket.transform.position = Vector3.MoveTowards(Rocket.transform.position, destination, RocketSpeed * Time.deltaTime);
                //Vector3.Lerp(Rocket.gameObject.transform.position, destination, RocketSpeed * Time.deltaTime);
                yield return null;
            }

            puppeteer.SetTrigger("Detonated");
            Rocket.SetActive(false);
            Rocket.transform.localPosition = RocketPositionOnGantry;
            
        }

        

        this.ResolveCombat(target);

        //wait for a few frames to ensure that the other unit has had time to handle being attacked
        for (int i = 0; i < 3; i++)
        {
            yield return null;
        }



        orderComplete = true;
    }

}
