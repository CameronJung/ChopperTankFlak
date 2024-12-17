using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStep : DialogueStep
{

    public ModularCondition Condition { get; private set; }

    [SerializeField] public Vector3 CursorPos { get; protected set; }

    private void Awake()
    {
        this.Condition = gameObject.GetComponent<ModularCondition>();
    }

    private void Start()
    {
        
        this.ShowContinue = false;

        
        
    }


    public override bool IsConditionSatisfied()
    {
        return Condition.IsSatisfied();
    }

    public Vector3 GetPositionForMapPointer()
    {
        if (this.Condition == null)
        {
            Debug.Log("Could not find a condition");
        }
        else
        {
            CursorPos = Condition.GetSuggestedPointerPosition();
        }

        return CursorPos;
    }

    public bool ShouldShowMapPointer()
    {
        return Condition.ShouldArrowShow();
    }
}
