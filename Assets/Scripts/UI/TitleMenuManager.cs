using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TitleMenuManager : MonoBehaviour
{

    private Vector2 BACKGROUNDRESOLUTION = new Vector2(2048f, 2048f);

    [SerializeField] private GameObject exitButton;
    [SerializeField] private RectTransform Background;
    [SerializeField] private float speed = 64f;
    [SerializeField] private Camera cam;

    private Vector3 direction;
    private Vector3 origin;
    private Vector2 movableSpace;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_WEBGL
    exitButton.SetActive(false);
#endif
        direction = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0.0f);
        direction.Normalize();
        movableSpace = new Vector2((BACKGROUNDRESOLUTION.x - Screen.width) * 0.5f - speed, (BACKGROUNDRESOLUTION.y - Screen.height) * 0.5f - speed);
        origin = Background.position;

    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 change = speed * direction * Time.deltaTime;
        
        Vector3 pos = Background.position + change;

        

        //Check if we need to bounce
        if(pos.x < origin.x - movableSpace.x || pos.x > origin.x + movableSpace.x)
        {
            //bounce against a side
            direction.x *= -1;
            change.x *= -1;
        }
        if(pos.y < origin.y - movableSpace.y || pos.y > origin.y + movableSpace.y)
        {
            //bounce against the top or bottom
            direction.y *= -1;
            change.y *= -1;
        }
        Background.position += change;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowInstructions()
    {
        SceneManager.LoadScene(2);
    }

    public void ShowCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#endif
        Application.Quit();
    }
}
