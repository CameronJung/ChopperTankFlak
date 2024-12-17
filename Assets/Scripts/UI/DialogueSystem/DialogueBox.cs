using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UniversalConstants;

public class DialogueBox : MonoBehaviour
{

    private const float HIDE_DISTANCE = 10.0f;

    [SerializeField] private TextMeshProUGUI TextBox;

    [SerializeField] private Manuscript Playbook;
    [SerializeField] private GameObject ContinueButton;

    [SerializeField] private GameObject Pointer;
    [SerializeField] private GameObject Highlighter = null;


    private Sprite PointerSprite;

    private DialogueStep CurrStep;

    private ControlsManager Controls;

    private DialogueManager Director;

    private Vector3 HidingSpot;
    private Vector3 StagePlace;


    private void Awake()
    {
        Director = GameObject.Find(MANAGERPATH).GetComponent<DialogueManager>();
        Director.LearnOfDialogueBox(this);
    }


    // Start is called before the first frame update
    void Start()
    {
        
        PointerSprite = Pointer.GetComponent<Sprite>();
        Controls = GameObject.Find(UniversalConstants.MANAGERPATH).GetComponent<ControlsManager>();
        
        Controls.LearnOfDialogueBox(this);

        Debug.Log("Size is: " + gameObject.GetComponent<RectTransform>().rect.height);
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void LoadDialogueFromStep(DialogueStep step)
    {
        if(step != null)
        {
            CurrStep = step;
            TextBox.text = step.GetText();

            ContinueButton.SetActive(step.ShowContinue);

            if (step is TutorialStep)
            {
                TutorialStep tutor = (TutorialStep)step;
                Pointer.SetActive(tutor.ShouldShowMapPointer());
                if (tutor.ShouldShowMapPointer())
                {
                    Pointer.transform.position = tutor.GetPositionForMapPointer();
                }
            }
            else
            {
                Pointer.SetActive(false);
            }

            if (step.ShouldBoxBeShown())
            {
                RevealSelf();
            }
            else
            {
                ObscureSelf();
            }


            ContinueButton.SetActive(step.GetControlsForStep().CheckBooleanControlPermission(UniversalConstants.BooleanControls.ContinueButton));

            if (Highlighter != null)
            {
                if (step.GetHighlighted() != null)
                {
                    RectTransform highlight = step.GetHighlighted();

                    Highlighter.SetActive(true);

                    RectTransform ink = Highlighter.GetComponent<RectTransform>();

                    ink.pivot = highlight.pivot;



                    //ink.anchoredPosition = highlight.anchoredPosition;
                    //ink.anchorMax = highlight.anchorMax;
                    //ink.anchorMin = highlight.anchorMin;


                    ink.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, highlight.rect.width);
                    ink.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, highlight.rect.height);

                    //ink.offsetMin = highlight.offsetMin;
                    //ink.offsetMax = highlight.offsetMax;

                    ink.position = highlight.position;

                    

                    //ink.sizeDelta = highlight.sizeDelta;


                }
                else
                {
                    Highlighter.SetActive(false);
                }
            }
        }
        else
        {
            //Being sent null indicates there are no more steps so the dialogue box should hide
            ObscureSelf();
            if(Highlighter != null)
            {
                Highlighter.SetActive(false);
            }
            
            Pointer.SetActive(false);
        }

        
        
        

    }

    public void ObscureSelf()
    {
        

        Rect rec = gameObject.GetComponent<RectTransform>().rect;

        gameObject.transform.position = new Vector3(transform.position.x, Screen.height + HIDE_DISTANCE + rec.height, transform.position.z);
        Debug.Log("Dialogue Hidden");

    }


    public void RevealSelf()
    {
        Rect rec = gameObject.GetComponent<RectTransform>().rect;

        gameObject.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        Debug.Log("Dialogue Shown");
    }

    public void ContinuePressed()
    {
        Director.GoToNextStep();
    }


    
}
