using CurvedPathGenerator;
using UnityEngine;

public class ConveyorSystem : MonoBehaviour
{
    public Transform EntryPoint => transform.GetChild(1);
    public Vector3 GetEntryPosition => EntryPoint.position;
    public PathGenerator Path => GetComponentInChildren<PathGenerator>();

    public MeshRenderer BeltMeshRenderer => transform.parent.GetComponent<MeshRenderer>();
    public MeshRenderer BeltWallMeshRenderer => transform.parent.parent.GetComponent<MeshRenderer>();

    [Range(1, 100)]
    public int MaximumCupsAllowed = 35;
}
