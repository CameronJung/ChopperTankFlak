using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//An act represents the series of dialgue steps that occur during a turn, be it the player or AI
public class Act : MonoBehaviour
{

    [SerializeField] private DialogueStep[] lines;
    private int CurrStepIndex = 0;    


    public DialogueStep GetCurrStep()
    {
        if(CurrStepIndex < lines.Length)
        {
            return lines[CurrStepIndex];
        }
        else
        {
            return null;
        }
    }


    public DialogueStep GetNextStep()
    {
        CurrStepIndex++;
        return GetCurrStep();
    }

    public bool HasNextStep()
    {
        return CurrStepIndex + 1 < lines.Length;
    }


    public List<RequiredMove> GetAISteps()
    {
        List<RequiredMove> moves = new List<RequiredMove>();

        foreach(DialogueStep step in lines)
        {
            if(step is RequiredMove)
            {

                moves.Add((RequiredMove)step);
            }
        }

        return moves;
    }

}
