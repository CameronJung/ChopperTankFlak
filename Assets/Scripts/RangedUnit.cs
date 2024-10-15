using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public abstract class RangedUnit : TurretedUnit {

    //These two values dictate the number of tiles away a ranged unit can attack
    [SerializeField] protected int MinimumRange;
    [SerializeField] protected int MaximumRange;


    /*
     * DetermineAttackOptions
     * 
     * Since this unit attacks from a range the handelling needs to be different
     * 
     * This method will find everey hex within MaximumRange of this unit, and then mark every one of these hexes with a range 
     * of at least Minimum Range as AttackRange Or RangedAttack
     * 
     */
    protected override void DetermineAttackOptions()
    {
        HexOverlay start = this.GetOccupiedHex();

        HashSet<HexOverlay> explored = new HashSet<HexOverlay>();
        HashSet<HexOverlay> unexplored = new HashSet<HexOverlay>();
        HashSet<HexOverlay> discovered = new HashSet<HexOverlay>();
        unexplored.Add(start);

        int itter = 0;
        ResetPossibleAttacks();


        int step = 0;

        do
        {

            foreach(HexOverlay hex in discovered)
            {
                unexplored.Add(hex);
            }


            foreach( HexOverlay hex in unexplored)
            {
                int range = GridHelper.CalcTilesBetweenGridCoords(this.myTilePos, hex.myCoords);

                if (Affectors.ContainsKey(GridHelper.HashGridCoordinates(hex.myCoords)))
                {
                    Affectors[GridHelper.HashGridCoordinates(hex.myCoords)].MutateWithinRange(MinimumRange, MaximumRange);
                }
                else
                {
                    if(range >= MinimumRange && range <= MaximumRange)
                    {

                        HexState state = HexState.unreachable;

                        if(hex.GetOccupiedBy() != null)
                        {
                            if(hex.GetOccupiedBy().GetAllegiance() != this.GetAllegiance())
                            {
                                state = HexState.snipe;
                                PossibleAttacks[PredictBattleResult(this, hex.GetOccupiedBy())]++;
                            }
                        }

                        Affectors.Add(GridHelper.HashGridCoordinates(hex.myCoords), new HexAffect(this, hex, state, step, state == HexState.unreachable, true));
                        hex.intel.AffectedBy(this);
                    }
                }

                if (range <= MaximumRange)
                {

                    foreach (HexOverlay adj in hex.adjacent)
                    {

                        if (!discovered.Contains(adj) && !explored.Contains(adj))
                        {
                            discovered.Add(adj);
                        }

                    }

                }

                discovered.Remove(hex);
                explored.Add(hex);



            }

            foreach(HexOverlay hex in explored)
            {
                if (unexplored.Contains(hex))
                {
                    unexplored.Remove(hex);
                }
            }

            step++;
            itter++;
        } while (step <= MaximumRange && itter < 50);


        if (itter >= 100)
        {
            Debug.Log("Ranged Combat search exited through exceeding maximum itterations");
        }
    }


}
