using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    public Slider healthBar;          // Arrastra tu Slider de UI aquí

    [Header("Invincibility Frames")]
    public float invincibilityTime = 0.5f;   // Segundos de invulnerabilidad tras recibir daño
    private float lastDamageTime = -999f;

    [Header("Death")]
    public float respawnDelay = 3f;
    public Transform respawnPoint;    // Punto donde reaparece el jugador (opcional)

    private bool isDead = false;
    private Animator anim;
    private Image DamageFlash => MainScene.MainCanvas.HUD.DamageFlash;
    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        // Invincibility frames
        if (Time.time - lastDamageTime < invincibilityTime) return;
        lastDamageTime = Time.time;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthBar();
        SoundManager.Instance?.PlaySFX(SFXType.PlayerDamage);
        if (anim != null)
            anim.SetTrigger("Hit");

        if (DamageFlash != null)
            StartCoroutine(FlashDamage());

        if (currentHealth <= 0f)
            Die();
    }

    public bool Heal(float amount)
    {
        if (isDead) { return false; }
        if (currentHealth >= maxHealth) { return false; }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthBar();
        return true;
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.value = currentHealth;
    }

    System.Collections.IEnumerator FlashDamage()
    {
        Color c = DamageFlash.color;
        DamageFlash.color = new Color(c.r, c.g, c.b, 0.4f);
        yield return new WaitForSeconds(0.1f);
        DamageFlash.color = new Color(c.r, c.g, c.b, 0f);
    }

    void Die()
    {
        isDead = true;

        if (anim != null)
            anim.SetTrigger("Die");

        // Desactiva controles del jugador

        if (MainGame.Instance != null && respawnPoint == null)
        {
            MainGame.OnDeath();
        }
        else
        {
            MonoBehaviour controller = GetComponent<FirstPersonController>();
            controller.enabled = false;
        }

        Invoke(nameof(Respawn), respawnDelay);
    }

    void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        UpdateHealthBar();

        if (respawnPoint != null)
            transform.position = respawnPoint.position;

        if (anim != null)
            anim.SetTrigger("Respawn");
    }

    public bool IsDead() => isDead;
    public float GetHealthPercent() => currentHealth / maxHealth;
}