using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public bool IsPlay;
    public bool IsOptions;
    public bool IsQuit;

    private Animation anim;

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
        if (IsPlay)
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
