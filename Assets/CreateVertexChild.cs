using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateVertexChild : MonoBehaviour
{   
    [Range(10,70)]
    public int Resolution;

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

        float step = totalLength / (Resolution - 1);
        float distCovered = 0f;
        int currentSegment = 0;

        for (int i = 0; i < Resolution; i++)
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
