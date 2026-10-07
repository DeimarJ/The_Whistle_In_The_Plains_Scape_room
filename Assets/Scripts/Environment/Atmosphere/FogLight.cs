using System.Collections.Generic;
using UnityEngine;

// El pase de niebla toma las primeras 4 luces activas.
[ExecuteAlways]
public class FogLight : MonoBehaviour
{
    public static readonly List<FogLight> activeLights = new List<FogLight>();

    [SerializeField] private Light sourceLight;
    [SerializeField] private float fogIntensity = 1f;
    [SerializeField, Range(0f, 1f)] private float flicker = 0.15f;
    [SerializeField] private float flickerSpeed = 3f;

    private float seed;

    public Vector4 GetPositionRange()
    {
        Vector3 position = transform.position;
        float range = sourceLight != null ? sourceLight.range : 10f;
        return new Vector4(position.x, position.y, position.z, range);
    }

    public Vector4 GetColor()
    {
        Color baseColor = sourceLight != null ? sourceLight.color.linear : Color.white;
        float baseIntensity = sourceLight != null ? sourceLight.intensity : 1f;

        float flickerFactor = 1f - flicker * Mathf.PerlinNoise(seed, Time.time * flickerSpeed);
        float k = baseIntensity * fogIntensity * flickerFactor;

        return new Vector4(baseColor.r * k, baseColor.g * k, baseColor.b * k, 0f);
    }

    private void Reset()
    {
        sourceLight = GetComponent<Light>();
    }

    private void OnEnable()
    {
        seed = Random.value * 100f;
        if (!activeLights.Contains(this)) activeLights.Add(this);
    }

    private void OnDisable()
    {
        activeLights.Remove(this);
    }
}