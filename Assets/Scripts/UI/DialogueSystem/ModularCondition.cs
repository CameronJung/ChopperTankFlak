using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class ModularCondition:MonoBehaviour
{
    protected readonly Vector3 PointerOffset = new Vector3(-0.75f, -0.75f, 0.0f);


    protected SelectionManager Selector;

    [SerializeField] protected bool ShowMapArrow = true;

    protected void InitializeCondition()
    {
        Selector = GameObject.Find(UniversalConstants.SELECTORPATH).GetComponent<SelectionManager>();
    }

    //Returns true when the condition is true
    public abstract bool IsSatisfied();

    public abstract Vector3 GetSuggestedPointerPosition();

    public bool ShouldArrowShow()
    {
        return ShowMapArrow;
    }
}
