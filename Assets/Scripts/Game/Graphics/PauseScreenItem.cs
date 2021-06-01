using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseScreenItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public bool IsResume;
    public bool IsOptions;
    public bool IsQuit;

    private Animation anim;

    public delegate void RestartScene();
    public static event RestartScene OnRestartScene;

    void Awake()
    {
        anim = GetComponent<Animation>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        anim.Play("MenuItemSlideForward");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        anim.Play("MenuItemSlideReverse");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsResume)
        {
            SceneManager.LoadScene("Level1");
        }
        else if (IsOptions)
        {

        }
        else if (IsQuit)
        {
            Application.Quit();
        }
        else
        {
            Debug.LogError("Error. This button is not registered in MainMenuItem. Button text: " + GetComponent<Text>().text);
        }
    }
}
