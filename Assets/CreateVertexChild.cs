using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateVertexChild : MonoBehaviour
{
    [ContextMenu("Create quad centers")]
    public void CreateQuadCenters()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 6) // 2 triangles per quad
        {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];
            Vector3 v4 = vertices[triangles[i + 3]];
            Vector3 v5 = vertices[triangles[i + 4]];
            Vector3 v6 = vertices[triangles[i + 5]];

            Vector3 center = (v1 + v2 + v3 + v4 + v5 + v6) / 6f;

            GameObject quadCenter = new GameObject("QuadCenter " + (i / 6));
            quadCenter.transform.SetParent(transform, false);
            quadCenter.transform.localPosition = center;
        }
    }

    [ContextMenu("Populate Path")]
    public void PopulatePath()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;

        // Ensure there are at least 2 vertices
        if (vertices.Length < 2) return;

        // Calculate total path length
        float totalLength = 0f;
        for (int i = 0; i < vertices.Length - 1; i++)
            totalLength += Vector3.Distance(vertices[i], vertices[i + 1]);

        int resolution = 70;
        float step = totalLength / (resolution - 1);
        float distCovered = 0f;
        int currentSegment = 0;

        for (int i = 0; i < resolution; i++)
        {
            float targetDist = i * step;

            // Advance segment until we reach the one containing targetDist
            while (currentSegment < vertices.Length - 2 &&
                   Vector3.Distance(vertices[currentSegment], vertices[currentSegment + 1]) + distCovered < targetDist)
            {
                distCovered += Vector3.Distance(vertices[currentSegment], vertices[currentSegment + 1]);
                currentSegment++;
            }

            // Interpolate between segment vertices
            Vector3 a = vertices[currentSegment];
            Vector3 b = vertices[currentSegment + 1];
            float segmentLength = Vector3.Distance(a, b);
            float t = (targetDist - distCovered) / segmentLength;

            Vector3 pos = Vector3.Lerp(a, b, t);
            pos = transform.TransformPoint(pos);

            GameObject marker = new GameObject("PathPoint " + i);
            marker.transform.position = pos;
            marker.transform.SetParent(transform);
        }
    }
}
