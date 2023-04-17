using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class GameManager : MonoBehaviour
{
    private int turn = 0;

    private List<Unit> playerUnits = new List<Unit>();
    private List<Unit> computerUnits = new List<Unit>();



    // Start is called before the first frame update0
    void Start()
    {
        BeginTurn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Recruit(Unit unit)
    {
        if(unit.GetAllegiance() == Faction.PlayerTeam)
        {
            playerUnits.Add(unit);
        }
        else
        {
            computerUnits.Add(unit);
        }
    }

    private void BeginTurn()
    {
        if(turn % 2 == 0)
        {
            //Player's turn
            foreach(Unit unit in playerUnits)
            {
                unit.Revitalize();
            }
        }
        else
        {
            //Computer's turn
        }
    }
}
