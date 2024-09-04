using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFindNearestNeighbour
{
    public void Setup(List<Transform> units);
    public Transform FindNearestNeighbour(Vector3 position);
}
