using System.IO;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainZoneClassifier : MonoBehaviour
{
    public enum ZoneType { SabanaPlana, BancoElevado, BajioEstero, Pendiente }

    [Header("Radio de análisis local en meetros")]
    [Tooltip("Radio usado para calcular la altura promedio del entorno. 50-100m funciona bien para distinguir bancos de esteros en llanos.")]

    public float radiusMeters = 70f;

    [Header("Pre-filtrado de ruido del DEM")]
    [Tooltip("Radio pequeño (píxeles) para suavizar ruido de sensor antes de comparar contra el promedio local. 3-5 suele bastar. Poné 0 para desactivar.")]
    public int noisePreSmoothRadius = 4;

    [Header("Umbrales de clasificación")]
    [Tooltip("Diferencia mínima (metros) respecto al promedio local para considerarse Banco o Bajío.")]
    public float heightThreshold = 0.6f;

    [Tooltip("Pendiente (grados) a partir de la cual se considera zona de barranco/orilla de caño.")]
    public float slopeThresholdDegrees = 20f;

    [Header("Debug visual")]
    public bool generateDebugTexture = true;

    [Tooltip("Si está activo, guarda automáticamente la textura de debug como PNG al clasificar.")]
    public bool exportDebugPNG = true;

    [Tooltip("Ruta relativa (dentro de Assets/) donde se guarda el PNG de debug.")]
    public string debugPNGPath = "Assets/TerrainDebug/zone_classification.png";

    private Terrain _terrain;
    private TerrainData _data;

    // Resultado: una zona por cada punto del heightmap
    public ZoneType[,] ZoneMap { get; private set; }
    public Texture2D DebugTexture { get; private set; }

    void Reset()
    {
        _terrain = GetComponent<Terrain>();
    }

    [ContextMenu("Clasificar Terreno")]
    public void ClassifyTerrain()
    {
        _terrain = GetComponent<Terrain>();
        _data = _terrain.terrainData;

        int res = _data.heightmapResolution;
        float[,] heights = _data.GetHeights(0, 0, res, res); // valores 0-1 normalizados
        float terrainHeight = _data.size.y; // altura máxima real en metros
        float worldWidth = _data.size.x; // ancho real en metros
        float worldLength = _data.size.z;

        // metros que representa cada píxel del heightmap
        float metersPerPixelX = worldWidth / (res - 1);
        float metersPerPixelZ = worldLength / (res - 1);

        // convertir alturas normalizadas (0-1) a metros reales
        float[,] heightsMeters = new float[res, res];
        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
                heightsMeters[z, x] = heights[z, x] * terrainHeight;

        // Pre-suaviza para eliminar ruido de sensor de alta frecuencia
        // (radio pequeño en píxeles, no en metros — actúa como un "denoise" local)
        float[,] heightsSmoothed = noisePreSmoothRadius > 0
            ? BoxBlur(heightsMeters, res, noisePreSmoothRadius, noisePreSmoothRadius)
            : heightsMeters;

        //Box blur separable para obtener el promedio local (sobre los datos YA suavizados)
        int radiusPixelsX = Mathf.Max(1, Mathf.RoundToInt(radiusMeters / metersPerPixelX));
        int radiusPixelsZ = Mathf.Max(1, Mathf.RoundToInt(radiusMeters / metersPerPixelZ));

        float[,] localAverage = BoxBlur(heightsSmoothed, res, radiusPixelsX, radiusPixelsZ);

        //Clasificación
        ZoneMap = new ZoneType[res, res];
        Color[] debugPixels = generateDebugTexture ? new Color[res * res] : null;

        // contadores para saber la proporción de cada zona (útil para ajustar thresholds)
        int countSabana = 0, countBanco = 0, countBajio = 0, countPendiente = 0;

        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                // diff se calcula sobre datos suavizados, no crudos, para ignorar ruido de sensor
                float diff = heightsSmoothed[z, x] - localAverage[z, x];
                float steepness = _terrain.terrainData.GetSteepness(
                    (float)x / (res - 1), (float)z / (res - 1));

                ZoneType zone;
                if (steepness >= slopeThresholdDegrees)
                    zone = ZoneType.Pendiente;
                else if (diff > heightThreshold)
                    zone = ZoneType.BancoElevado;
                else if (diff < -heightThreshold)
                    zone = ZoneType.BajioEstero;
                else
                    zone = ZoneType.SabanaPlana;

                ZoneMap[z, x] = zone;

                switch (zone)
                {
                    case ZoneType.SabanaPlana: countSabana++; break;
                    case ZoneType.BancoElevado: countBanco++; break;
                    case ZoneType.BajioEstero: countBajio++; break;
                    case ZoneType.Pendiente: countPendiente++; break;
                }

                if (generateDebugTexture)
                {
                    Color c = zone switch
                    {
                        ZoneType.SabanaPlana => new Color(0.85f, 0.85f, 0.45f), // amarillo
                        ZoneType.BancoElevado => new Color(0.55f, 0.35f, 0.15f), // marrón
                        ZoneType.BajioEstero => new Color(0.25f, 0.5f, 0.75f),   // azul
                        ZoneType.Pendiente => new Color(0.9f, 0.1f, 0.1f),       // rojo
                        _ => Color.magenta
                    };
                    debugPixels[z * res + x] = c;
                }
            }
        }

        if (generateDebugTexture)
        {
            DebugTexture = new Texture2D(res, res);
            DebugTexture.SetPixels(debugPixels);
            DebugTexture.Apply();
            Debug.Log("Textura de depuración generada. Accede a ella vía DebugTexture o arrástrala a un material para verla sobre el terreno.");

            if (exportDebugPNG)
                SaveDebugTextureAsPNG();
        }

        int total = res * res;
        Debug.Log($"Clasificación completa. Resolución: {res}x{res}, " +
                   $"radio de análisis: {radiusMeters}m (~{radiusPixelsX}x{radiusPixelsZ} px), " +
                   $"pre-suavizado: {noisePreSmoothRadius}px.");
        Debug.Log($"Distribución de zonas — Sabana: {countSabana} ({(100f * countSabana / total):F1}%), " +
                   $"Banco: {countBanco} ({(100f * countBanco / total):F1}%), " +
                   $"Bajío/Estero: {countBajio} ({(100f * countBajio / total):F1}%), " +
                   $"Pendiente: {countPendiente} ({(100f * countPendiente / total):F1}%)");
    }

    /// <summary>Guarda DebugTexture como PNG dentro de Assets/, creando la carpeta si no existe.</summary>
    private void SaveDebugTextureAsPNG()
    {
        if (DebugTexture == null)
        {
            Debug.LogWarning("No hay DebugTexture para exportar.");
            return;
        }

        try
        {
            string fullPath = Path.Combine(Application.dataPath, "..", debugPNGPath);
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            byte[] pngData = DebugTexture.EncodeToPNG();
            File.WriteAllBytes(fullPath, pngData);

            Debug.Log($"PNG de debug guardado en: {debugPNGPath}. " +
                       "Si no aparece en el Project de inmediato, hacé click derecho en la carpeta y 'Reimport'.");

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar el PNG de debug: {e.Message}");
        }
    }

    // Box blur separable (horizontal luego vertical) — O(n) en vez de O(n * r^2).
    private float[,] BoxBlur(float[,] src, int res, int radiusX, int radiusZ)
    {
        float[,] temp = new float[res, res];
        float[,] result = new float[res, res];

        // Pasada horizontal
        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                float sum = 0f;
                int count = 0;
                for (int dx = -radiusX; dx <= radiusX; dx++)
                {
                    int xx = x + dx;
                    if (xx < 0 || xx >= res) continue;
                    sum += src[z, xx];
                    count++;
                }
                temp[z, x] = sum / count;
            }
        }

        // Pasada vertical
        for (int x = 0; x < res; x++)
        {
            for (int z = 0; z < res; z++)
            {
                float sum = 0f;
                int count = 0;
                for (int dz = -radiusZ; dz <= radiusZ; dz++)
                {
                    int zz = z + dz;
                    if (zz < 0 || zz >= res) continue;
                    sum += temp[zz, x];
                    count++;
                }
                result[z, x] = sum / count;
            }
        }

        return result;
    }
}