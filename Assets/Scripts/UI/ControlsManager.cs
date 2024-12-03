using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;
using System;


/*
 * Controls Manager
 * 
 * The controls manager is responsible for keeping track of which controls and automated systems are enabled
 * 
 * The scripts that actually contain these controls are responsible for checking with this component before acting
 */
public class ControlsManager : MonoBehaviour
{

    public ControlStateCard CurrentPermissions { get; private set; }
    private ModularCondition CurrentCondition;
    [SerializeField] private ControlStateCard DefaultPermissions;
    public DialogueBox Box { get; private set; }
    private DialogueManager Director;

    private void Awake()
    {
        if(Box == null)
        {
            CurrentPermissions = DefaultPermissions;
        }

        Director = gameObject.GetComponent<DialogueManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LearnOfDialogueBox(DialogueBox box)
    {
        Box = box;
    }

    public void LoadFromStep(DialogueStep step)
    {
        if (step != null)
        {
            CurrentPermissions = step.GetControlsForStep();

            if (step is TutorialStep)
            {
                TutorialStep tut = (TutorialStep)step;
                CurrentCondition = tut.Condition;
            }
        }
        else
        {
            CurrentPermissions = DefaultPermissions;
        }
    }

    public bool IsConditionSatisfied()
    {
        bool satisfied = false;
        
        if(CurrentCondition != null) satisfied = CurrentCondition.IsSatisfied();

        if (satisfied)
        {
            Director.GoToNextStep();
        }

        return satisfied;
    }


    

}
