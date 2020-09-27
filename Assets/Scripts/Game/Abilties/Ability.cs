using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class Ability : MonoBehaviour
{
    public float Cooldown = 0.1f;
    public string Button;
    public int PlayerNumber;
    public Slider CooldownSlider;
    public bool AbilityOnCooldown { get; private set; } = false;

    public Player player;
    private float timeAccumulator = 0.0f;

    private void Start()
    {
        /*
        player = InputAssign.GetPlayer(PlayerNumber);
        if (player == null)
        {
            Debug.LogError("Could not find player with given player number!");
        }
        */
        DerivedStart();
    }


    private void Update()
    {
        /*
        if (!AbilityOnCooldown && player != null && player.ControllerInput != null)
        {
            string buttonString = player.ControllerInput.Button(Button);
            if (Input.GetButtonDown(buttonString) || Input.GetButton(buttonString))
            {
                StartAbility();
            }
        }
        */

        if (AbilityOnCooldown && Cooldown != 0.0f)
        {
            timeAccumulator += Time.deltaTime;
            CooldownSlider.value = Mathf.Lerp(CooldownSlider.maxValue, CooldownSlider.minValue, timeAccumulator / Cooldown);
        }

        DerivedUpdate();
    }


    public void StartAbility(Player player)
    {
        if(AbilityOnCooldown)
        {
            return;
        }

        this.player = player;
        UseAbility(player);
        StartCoolDown();
    }


    private void StartCoolDown()
    {
        StartCoroutine(CoolDownTimer());
    }


    IEnumerator CoolDownTimer()
    {
        AbilityOnCooldown = true;
        timeAccumulator = 0;
        CooldownSlider.value = CooldownSlider.maxValue;
        yield return new WaitForSeconds(Cooldown);
        AbilityOnCooldown = false;
        CooldownSlider.value = CooldownSlider.minValue;
    }


    protected abstract void UseAbility(Player player);
    protected abstract void DerivedStart();
    protected abstract void DerivedUpdate();
}
