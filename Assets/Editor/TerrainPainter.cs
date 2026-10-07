using UnityEngine;
using UnityEditor;

public class TerrainPainter : EditorWindow
{
    [MenuItem("Tools/Terrain Painter - Llanos")]
    public static void ShowWindow()
    {
        GetWindow<TerrainPainter>("Terrain Painter");
    }

    // Referencias
    private Terrain terrain;

    // Altura del río en mundo
    private float riverWorldY      = 20f;
    private float terrainHeight    = 125.4f;

    // Zona de tierra (barro/cauce)
    private float dirtHeightMargin = 4f;    // pinta tierra X metros por encima del río
    private float dirtBlend        = 3f;    // transición suave entre tierra y pasto (metros)

    // Pendiente
    private float slopeThreshold   = 25f;   // ángulo a partir del cual pinta tierra
    private float slopeBlend       = 10f;   // rango de transición de pendiente

    // Índices de layers (orden en que los agregaste al Terrain)
    private int grassLayerIndex    = 0;     // NewLayer  (pasto)
    private int dirtLayerIndex     = 1;     // NewLayer2 (tierra)

    private void OnGUI()
    {
        GUILayout.Label("Configuración", EditorStyles.boldLabel);

        terrain         = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);

        EditorGUILayout.Space();
        GUILayout.Label("Altura del terreno", EditorStyles.boldLabel);
        terrainHeight   = EditorGUILayout.FloatField("Terrain Height (m)", terrainHeight);
        riverWorldY     = EditorGUILayout.FloatField("Río Y (world)", riverWorldY);
        dirtHeightMargin= EditorGUILayout.FloatField("Margen sobre el río (m)", dirtHeightMargin);
        dirtBlend       = EditorGUILayout.FloatField("Blend altura (m)", dirtBlend);

        EditorGUILayout.Space();
        GUILayout.Label("Pendiente", EditorStyles.boldLabel);
        slopeThreshold  = EditorGUILayout.FloatField("Ángulo mínimo tierra (°)", slopeThreshold);
        slopeBlend      = EditorGUILayout.FloatField("Blend pendiente (°)", slopeBlend);

        EditorGUILayout.Space();
        GUILayout.Label("Layers", EditorStyles.boldLabel);
        grassLayerIndex = EditorGUILayout.IntField("Índice Pasto", grassLayerIndex);
        dirtLayerIndex  = EditorGUILayout.IntField("Índice Tierra", dirtLayerIndex);

        EditorGUILayout.Space();

        GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
        if (GUILayout.Button("🖌  Pintar Terrain", GUILayout.Height(40)))
        {
            if (terrain == null)
            {
                EditorUtility.DisplayDialog("Error", "Asigná el Terrain primero.", "OK");
                return;
            }
            PaintTerrain();
        }

        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("↩  Resetear a Pasto"))
        {
            if (terrain != null) ResetToGrass();
        }
    }

    private void PaintTerrain()
    {
        TerrainData data       = terrain.terrainData;
        int alphaRes           = data.alphamapResolution;
        int layerCount         = data.terrainLayers.Length;

        float[,,] alphamaps    = new float[alphaRes, alphaRes, layerCount];

        // Altura normalizada del río + margen
        float riverNorm        = riverWorldY / terrainHeight;
        float dirtTopNorm      = (riverWorldY + dirtHeightMargin) / terrainHeight;
        float blendRangeNorm   = dirtBlend / terrainHeight;

        for (int y = 0; y < alphaRes; y++)
        {
            for (int x = 0; x < alphaRes; x++)
            {
                // Coordenadas normalizadas (0-1)
                float nx = (float)x / (alphaRes - 1);
                float ny = (float)y / (alphaRes - 1);

                // Altura y pendiente en este punto
                float height = data.GetInterpolatedHeight(nx, ny);
                float slope  = data.GetSteepness(nx, ny);

                // ── Factor de tierra por ALTURA ─────────────────────────────
                float heightNorm     = height / terrainHeight;
                float dirtByHeight   = 0f;

                if (heightNorm <= riverNorm)
                {
                    // Dentro del cauce: tierra total
                    dirtByHeight = 1f;
                }
                else if (heightNorm <= dirtTopNorm + blendRangeNorm)
                {
                    // Zona de transición: blend suave
                    float t = Mathf.InverseLerp(dirtTopNorm + blendRangeNorm, riverNorm, heightNorm);
                    dirtByHeight = Mathf.SmoothStep(0f, 1f, t);
                }

                // ── Factor de tierra por PENDIENTE ───────────────────────────
                float dirtBySlope = 0f;
                if (slope >= slopeThreshold + slopeBlend)
                {
                    dirtBySlope = 1f;
                }
                else if (slope >= slopeThreshold)
                {
                    dirtBySlope = Mathf.SmoothStep(0f, 1f,
                        Mathf.InverseLerp(slopeThreshold, slopeThreshold + slopeBlend, slope));
                }

                // ── Combinamos ambos factores ────────────────────────────────
                float dirtWeight  = Mathf.Clamp01(Mathf.Max(dirtByHeight, dirtBySlope));
                float grassWeight = 1f - dirtWeight;

                // Inicializamos todos los layers en 0
                for (int l = 0; l < layerCount; l++)
                    alphamaps[y, x, l] = 0f;

                alphamaps[y, x, grassLayerIndex] = grassWeight;
                alphamaps[y, x, dirtLayerIndex]  = dirtWeight;
            }
        }

        Undo.RegisterCompleteObjectUndo(data, "Terrain Procedural Paint");
        data.SetAlphamaps(0, 0, alphamaps);
        EditorUtility.SetDirty(terrain);
        Debug.Log("✅ Terrain pintado proceduralmente.");
    }

    private void ResetToGrass()
    {
        TerrainData data    = terrain.terrainData;
        int alphaRes        = data.alphamapResolution;
        int layerCount      = data.terrainLayers.Length;

        float[,,] alphamaps = new float[alphaRes, alphaRes, layerCount];
        for (int y = 0; y < alphaRes; y++)
            for (int x = 0; x < alphaRes; x++)
                alphamaps[y, x, grassLayerIndex] = 1f;

        Undo.RegisterCompleteObjectUndo(data, "Terrain Reset to Grass");
        data.SetAlphamaps(0, 0, alphamaps);
        Debug.Log("↩ Terrain reseteado a pasto.");
    }
}
