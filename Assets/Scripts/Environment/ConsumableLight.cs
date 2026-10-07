using UnityEngine;

public class ConsumableLight : MonoBehaviour
{
    Light candlelight;
    [SerializeField] private float initialIntensity;
    [SerializeField] private float duration;
    [SerializeField] private float timeRemaining;

    [SerializeField] private float flickerStrength = 0.2f;
    [SerializeField] private float flickerSpeed = 10f;

    [SerializeField] private float swayAmount = 0.05f;
    [SerializeField] private float swaySpeed = 2f;

    private Vector3 initialPosition;

    void Start()
    {
        candlelight = GetComponent<Light>();

        initialIntensity = candlelight.intensity;

        duration = Random.Range(210f, 300f);

        timeRemaining = duration;

        initialPosition = transform.localPosition;

        float swayX = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        float swayY = Mathf.Cos(Time.time * swaySpeed * 0.5f) * swayAmount;
        transform.localPosition = initialPosition + new Vector3(swayX, swayY, 0f);
    }


    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            float normalizedTime = timeRemaining / duration;

            candlelight.intensity = initialIntensity * normalizedTime;

            float intensityBase = initialIntensity * normalizedTime;

            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
            candlelight.intensity = intensityBase + (noise - 0.5f) * flickerStrength;
        }
        else
        {
            candlelight.intensity = 0f;
        }
    }

    public void ReduceTime(float percent)
    {
        timeRemaining -= timeRemaining * percent;
    }
}
