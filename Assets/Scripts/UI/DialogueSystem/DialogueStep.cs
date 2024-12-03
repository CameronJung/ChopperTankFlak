using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DialogueStep : MonoBehaviour{

    [SerializeField] [TextArea] private string Words;

    [SerializeField] private bool RequiresMobileVersion;
    [SerializeField] [TextArea] private string MobileText;

    [SerializeField] public bool ShowContinue { get; protected set; } = true;
    [SerializeField] private bool ShowBox = true;

    [SerializeField] protected ControlStateCard ControlsDuringStep = null;
    protected DialogueBox Box = null;



    virtual public bool IsConditionSatisfied()
    {
        return true;
    }

    public string GetText()
    {
        if (Application.isMobilePlatform)
        {
            return MobileText;
        }
        else
        {
            return Words;
        }
    }

    public virtual ControlStateCard GetControlsForStep()
    {
        return ControlsDuringStep;
    }


    public virtual void LoadStep(DialogueBox box)
    {

    }
    
    public bool ShouldBoxBeShown()
    {
        return ShowBox;
    }
}
