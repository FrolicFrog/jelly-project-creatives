using UnityEngine;
using System.Collections.Generic;
using CurvedPathGenerator;
using UnityEngine.Diagnostics;
using UnityEditor;

public class GhostCup : MonoBehaviour
{
    [Header("SETTINGS")]
    public float DetectionRange = 2f;

    public void UpdatePos()
    {
        List<Transform> CupsOnConveyer = Manager.Instance.GameManagement.CupsOnConveyer;
        float closestDist = DetectionRange;
        Transform nearestCup = null;

        foreach (var realCup in CupsOnConveyer)
        {
            float dist = Vector3.Distance(transform.position, realCup.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearestCup = realCup;
            }
        }

        if (nearestCup != null)
        {
            if (TryGetComponent(out PathFollower pf))
            {
                pf.InsertIntoPath(nearestCup.position);
                Debug.Log("Inserted into path at position: " + nearestCup.position);
            }
            
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
