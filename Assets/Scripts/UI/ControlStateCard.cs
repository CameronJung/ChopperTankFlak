using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;
using System;



//This class is used to store the states of a player's access to the controls
[CreateAssetMenu]
public class ControlStateCard:ScriptableObject
{

    [SerializeField] public Dictionary<BooleanControls, ControlAccess> States;

    [SerializeField] private ControlAccess SelectAccess = ControlAccess.free;
    [SerializeField] private ControlAccess OrderUnitAccess = ControlAccess.free;

    [SerializeField] private bool DeselectAccess = true;
    [SerializeField] private bool EndTurnAccess = true;
    [SerializeField] private bool PanMapAccess = true;
    [SerializeField] private bool AutomaticTurnEnd = true;
    [SerializeField] private bool ContinueAccess = true;
    [SerializeField] private bool ShowDialogue = false;


    public ControlStateCard()
    {
        States = new Dictionary<BooleanControls, ControlAccess>();

        foreach(BooleanControls cont in Enum.GetValues(typeof(BooleanControls)))
        {
            States.Add(cont, ControlAccess.free);
        }
    }


    public bool CheckBooleanControlPermission(BooleanControls control)
    {
        bool value = false;

        switch (control)
        {
            case BooleanControls.deselect:
                value = DeselectAccess;
                break;
            case BooleanControls.end_turn:
                value = EndTurnAccess;
                break;
            case BooleanControls.pan_map:
                value = PanMapAccess;
                break;
            case BooleanControls.AutoTurnEnd:
                value = AutomaticTurnEnd;
                break;
            case BooleanControls.ContinueButton:
                value = ContinueAccess;
                break;
            case BooleanControls.DialogueShown:
                value = ShowDialogue;
                break;

        }

        return value;
    }


    public bool CheckSpecifiableControlPermission( SpecifiableControls specific, ControlAccess expected)
    {
        bool correct = false;

        switch (specific){
            case SpecifiableControls.select:
                correct = expected == SelectAccess;
                break;
            case SpecifiableControls.order_unit:
                correct = expected == OrderUnitAccess;
                break;
        }

        return correct;
    }

}
