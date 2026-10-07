using UnityEngine;

public enum Season
{
    Dry,
    Rainy
}

// Un tipo de clima para un canal, un rango de valores y su peso por estación (Próximamente hacer esa bruma roja que se forma en las estaciones seecas).
[System.Serializable]
public class WeatherState
{
    public string stateName;
    [Range(0f, 1f)] public float minValue;
    [Range(0f, 1f)] public float maxValue;
    [Min(0f)] public float dryWeight;
    [Min(0f)] public float rainyWeight;

    public WeatherState(string stateName, float minValue, float maxValue, float dryWeight, float rainyWeight)
    {
        this.stateName = stateName;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.dryWeight = dryWeight;
        this.rainyWeight = rainyWeight;
    }
}

// Decide cuándo hay nubes y niebla. Cada canal elige un objetivo nuevo cada cierto tiempo aleatorio. Nubes y niebla son independientes.
public class WeatherController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CloudCoverageDriver cloudDriver;
    [SerializeField] private FogDriver fogDriver;

    [Header("General")]
    [SerializeField] private bool automatic = true;
    [SerializeField] private Season season = Season.Dry;
    [SerializeField] private bool useFixedSeed = false;
    [SerializeField] private int seed = 12345;
    [SerializeField] private bool logChanges = false;

    [Header("Nubes")]
    [Tooltip("Segundos mínimo y máximo que dura cada objetivo antes de elegir otro")]
    [SerializeField] private Vector2 cloudHoldTime = new Vector2(60f, 240f);
    [SerializeField] private float cloudTransitionTime = 40f;
    [SerializeField]
    private WeatherState[] cloudStates =
    {
        new WeatherState("Despejado", 0f, 0.1f, 0.45f, 0.10f),
        new WeatherState("Parcial", 0.25f, 0.55f, 0.35f, 0.30f),
        new WeatherState("Cubierto", 0.75f, 1f, 0.20f, 0.60f)
    };

    [Header("Niebla")]
    [SerializeField] private Vector2 fogHoldTime = new Vector2(90f, 300f);
    [SerializeField] private float fogTransitionTime = 60f;
    [SerializeField]
    private WeatherState[] fogStates =
    {
        new WeatherState("Sin niebla", 0f, 0.05f, 0.55f, 0.35f),
        new WeatherState("Neblina", 0.12f, 0.35f, 0.30f, 0.35f),
        new WeatherState("Niebla espesa", 0.55f, 1f, 0.15f, 0.30f)
    };

    [Header("Bruma base (polvo y humo en el aire)")]
    [SerializeField] private float dryBaseDensity = 0.004f;
    [SerializeField] private float rainyBaseDensity = 0.002f;
    [SerializeField] private float baseDensityTransitionTime = 20f;

    [Header("Estado actual (solo para ver, se sobrescribe)")]
    [SerializeField, Range(0f, 1f)] private float currentCloud;
    [SerializeField, Range(0f, 1f)] private float currentFog;
    [SerializeField] private float currentBaseDensity;

    private class WeatherChannel
    {
        public float current;
        public float target;
        public float velocity;
        public float timer;
        public WeatherState lastState;
    }

    private readonly WeatherChannel cloud = new WeatherChannel();
    private readonly WeatherChannel fog = new WeatherChannel();
    private float baseDensityVelocity;
    private System.Random rng;

    public float CurrentCloud => cloud.current;
    public float CurrentFog => fog.current;
    public Season CurrentSeason => season;

    //Dejaré esto público con al intención de vincularlo a eventos del juego.
    // Fuerza un clima concreto durante holdSeconds. Después, si automatic está activo, el sistema retoma solo.
    public void ForceWeather(float cloudCoverage, float fogAmount, float holdSeconds = 120f)
    {
        cloud.target = Mathf.Clamp01(cloudCoverage);
        cloud.timer = holdSeconds;
        cloud.lastState = null;

        fog.target = Mathf.Clamp01(fogAmount);
        fog.timer = holdSeconds;
        fog.lastState = null;
    }

    public void SetSeason(Season newSeason, bool rerollNow = false)
    {
        season = newSeason;
        if (!rerollNow) return;

        PickNext(cloud, cloudStates, cloudHoldTime, "Nubes");
        PickNext(fog, fogStates, fogHoldTime, "Niebla");
    }

    public void SetAutomatic(bool value)
    {
        automatic = value;
    }

    // Toda la lógica de estos cambios

    private void Awake()
    {
        rng = useFixedSeed ? new System.Random(seed) : new System.Random();
    }

    private void Start()
    {
        if (cloudDriver == null) Debug.LogWarning("WeatherController: falta asignar CloudCoverageDriver.");
        if (fogDriver == null) Debug.LogWarning("WeatherController: falta asignar FogDriver.");

        // Al empezar el juego no hay transición: se arranca directamente en el primer clima elegido
        PickNext(cloud, cloudStates, cloudHoldTime, "Nubes");
        cloud.current = cloud.target;

        PickNext(fog, fogStates, fogHoldTime, "Niebla");
        fog.current = fog.target;

        currentBaseDensity = GetBaseDensityTarget();
        ApplyToDrivers();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        UpdateChannel(cloud, cloudStates, cloudHoldTime, cloudTransitionTime, "Nubes", deltaTime);
        UpdateChannel(fog, fogStates, fogHoldTime, fogTransitionTime, "Niebla", deltaTime);

        currentBaseDensity = Mathf.SmoothDamp(
            currentBaseDensity, GetBaseDensityTarget(), ref baseDensityVelocity,
            Mathf.Max(baseDensityTransitionTime, 0.01f), Mathf.Infinity, deltaTime);

        ApplyToDrivers();

        currentCloud = cloud.current;
        currentFog = fog.current;
    }

    private void UpdateChannel(WeatherChannel channel, WeatherState[] states, Vector2 holdTime,
        float transitionTime, string label, float deltaTime)
    {
        if (automatic)
        {
            channel.timer -= deltaTime;
            if (channel.timer <= 0f) PickNext(channel, states, holdTime, label);
        }

        channel.current = Mathf.SmoothDamp(
            channel.current, channel.target, ref channel.velocity,
            Mathf.Max(transitionTime, 0.01f), Mathf.Infinity, deltaTime);
        channel.current = Mathf.Clamp01(channel.current);
    }

    private void PickNext(WeatherChannel channel, WeatherState[] states, Vector2 holdTime, string label)
    {
        WeatherState state = PickState(states, channel.lastState);
        if (state == null) return;

        channel.lastState = state;
        channel.target = NextFloat(state.minValue, state.maxValue);
        channel.timer = NextFloat(holdTime.x, holdTime.y);

        if (logChanges)
        {
            Debug.Log($"Weather [{label}] -> {state.stateName} (objetivo {channel.target:F2}, dura {channel.timer:F0}s)");
        }
    }

    // Elección por pesos según la estación. Intenta no repetir el mismo estado dos veces seguidas.
    private WeatherState PickState(WeatherState[] states, WeatherState previous)
    {
        if (states == null || states.Length == 0) return null;

        WeatherState picked = null;
        for (int attempt = 0; attempt < 3; attempt++)
        {
            picked = RollState(states);
            if (picked != previous || states.Length == 1) break;
        }
        return picked;
    }

    private WeatherState RollState(WeatherState[] states)
    {
        float total = 0f;
        for (int i = 0; i < states.Length; i++) total += GetWeight(states[i]);
        if (total <= 0f) return states[0];

        float roll = NextFloat(0f, total);
        for (int i = 0; i < states.Length; i++)
        {
            roll -= GetWeight(states[i]);
            if (roll <= 0f) return states[i];
        }
        return states[states.Length - 1];
    }

    private float GetWeight(WeatherState state)
    {
        return season == Season.Dry ? state.dryWeight : state.rainyWeight;
    }

    private float GetBaseDensityTarget()
    {
        return season == Season.Dry ? dryBaseDensity : rainyBaseDensity;
    }

    private float NextFloat(float a, float b)
    {
        float min = Mathf.Min(a, b);
        float max = Mathf.Max(a, b);
        return min + (float)rng.NextDouble() * (max - min);
    }

    private void ApplyToDrivers()
    {
        if (cloudDriver != null) cloudDriver.SetCloudCoverage(cloud.current);

        if (fogDriver != null)
        {
            // Primero la bruma base: SetFogAmount aplica ambos valores a la vez
            fogDriver.SetBaseDensity(currentBaseDensity);
            fogDriver.SetFogAmount(fog.current);
        }
    }
}