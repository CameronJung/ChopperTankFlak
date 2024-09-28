using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

//This component is responsible for maintaining a connection to the strategist in AI
public class UnitAI : MonoBehaviour
{

    public Objective CurrentObjective { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
        CurrentObjective = null;

    }

    // Update is called once per frame
    void Update()
    {
        
    }



}
