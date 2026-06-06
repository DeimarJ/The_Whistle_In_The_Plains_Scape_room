using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Lantern : GrabbableObject
{
    [Header("Light")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private GameObject lightVisuals;
    [Header("Initial State")]
    [SerializeField] private bool startOn = false;

    [Header("Oil")]
    [SerializeField] private float maxOil = 100f;

    [SerializeField] private float currentOil = 100f;

    [SerializeField] private float oilConsumptionRate = 1f;

    [Header("Low Oil")]
    [SerializeField] private float lowOilThreshold = 20f;

    [SerializeField] private float flickerSpeed = 8f;

    [SerializeField] private float minLowIntensity = 0.3f;

    [SerializeField] private float maxIntensity = 1f;

    private bool isOn;

    private void Start()
    {
        if (startOn && currentOil > 0f)
        {
            TurnOn();
        }
        else
        {
            TurnOff();
        }
    }

    private void Update()
    {
        if (!isOn)
        {
            return;
        }

        ConsumeOil();

        UpdateLowOilEffects();
    }

    public void ToggleLantern()
    {
        if (isOn)
        {
            TurnOff();
            return;
        }

        if (currentOil <= 0f)
        {
            return;
        }

        TurnOn();
    }

    private void TurnOn()
    {
        isOn = true;

        lanternLight.enabled = true;
        lightVisuals.SetActive(true);
        SoundManager.Instance?.PlaySFX(SFXType.LanternOn);
    }

    private void TurnOff()
    {
        isOn = false;

        lanternLight.enabled = false;
        lightVisuals.SetActive(false);
        SoundManager.Instance?.PlaySFX(SFXType.LanternOff);
    }

    private void ConsumeOil()
    {
        currentOil -= oilConsumptionRate * Time.deltaTime;

        currentOil = Mathf.Max(currentOil, 0f);

        if (currentOil <= 0f)
        {

            SoundManager.Instance?.PlaySFX(SFXType.OilOver);
            TurnOff();
        }
    }

    private void UpdateLowOilEffects()
    {
        if (currentOil > lowOilThreshold)
        {
            lanternLight.intensity = maxIntensity;
            return;
        }

        float normalized =
            currentOil / lowOilThreshold;

        float targetIntensity =
            Mathf.Lerp(minLowIntensity, maxIntensity, normalized);

        float flicker =
            Mathf.Sin(Time.time * flickerSpeed) * 0.15f;

        lanternLight.intensity =
            targetIntensity + flicker;
    }

    public bool RefillOil(float amount)
    {
        if (currentOil >= maxOil)
        {
            return false;
        }

        currentOil += amount;

        currentOil = Mathf.Min(currentOil, maxOil);


        SoundManager.Instance?.PlaySFX(SFXType.RefillOil);
        return true;
    }

    public float OilPercent =>
        currentOil / maxOil;

    public bool IsOn => isOn;
}