using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestNeighbourTest : MonoBehaviour
{
    [SerializeField] private Transform comparisonUnit;
    [SerializeField] private UnitManager unitManager;
    
    [SerializeField] private Material unitMaterial;
    [SerializeField] private Material closestMaterial;
    
    [Header("Solution Under Test")]
    [SerializeField] private GameObject solution;

    private IFindNearestNeighbour nearestNeighbourSolution;
    private Transform currentClosest;
    private bool hasSolution = true;
    public void Start()
    {
        nearestNeighbourSolution = solution.GetComponent<IFindNearestNeighbour>();

        if (nearestNeighbourSolution == null)
        {
            hasSolution = false;
            Debug.LogError("No Nearest Neighbour solution connected");
            return;
        }
        
        nearestNeighbourSolution.Setup(unitManager.units);
    }

    private void Update()
    {
        if (!hasSolution) return;

        Vector3 comparisonPosition = comparisonUnit.transform.position;
        comparisonPosition = new Vector3(comparisonPosition.x, 0, comparisonPosition.z);
        comparisonUnit.transform.position = comparisonPosition;
        
        SetClosest(nearestNeighbourSolution.FindNearestNeighbour(comparisonPosition));
    }

    private void SetClosest(Transform nextClosest)
    {
        if (currentClosest == nextClosest) return;
        
        if (currentClosest != null)
        {
            currentClosest.GetComponent<Renderer>().material = unitMaterial;
        }

        currentClosest = nextClosest;

        if (currentClosest != null)
        {
            currentClosest.GetComponent<Renderer>().material = closestMaterial;
        }
    }
}
