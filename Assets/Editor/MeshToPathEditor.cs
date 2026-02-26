using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CurvedPathGenerator;

namespace CurvedPathGenerator
{
    public class MeshToPathTool : EditorWindow
    {
        private GameObject targetMeshObject;
        private int submeshIndex = 0; 
        private bool isClosedPath = true; // Added toggle for closed path

        [MenuItem("Tools/Curved Path Generator/Mesh To Block Path Tool")]
        public static void ShowWindow()
        {
            GetWindow<MeshToPathTool>("Block to Path");
        }

        private void OnGUI()
        {
            GUILayout.Label("Path from Disconnected Blocks", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this tool for tracks made of individual segments/blocks. It finds each disconnected piece and puts exactly one node in its center.", MessageType.Info);
            
            EditorGUILayout.Space();

            targetMeshObject = (GameObject)EditorGUILayout.ObjectField("Target Mesh Object", targetMeshObject, typeof(GameObject), true);
            
            EditorGUILayout.Space();
            
            GUILayout.Label("Path Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox("If the top of your blocks use a specific material (like 'Belt'), enter its Submesh Index here (usually 0, 1, or 2) to put the point exactly on the top face.", MessageType.None);
            submeshIndex = EditorGUILayout.IntField("Submesh Index", submeshIndex);
            if (submeshIndex < 0) submeshIndex = 0;

            // Added Toggle for Closed Path
            isClosedPath = EditorGUILayout.Toggle("Close Path (Loop)", isClosedPath);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Generate 1 Point Per Block", GUILayout.Height(40)))
            {
                GeneratePath();
            }
        }

        private void GeneratePath()
        {
            if (targetMeshObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Target Mesh Object.", "OK");
                return;
            }

            MeshFilter meshFilter = targetMeshObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                EditorUtility.DisplayDialog("Error", "The target object must have a MeshFilter with a valid mesh.", "OK");
                return;
            }

            Mesh mesh = meshFilter.sharedMesh;

            if (submeshIndex >= mesh.subMeshCount)
            {
                EditorUtility.DisplayDialog("Error", $"Submesh Index {submeshIndex} is out of bounds. This mesh only has {mesh.subMeshCount} submeshes (materials). Try 0.", "OK");
                return;
            }

            Vector3[] localVertices = mesh.vertices;
            int[] triangles = mesh.GetTriangles(submeshIndex); 
            Transform objTransform = targetMeshObject.transform;

            // --- STEP 1: GROUP VERTICES BY POSITION ---
            Dictionary<Vector3Int, List<int>> vertexPosToTriangles = new Dictionary<Vector3Int, List<int>>();
            
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int triIndex = i / 3;
                for (int v = 0; v < 3; v++)
                {
                    Vector3 pos = localVertices[triangles[i + v]];
                    Vector3Int roundedPos = new Vector3Int(
                        Mathf.RoundToInt(pos.x * 1000f),
                        Mathf.RoundToInt(pos.y * 1000f),
                        Mathf.RoundToInt(pos.z * 1000f)
                    );

                    if (!vertexPosToTriangles.ContainsKey(roundedPos))
                        vertexPosToTriangles[roundedPos] = new List<int>();

                    vertexPosToTriangles[roundedPos].Add(triIndex);
                }
            }

            // --- STEP 2: FIND DISCONNECTED BLOCKS (ISLANDS) ---
            int totalTriangles = triangles.Length / 3;
            bool[] visitedTriangles = new bool[totalTriangles];
            List<Vector3> unsortedBlockCenters = new List<Vector3>();

            for (int i = 0; i < totalTriangles; i++)
            {
                if (visitedTriangles[i]) continue;

                Vector3 sumPos = Vector3.zero;
                int vertCount = 0;

                Queue<int> queue = new Queue<int>();
                queue.Enqueue(i);
                visitedTriangles[i] = true;

                while (queue.Count > 0)
                {
                    int currentTri = queue.Dequeue();
                    int tIndex = currentTri * 3;

                    for (int v = 0; v < 3; v++)
                    {
                        Vector3 pos = localVertices[triangles[tIndex + v]];
                        sumPos += pos;
                        vertCount++;

                        Vector3Int roundedPos = new Vector3Int(
                            Mathf.RoundToInt(pos.x * 1000f),
                            Mathf.RoundToInt(pos.y * 1000f),
                            Mathf.RoundToInt(pos.z * 1000f)
                        );

                        if (vertexPosToTriangles.ContainsKey(roundedPos))
                        {
                            foreach (int neighborTri in vertexPosToTriangles[roundedPos])
                            {
                                if (!visitedTriangles[neighborTri])
                                {
                                    visitedTriangles[neighborTri] = true;
                                    queue.Enqueue(neighborTri);
                                }
                            }
                        }
                    }
                }

                Vector3 blockLocalCenter = sumPos / vertCount;
                unsortedBlockCenters.Add(objTransform.TransformPoint(blockLocalCenter));
            }

            if (unsortedBlockCenters.Count < 2)
            {
                EditorUtility.DisplayDialog("Error", "Could not find enough disconnected blocks. Ensure your mesh is actually separated into pieces.", "OK");
                return;
            }

            // --- STEP 3: SORT BLOCKS TO FORM A PATH ---
            List<Vector3> sortedNodes = SortPointsNearestNeighbor(unsortedBlockCenters);

            // --- STEP 4: CREATE THE PATH ---
            GameObject pathObj = new GameObject($"{targetMeshObject.name}_BlockPath");
            pathObj.transform.SetParent(targetMeshObject.transform);
            pathObj.transform.localPosition = Vector3.zero;
            pathObj.transform.localRotation = Quaternion.identity;
            pathObj.transform.localScale = Vector3.one;

            Undo.RegisterCreatedObjectUndo(pathObj, "Create Block Path");

            PathGenerator pathGen = pathObj.AddComponent<PathGenerator>();
            pathGen.NodeList.Clear();
            pathGen.NodeList_World.Clear();
            pathGen.AngleList.Clear();
            pathGen.AngleList_World.Clear();
            
            // Set the Closed Path boolean based on UI toggle
            pathGen.IsClosed = isClosedPath;

            for (int i = 0; i < sortedNodes.Count; i++)
            {
                Vector3 wNode = sortedNodes[i];
                Vector3 lNode = pathObj.transform.InverseTransformPoint(wNode);

                pathGen.NodeList_World.Add(wNode);
                pathGen.NodeList.Add(lNode);

                // Control angles
                if (i < sortedNodes.Count - 1)
                {
                    Vector3 wNext = sortedNodes[i + 1];
                    Vector3 wMid = (wNode + wNext) / 2f;
                    Vector3 lMid = pathObj.transform.InverseTransformPoint(wMid);

                    pathGen.AngleList_World.Add(wMid);
                    pathGen.AngleList.Add(lMid);
                }
                // If it's a closed path, calculate the final angle connecting the LAST node back to the FIRST node
                else if (isClosedPath)
                {
                    Vector3 wNext = sortedNodes[0];
                    Vector3 wMid = (wNode + wNext) / 2f;
                    Vector3 lMid = pathObj.transform.InverseTransformPoint(wMid);

                    pathGen.AngleList_World.Add(wMid);
                    pathGen.AngleList.Add(lMid);
                }
            }

            pathGen.UpdatePath();
            Selection.activeGameObject = pathObj;

            string pathType = isClosedPath ? "closed (looping)" : "open";
            Debug.Log($"<color=cyan>Success!</color> Found <b>{sortedNodes.Count}</b> individual blocks and created a <b>{pathType}</b> path node at the center of each one.");
        }

        private List<Vector3> SortPointsNearestNeighbor(List<Vector3> unsortedPoints)
        {
            List<Vector3> sorted = new List<Vector3>();
            List<Vector3> pool = new List<Vector3>(unsortedPoints);

            sorted.Add(pool[0]);
            pool.RemoveAt(0);

            while (pool.Count > 0)
            {
                Vector3 current = sorted[sorted.Count - 1];
                int nearestIndex = -1;
                float minDistanceSqr = float.MaxValue;

                for (int i = 0; i < pool.Count; i++)
                {
                    float distSqr = (current - pool[i]).sqrMagnitude;
                    if (distSqr < minDistanceSqr)
                    {
                        minDistanceSqr = distSqr;
                        nearestIndex = i;
                    }
                }

                sorted.Add(pool[nearestIndex]);
                pool.RemoveAt(nearestIndex);
            }

            return sorted;
        }
    }
}