using UnityEngine;

// Por ahora se controla a mano con el slider; más adelante el weatherController va a llamar a SetCloudCoverage().
[ExecuteAlways]
public class CloudCoverageDriver : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float cloudCoverage = 0.4f;

    private static readonly int cloudCoverageId = Shader.PropertyToID("_CloudCoverage");

    public void SetCloudCoverage(float value)
    {
        cloudCoverage = Mathf.Clamp01(value);
        ApplyCoverage();
    }

    public float GetCloudCoverage()
    {
        return cloudCoverage;
    }

    private void ApplyCoverage()
    {
        Shader.SetGlobalFloat(cloudCoverageId, cloudCoverage);
    }

    private void OnEnable()
    {
        ApplyCoverage();
    }

    private void Update()
    {
        ApplyCoverage();
    }

    private void OnValidate()
    {
        ApplyCoverage();
    }
}