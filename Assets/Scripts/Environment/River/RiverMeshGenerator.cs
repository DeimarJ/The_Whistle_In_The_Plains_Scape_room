using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RiverMeshGenerator : MonoBehaviour
{
    [Header("Forma del río")]
    [SerializeField] private float riverWidth = 12f; // ancho en metros, ajustá según tu Stream Width real
    [SerializeField] private int segmentsPerUnit = 1; // resolución a lo largo del spline (más alto = más suave en curvas)
    [SerializeField] private float yOffset = 0f; // ajuste fino de altura sobre el waterSurfaceY

    [Header("UV / Flow")]
    [SerializeField] private float uvTiling = 0.1f; // qué tan seguido se repite la textura a lo largo del río

    private SplineContainer splineContainer;
    private MeshFilter meshFilter;

    [ContextMenu("Generar Mesh del Río")]
    public void GenerateMesh()
    {
        splineContainer = GetComponent<SplineContainer>();
        meshFilter = GetComponent<MeshFilter>();

        Spline spline = splineContainer.Spline;
        float splineLength = spline.GetLength();
        int segments = Mathf.Max(2, Mathf.RoundToInt(splineLength * segmentsPerUnit));

        var vertices = new System.Collections.Generic.List<Vector3>();
        var uvs = new System.Collections.Generic.List<Vector2>();
        var triangles = new System.Collections.Generic.List<int>();

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;

            spline.Evaluate(t, out float3 position, out float3 tangent, out float3 upVector);

            Vector3 worldPos = splineContainer.transform.TransformPoint(position);
            Vector3 forward = ((Vector3)tangent).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            Vector3 left = worldPos - right * (riverWidth * 0.5f) + Vector3.up * yOffset;
            Vector3 rightP = worldPos + right * (riverWidth * 0.5f) + Vector3.up * yOffset;

            // Convertimos a espacio local del propio objeto para el mesh
            vertices.Add(transform.InverseTransformPoint(left));
            vertices.Add(transform.InverseTransformPoint(rightP));

            float v = t * splineLength * uvTiling;
            uvs.Add(new Vector2(0f, v));
            uvs.Add(new Vector2(1f, v));

            if (i < segments)
            {
                int baseIndex = i * 2;
                // Dos triángulos por segmento (quad)
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);

                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "RiverMesh";
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        meshFilter.sharedMesh = mesh;

        // Generamos también un MeshCollider para usar de trigger del RiverCurrent
        MeshCollider col = GetComponent<MeshCollider>();
        if (col == null) col = gameObject.AddComponent<MeshCollider>();
        col.sharedMesh = mesh;
        col.convex = false;
        col.isTrigger = true;

        Debug.Log($"Mesh del río generado: {segments} segmentos, {vertices.Count} vértices.");
    }

    private void OnDrawGizmosSelected()
    {
        if (splineContainer == null) splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null) return;

        Gizmos.color = Color.cyan;
        Spline spline = splineContainer.Spline;
        if (spline == null) return;

        int previewSteps = 50;
        for (int i = 0; i < previewSteps; i++)
        {
            float t0 = i / (float)previewSteps;
            float t1 = (i + 1) / (float)previewSteps;
            spline.Evaluate(t0, out float3 p0, out _, out _);
            spline.Evaluate(t1, out float3 p1, out _, out _);
            Gizmos.DrawLine(splineContainer.transform.TransformPoint(p0), splineContainer.transform.TransformPoint(p1));
        }
    }
}
