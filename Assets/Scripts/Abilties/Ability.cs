using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Ability : MonoBehaviour
{
    public float CoolDown;
    public string Button;
    public int PlayerNumber;

    float currentCoolDown;

    public Slider coolDownSlider;

    protected Player player;

    public bool OnCoolDown
    {
        get
        {
            return onCoolDown;
        }
    }

    private bool onCoolDown = false;

    private void Start()
    {
        /*
        player = InputAssign.GetPlayer(PlayerNumber);
        if(player == null)
        {
            Debug.LogError("Could not find player with given player number!");
        }
        */
    }

    private void Update()
    {
        /*
        if (!onCoolDown)
        {
            if (player != null)
            {
                PlayerInput input = player.ControllerInput;
                if (input != null)
                {
                    string buttonString = input.Button(Button);
                    if (Input.GetButtonDown(buttonString) || Input.GetButton(buttonString))
                    {
                        Use();
                    }
                }
            }
        }

        if(onCoolDown)
        {
            currentCoolDown += Time.deltaTime;
            coolDownSlider.value = coolDownSlider.maxValue - currentCoolDown / CoolDown;
        }
        */
        DerivedUpdate();
    }

    protected abstract void DerivedUpdate();

    public void Use()
    {
        Function();
        EnableCoolDown();
    }

    private void EnableCoolDown()
    {
        StartCoroutine(CoolDownTimer());
    }

    IEnumerator CoolDownTimer()
    {
        SetCoolDownUI();
        onCoolDown = true;
        yield return new WaitForSeconds(CoolDown);
        onCoolDown = false;
        SetEnableUI();
    }

    private void SetEnableUI()
    {
        coolDownSlider.value = 0;
        currentCoolDown = 0;
    }

    private void SetCoolDownUI()
    {
        coolDownSlider.value = 0;
    }

    protected abstract void Function();
}
