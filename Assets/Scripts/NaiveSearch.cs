using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class NaiveSearch : MonoBehaviour, IFindNearestNeighbour
{
    private List<Transform> unitsToComapre;
    public void Setup(List<Transform> units)
    {
        unitsToComapre = units;
    }

    public Transform FindNearestNeighbour(Vector3 position)
    {
        Profiler.BeginSample("NaiveSearch");
        Transform closest = null;
        float best = float.MaxValue;
        foreach (var unit in unitsToComapre)
        {
            float distance = Vector3.Distance(unit.position, position);
            if(distance > best) continue;
            best = distance;
            closest = unit;
        }

        Profiler.EndSample();
        return closest;
    }
}
