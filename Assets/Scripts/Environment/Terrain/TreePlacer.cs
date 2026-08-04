using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PASO 2 — Coloca árboles automáticamente sobre el Terrain según la
/// clasificación de zonas generada por TerrainZoneClassifier.
///
/// Requiere que TerrainZoneClassifier ya haya corrido "Clasificar Terreno"
/// al menos una vez (si no, este script lo ejecuta automáticamente).
/// </summary>
[RequireComponent(typeof(Terrain))]
public class TreePlacer : MonoBehaviour
{
    [Header("Referencias")]
    public TerrainZoneClassifier classifier;

    [Header("Espaciado base")]
    [Tooltip("Distancia promedio (metros) entre posibles puntos de plantado. " +
             "Menor = más árboles evaluados (más denso posible, más lento).")]
    public float baseSpacingMeters = 10f;

    [Tooltip("Jitter aleatorio dentro de cada celda (0 = grilla perfecta, 1 = celda completa). " +
             "Recomendado 0.8-1.0 para que no se note el patrón de rejilla.")]
    [Range(0f, 1f)] public float positionJitter = 0.9f;

    [Header("Prototipos por rol (índices del Terrain Tree Prototypes)")]
    [Tooltip("Índices de Melia azedarach (finca/banco). Se alterna al azar entre estos.")]
    public int[] meliaPrototypeIndices = { 0, 1 };

    [Tooltip("Índice del Ficus (ribera / bosque de galería).")]
    public int ficusPrototypeIndex = 2;

    [Header("Densidad por zona (0 = nada, 1 = máxima)")]
    [Range(0f, 1f)] public float densityPendiente = 0.55f; // ribera -> Ficus
    [Range(0f, 1f)] public float densityBanco = 0.18f;     // banco -> Melia (más disperso)
    [Range(0f, 1f)] public float densityBajio = 0f;        // estero -> sin árboles
    [Range(0f, 1f)] public float densitySabana = 0f;       // sabana abierta -> sin árboles

    [Header("Ruido para evitar patrón de rejilla")]
    [Tooltip("Escala del ruido Perlin que modula la densidad (menor = parches más grandes).")]
    public float noiseScale = 0.02f;

    [Header("Variación visual")]
    public Vector2 scaleRange = new Vector2(0.85f, 1.25f);
    public bool randomizeRotation = true;

    [Header("Opciones")]
    [Tooltip("Si está activo y el terreno no fue clasificado aún, lo clasifica automáticamente.")]
    public bool autoClassifyIfNeeded = true;

    private Terrain _terrain;
    private TerrainData _data;

    [ContextMenu("Plantar Árboles")]
    public void PlaceTrees()
    {
        _terrain = GetComponent<Terrain>();
        _data = _terrain.terrainData;

        if (classifier == null)
        {
            classifier = GetComponent<TerrainZoneClassifier>();
            if (classifier == null)
            {
                Debug.LogError("No se encontró un TerrainZoneClassifier en este Terrain. Agregalo primero.");
                return;
            }
        }

        if (classifier.ZoneMap == null)
        {
            if (autoClassifyIfNeeded)
            {
                Debug.Log("El terreno no estaba clasificado. Ejecutando ClassifyTerrain() automáticamente...");
                classifier.ClassifyTerrain();
            }
            else
            {
                Debug.LogError("El terreno no está clasificado. Corré 'Clasificar Terreno' primero.");
                return;
            }
        }

        int zoneRes = classifier.ZoneMap.GetLength(0); // resolución del heightmap (ej. 1025)
        float worldWidth = _data.size.x;
        float worldLength = _data.size.z;

        var newInstances = new List<TreeInstance>();
        int seedCounter = 0;

        // Recorremos el mundo en celdas de baseSpacingMeters, con jitter para romper la rejilla
        for (float worldX = 0f; worldX < worldWidth; worldX += baseSpacingMeters)
        {
            for (float worldZ = 0f; worldZ < worldLength; worldZ += baseSpacingMeters)
            {
                seedCounter++;

                // Jitter dentro de la celda
                float jitterX = Random.Range(-positionJitter, positionJitter) * baseSpacingMeters * 0.5f;
                float jitterZ = Random.Range(-positionJitter, positionJitter) * baseSpacingMeters * 0.5f;
                float sampleX = Mathf.Clamp(worldX + jitterX, 0f, worldWidth - 0.01f);
                float sampleZ = Mathf.Clamp(worldZ + jitterZ, 0f, worldLength - 0.01f);

                // Coordenadas normalizadas 0-1 (las que usa Unity para Terrain)
                float normX = sampleX / worldWidth;
                float normZ = sampleZ / worldLength;

                // Buscar la zona correspondiente en el ZoneMap
                int zx = Mathf.Clamp(Mathf.RoundToInt(normX * (zoneRes - 1)), 0, zoneRes - 1);
                int zz = Mathf.Clamp(Mathf.RoundToInt(normZ * (zoneRes - 1)), 0, zoneRes - 1);
                var zone = classifier.ZoneMap[zz, zx]; // ZoneMap está indexado [z, x]

                float density = zone switch
                {
                    TerrainZoneClassifier.ZoneType.Pendiente => densityPendiente,
                    TerrainZoneClassifier.ZoneType.BancoElevado => densityBanco,
                    TerrainZoneClassifier.ZoneType.BajioEstero => densityBajio,
                    _ => densitySabana
                };

                if (density <= 0f) continue;

                // Ruido Perlin para modular densidad en parches orgánicos (no uniforme)
                float noise = Mathf.PerlinNoise(sampleX * noiseScale, sampleZ * noiseScale);
                float finalProbability = density * noise;

                if (Random.value > finalProbability) continue;

                // Elegir prototipo según la zona
                int prototypeIndex = zone == TerrainZoneClassifier.ZoneType.Pendiente
                    ? ficusPrototypeIndex
                    : meliaPrototypeIndices[Random.Range(0, meliaPrototypeIndices.Length)];

                float scale = Random.Range(scaleRange.x, scaleRange.y);
                float rotation = randomizeRotation ? Random.Range(0f, Mathf.PI * 2f) : 0f;

                var instance = new TreeInstance
                {
                    position = new Vector3(normX, 0f, normZ), // Unity ignora Y, ajusta a la altura del terreno solo
                    prototypeIndex = prototypeIndex,
                    widthScale = scale,
                    heightScale = scale,
                    rotation = rotation,
                    color = Color.white,
                    lightmapColor = Color.white
                };

                newInstances.Add(instance);
            }
        }

        _data.SetTreeInstances(newInstances.ToArray(), true);
        Debug.Log($"Plantado completo: {newInstances.Count} árboles colocados " +
                   $"(evaluados {seedCounter} puntos de grilla con jitter {positionJitter}).");
    }

    [ContextMenu("Borrar Todos los Árboles")]
    public void ClearAllTrees()
    {
        _terrain = GetComponent<Terrain>();
        _data = _terrain.terrainData;
        _data.SetTreeInstances(new TreeInstance[0], true);
        Debug.Log("Todos los árboles fueron eliminados del terreno.");
    }
}
