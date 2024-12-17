using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Manuscript : MonoBehaviour
{

    [SerializeField] private Act[] Acts;


    private int CurrActIndex = 0;

    public DialogueStep CurrStep { get; private set; }



    private void Start()
    {
        
    }

    public Act GetCurrAct()
    {
        if (CurrActIndex < Acts.Length)
        {
            return Acts[CurrActIndex];
        }
        else
        {
            return null;
        }
    }


   

    public void GoToNextAct()
    {
        if (HasNextAct())
        {
            CurrActIndex++;
            CurrStep = GetCurrAct().GetCurrStep();
        }
        
    }


    public bool HasNextAct()
    {
        return CurrActIndex + 1 < Acts.Length;
    }

    public DialogueStep GetCurrStep()
    {
        return GetCurrAct().GetCurrStep();
    }

    public DialogueStep GetNextStep()
    {
        DialogueStep next = null;

        if (Acts[CurrActIndex].HasNextStep())
        {
            next = Acts[CurrActIndex].GetNextStep();
        }
        /*else if(HasNextAct())
        {
            CurrActIndex++;
            CurrStep = GetCurrAct().GetCurrStep();
            next = CurrStep;
        }*/



        return next;
    }


    public bool HasAnotherStep()
    {
        bool goOn = Acts[CurrActIndex].HasNextStep();

        goOn = goOn || HasNextAct();

        return goOn;
    }
}
