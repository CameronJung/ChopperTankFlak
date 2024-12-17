using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UniversalConstants;

public class DialogueManager : MonoBehaviour
{

    private GameManager Manager;
    private ControlsManager Controller;
    private DialogueBox Box;

    [SerializeField] Manuscript Playbook;


    public Act CurrAct { get; private set; }
    public DialogueStep CurrStep { get; private set; }

    private void Awake()
    {
        
    }

    


    // Start is called before the first frame update
    void Start()
    {
        Manager = gameObject.GetComponent<GameManager>();
        Controller = gameObject.GetComponent<ControlsManager>();
        CurrStep = Playbook.GetCurrStep();
        CurrAct = Playbook.GetCurrAct();
        HandleStep(CurrStep);
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void LearnOfDialogueBox(DialogueBox box)
    {
        Box = box;
    }

    public void LoadNextAct()
    {
        if (Playbook.HasNextAct())
        {
            Playbook.GoToNextAct();
            CurrStep = Playbook.GetCurrStep();
            Box.LoadDialogueFromStep(CurrStep);
        }
        else
        {
            //If there are no more Acts hide the dialogue box
            Box.LoadDialogueFromStep(null);
        }
    }


    private void HandleStep(DialogueStep step)
    {
        Box.LoadDialogueFromStep(step);
        Controller.LoadFromStep(step);

    }


    public void NextAct()
    {
        if (Playbook.HasNextAct())
        {
            Playbook.GoToNextAct();
            CurrAct = Playbook.GetCurrAct();
            CurrStep = Playbook.CurrStep;
            HandleStep(CurrStep);
        }
    }


    /*
     * Go To Next Step
     * 
     * This method will attempt to proceed to the next dialogue, it will do nothing if conditions aren't met
     * 
     */
    public void GoToNextStep()
    {
        Debug.Log("Tried to go to the next dialogue");

        HandleStep(Playbook.GetNextStep());
    }


    public void HideBox()
    {
        Box.ObscureSelf();
    }

    public void ShowBox()
    {

        if (Playbook.HasAnotherStep())
        {
            Box.RevealSelf();
        }

        
    }

}
