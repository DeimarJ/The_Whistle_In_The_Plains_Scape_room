using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RiverMeshGenerator : MonoBehaviour
{
    [Header("Forma del río")]
    [SerializeField] private float riverWidth = 12f;
    [SerializeField] private int segmentsPerUnit = 1;
    [SerializeField] private float yOffset = 0f;

    [Header("Volumen de colisión")]
    [SerializeField] private float riverDepth = 6f;

    [Header("UV / Flow")]
    [SerializeField] private float uvTiling = 0.1f;

    private SplineContainer splineContainer;
    private MeshFilter meshFilter;

    // Nombre del contenedor de colliders hijos
    private const string VOLUME_CONTAINER_NAME = "RiverVolumeColliders";

    [ContextMenu("Generar Mesh del Río")]
    public void GenerateMesh()
    {
        splineContainer = GetComponent<SplineContainer>();
        meshFilter = GetComponent<MeshFilter>();

        Spline spline = splineContainer.Spline;
        float splineLength = spline.GetLength();
        int segments = Mathf.Max(2, Mathf.RoundToInt(splineLength * segmentsPerUnit));

        //1. MESH VISUAL
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        // Precalcular filas de la cinta (left/right por segmento)
        var leftPoints = new Vector3[segments + 1];
        var rightPoints = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            spline.Evaluate(t, out float3 position, out float3 tangent, out _);

            Vector3 worldPos = splineContainer.transform.TransformPoint(position);
            Vector3 forward = ((Vector3)tangent).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            leftPoints[i] = worldPos - right * (riverWidth * 0.5f) + Vector3.up * yOffset;
            rightPoints[i] = worldPos + right * (riverWidth * 0.5f) + Vector3.up * yOffset;

            vertices.Add(transform.InverseTransformPoint(leftPoints[i]));
            vertices.Add(transform.InverseTransformPoint(rightPoints[i]));

            float v = t * splineLength * uvTiling;
            uvs.Add(new Vector2(0f, v));
            uvs.Add(new Vector2(1f, v));

            if (i < segments)
            {
                int b = i * 2;
                triangles.Add(b); triangles.Add(b + 2); triangles.Add(b + 1);
                triangles.Add(b + 1); triangles.Add(b + 2); triangles.Add(b + 3);
            }
        }

        Mesh surfaceMesh = new Mesh { name = "RiverSurface" };
        surfaceMesh.SetVertices(vertices);
        surfaceMesh.SetUVs(0, uvs);
        surfaceMesh.SetTriangles(triangles, 0);
        surfaceMesh.RecalculateNormals();
        surfaceMesh.RecalculateBounds();
        surfaceMesh.RecalculateTangents();
        meshFilter.sharedMesh = surfaceMesh;

        // VOLUMEN DE COLISIÓN (un prismo convexo por segmento)
        Transform oldContainer = transform.Find(VOLUME_CONTAINER_NAME);
        if (oldContainer != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(oldContainer.gameObject);
#else
            Destroy(oldContainer.gameObject);
#endif
        }

        GameObject container = new GameObject(VOLUME_CONTAINER_NAME);
        container.transform.SetParent(transform, false);
        Rigidbody rb = container.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        int volumeCount = 0;

        for (int i = 0; i < segments; i++)
        {
            // 8 vértices del prisma: 4 arriba (superficie) + 4 abajo (fondo)
            Vector3 tl = leftPoints[i];
            Vector3 tr = rightPoints[i];
            Vector3 bl = leftPoints[i + 1];
            Vector3 br = rightPoints[i + 1];

            Vector3 tlD = tl + Vector3.down * riverDepth;
            Vector3 trD = tr + Vector3.down * riverDepth;
            Vector3 blD = bl + Vector3.down * riverDepth;
            Vector3 brD = br + Vector3.down * riverDepth;


            Vector3[] verts = new Vector3[8]
            {
                container.transform.InverseTransformPoint(tl),
                container.transform.InverseTransformPoint(tr),
                container.transform.InverseTransformPoint(bl),
                container.transform.InverseTransformPoint(br),
                container.transform.InverseTransformPoint(tlD),
                container.transform.InverseTransformPoint(trD),
                container.transform.InverseTransformPoint(blD),
                container.transform.InverseTransformPoint(brD),
            };

            // 12 triángulos (6 caras × 2 triángulos)
            int[] tris = new int[]
            {
                // Top
                0, 2, 1,  1, 2, 3,
                // Bottom
                4, 5, 6,  5, 7, 6,
                // Left
                0, 4, 2,  2, 4, 6,
                // Right
                1, 3, 5,  3, 7, 5,
                // Front
                0, 1, 4,  1, 5, 4,
                // Back
                2, 6, 3,  3, 6, 7,
            };

            Mesh segMesh = new Mesh { name = $"RiverSegVol_{i}" };
            segMesh.SetVertices(verts);
            segMesh.SetTriangles(tris, 0);
            segMesh.RecalculateNormals();
            segMesh.RecalculateBounds();

            GameObject segObj = new GameObject($"Seg_{i}");
            segObj.transform.SetParent(container.transform, false);
            segObj.layer = gameObject.layer;

            MeshCollider mc = segObj.AddComponent<MeshCollider>();
            mc.sharedMesh = segMesh;
            mc.convex = true; 
            mc.isTrigger = true;

            volumeCount++;
        }

        Debug.Log($"Río generado: {segments} segmentos visuales, {volumeCount} colisores de volumen convexos.");
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
            Gizmos.DrawLine(
                splineContainer.transform.TransformPoint(p0),
                splineContainer.transform.TransformPoint(p1));
        }

        // Dibuja el volumen en el primer y último segmento como referencia
        if (Application.isPlaying) return;
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.15f);
        spline.Evaluate(0f, out float3 posA, out float3 tanA, out _);
        spline.Evaluate(1f, out float3 posB, out float3 tanB, out _);
        Vector3 wA = splineContainer.transform.TransformPoint(posA);
        Vector3 wB = splineContainer.transform.TransformPoint(posB);
        Gizmos.DrawCube((wA + wB) * 0.5f + Vector3.down * (riverDepth * 0.5f),
                        new Vector3(riverWidth, riverDepth, Vector3.Distance(wA, wB)));
    }
}