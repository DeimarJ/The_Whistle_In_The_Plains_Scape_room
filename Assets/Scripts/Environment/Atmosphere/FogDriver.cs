using UnityEngine;

//Por ahora se maneja con el slider, luego el WeatherController llamará a SetFogAmount().
[ExecuteAlways]
public class FogDriver : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float fogAmount = 0.3f;
    [SerializeField] private float maxFogDensity = 0.04f;
    [SerializeField] private Color fogColor = new Color(0.025f, 0.03f, 0.038f, 1f);

    private static readonly int skyFogAmountId = Shader.PropertyToID("_SkyFogAmount");
    private static readonly int skyFogColorId = Shader.PropertyToID("_SkyFogColor");

    [SerializeField] private float maxVolumetricDensity = 0.05f;
    [SerializeField] private bool useDistanceFog = false;
    private static readonly int volFogDensityId = Shader.PropertyToID("_VolFogDensity");

    [SerializeField] private float baseDensity = 0.003f;

    public void SetFogAmount(float value)
    {
        fogAmount = Mathf.Clamp01(value);
        ApplyFog();
    }

    public float GetFogAmount()
    {
        return fogAmount;
    }

    private void ApplyFog()
    {
        RenderSettings.fog = useDistanceFog && fogAmount > 0.001f;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        // Curva cuadrática: los valores bajos del slider dan neblina suave, los altos niebla espesa
        RenderSettings.fogDensity = fogAmount * fogAmount * maxFogDensity;
        RenderSettings.fogColor = fogColor;

        Shader.SetGlobalFloat(skyFogAmountId, fogAmount);
        Shader.SetGlobalColor(skyFogColorId, fogColor);
        Shader.SetGlobalFloat(volFogDensityId, baseDensity + fogAmount * fogAmount * maxVolumetricDensity);
    }

    private void OnEnable()
    {
        ApplyFog();
    }

    private void OnValidate()
    {
        ApplyFog();
    }

    private void Update()
    {
        if (Application.isPlaying) ApplyFog();
    }

    public void SetBaseDensity(float value)
    {
        baseDensity = Mathf.Max(0f, value);
    }
}