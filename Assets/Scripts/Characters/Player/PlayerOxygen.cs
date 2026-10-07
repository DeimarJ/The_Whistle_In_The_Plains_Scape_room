using UnityEngine;
using UnityEngine.UI;

public class PlayerOxygen : MonoBehaviour
{
    [Header("References")]
    private PlayerHealth playerHealth;
    private FirstPersonController player;
    private Slider OxygenBar => MainScene.MainCanvas.HUD.OxygenBar;

    [Header("Parameters")]
    [SerializeField] private float oxygenDecreaseRate = 10f; // unidades por segundo
    [SerializeField] private float oxygenIncreaseRate = 15f; // unidades por segundo
    [SerializeField] private float damageInterval = 1f;      // cada cuánto tiempo quita vida
    [SerializeField] private int damageAmount = 5;
    public float maxOxygen = 100f;
    private float currentOxygen;
    private float damageTimer;

    void Start()
    {
        player = GetComponent<FirstPersonController>();
        playerHealth = GetComponent<PlayerHealth>();
        player.OnSwimmingStateChanged += HandleSwimmingStateChanged;
        currentOxygen = maxOxygen;
        UpdateOxygenBar();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.GetIsSwimming())
        {
            // Decrece oxígeno
            currentOxygen -= oxygenDecreaseRate * Time.deltaTime;
            currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);

            // Si oxígeno llega a 0, empieza a quitar vida
            if (currentOxygen <= 0)
            {
                damageTimer += Time.deltaTime;
                if (damageTimer >= damageInterval)
                {
                    playerHealth.TakeDamage(damageAmount);
                    damageTimer = 0f;
                }
            }

            
        }
        else
        {
            // Regenera oxígeno
            currentOxygen += oxygenIncreaseRate * Time.deltaTime;
            currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);
            damageTimer = 0f; // reset del contador de daño
        }

        UpdateOxygenBar();
    }

    private void HandleSwimmingStateChanged(bool isSwimming)
    {
        if (OxygenBar != null)
            OxygenBar.gameObject.SetActive(isSwimming);
        else
            Debug.LogWarning("OxygenBar no está asignado");
    }

    private void OnDestroy()
    {
        player.OnSwimmingStateChanged -= HandleSwimmingStateChanged;
    }

    void UpdateOxygenBar()
    {
        if (OxygenBar != null)
        {
            if (OxygenBar.value != currentOxygen)
            {
                OxygenBar.value = currentOxygen;
            }
        }
    }
}
