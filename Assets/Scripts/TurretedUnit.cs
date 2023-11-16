using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurretedUnit : Unit
{
    //The purpose of this class is used to change how units with turrets process attack orders,
    //ie so that they turn their turret instead of fully facing the enemy to attack
    [SerializeField] protected GameObject Turret;
    protected float turretTraverse = 150f;




    public override IEnumerator ExecuteMoveOrder(Vector3 origin, Vector3 destination)
    {
        Vector3 direction = destination - origin;
        float travel = this.speed * Time.deltaTime;
        WaitForFixedUpdate wait = new WaitForFixedUpdate();
        Vector3 heading = Turret.transform.eulerAngles;
        float turnBy = Time.deltaTime * this.turnSpeed;

        int count = 1;

        //If we can make it to the destination this frame we just set our location instead
        while ((travel * travel) <= (destination - gameObject.transform.position).sqrMagnitude && count < MAXFRAMESPERACTION)
        {
            gameObject.transform.position += direction.normalized * travel;

            //We only rotate the base sprite so the availSprite remains aligned with the grid
            Vector3 angles = baseSprite.transform.eulerAngles;
            angles.z = Vector3.SignedAngle(Vector3.right, direction, Vector3.forward);
            baseSprite.transform.eulerAngles = angles;

            //Return the turret to its rest possition
            //Return the turret to the neutral position

            float bearing = baseSprite.transform.eulerAngles.z;

            if (bearing < 0)
            {
                bearing += 360;
            }

            float spinDirection = Vector3.SignedAngle(Turret.transform.TransformDirection(Vector3.right), baseSprite.transform.TransformDirection(Vector3.right), Vector3.forward);
            spinDirection = Mathf.Sign(spinDirection);

            count = 1;

            if ((turnBy < Mathf.Abs(heading.z - bearing)))
            {
                
                turnBy = Time.deltaTime * this.turnSpeed;
                Turret.transform.Rotate(Vector3.forward, spinDirection * turnBy);
                heading = Turret.transform.eulerAngles;
            }


            yield return wait;
            direction = destination - transform.position;
            travel = this.speed * Time.deltaTime;
            count++;
        }

        if (count >= MAXFRAMESPERACTION)
        {
            Debug.Log("!ERROR! Move order completed through exceeding maximum frame tolerance");
        }

        gameObject.transform.position = destination;

        orderComplete = true;

    }




    public override IEnumerator ExecuteAttackOrder(Vector3 origin, Vector3 destination)
    {
        Vector3 displacement = destination - origin;
        Vector3 heading = Turret.transform.eulerAngles;
        float turnBy = Time.deltaTime * this.turnSpeed;
        float bearing = Vector3.SignedAngle(Vector3.right, displacement, Vector3.forward);
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        //Attack orders always come at the end of a stack so we should update the board ASAP
        this.FinalizeMovement();


        if (bearing < 0)
        {
            bearing += 360;
        }

        float direction = Vector3.SignedAngle(Turret.transform.TransformDirection(Vector3.right), displacement, Vector3.forward);

        direction = Mathf.Sign(direction);

        int count = 1;

        while ((turnBy < Mathf.Abs(heading.z - bearing)) && count < MAXFRAMESPERACTION)
        {
            Turret.transform.Rotate(Vector3.forward, direction * turnBy);
            yield return wait;
            turnBy = Time.deltaTime * this.turnSpeed;
            heading = Turret.transform.eulerAngles;
            count++;
        }

        if (count >= MAXFRAMESPERACTION)
        {
            Debug.Log("!ERROR! Attack order completed through exceeding maximum frame tolerance");
        }

        Turret.transform.eulerAngles = new Vector3(0, 0, bearing);
        HexOverlay hex = map.GetInstantiatedObject(map.WorldToCell(destination)).GetComponent<HexOverlay>();
        Unit target = hex.GetOccupiedBy();

        //Play attack animation
        if (puppeteer != null)
        {
            puppeteer.SetTrigger("Attack");
            yield return new WaitForSeconds(0.5f);
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
