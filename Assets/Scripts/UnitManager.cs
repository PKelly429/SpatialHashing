using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private int worldSize; 
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int targetUnits;
    
    [HideInInspector] public List<Transform> units = new List<Transform>();

    private TransformAccessArray unitTransforms;
    private NativeArray<float> unitSpeeds;
    private void Start()
    {
        unitTransforms = new TransformAccessArray(units.ToArray());
        unitSpeeds = new NativeArray<float>(units.Count, Allocator.Persistent);

        for (int i=0; i<unitSpeeds.Length; i++)
        {
            unitSpeeds[i] = Random.Range(1, 10f);
        }
    }

    private void OnDestroy()
    {
        unitTransforms.Dispose();
    }

    private void Update()
    {
        MoveUnitsJob moveJob = new MoveUnitsJob()
        {
            baseSpeed = unitSpeeds,
            moveSpeed = Time.deltaTime*10f,
            rotateSpeed = Time.deltaTime
        };
        moveJob.Schedule(unitTransforms);
    }

    [ContextMenu ("Update Unit Count")]
    private void UpdateUnitCount()
    {
        if (units.Count == targetUnits)
        {
            return;
        }

        if (targetUnits < units.Count)
        {
            for (int i = units.Count-1; i >= targetUnits; i--)
            {
                if(units[i] != null) DestroyImmediate(units[i].gameObject);
                units.RemoveAt(i);
            }

            return;
        }


        Transform parent = transform;
        for (int i = units.Count; i < targetUnits; i++)
        {
            Vector3 position = new Vector3(Random.Range(-worldSize, worldSize), 0, Random.Range(-worldSize, worldSize));
            Vector3 forward = new Vector3(Random.Range(-worldSize, worldSize), 0, Random.Range(-worldSize, worldSize));
            units.Add(Instantiate(unitPrefab, position, Quaternion.LookRotation(forward), parent).transform);
        }
    }
}

[BurstCompile]
public struct MoveUnitsJob : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<float> baseSpeed;
    public float moveSpeed;
    public float rotateSpeed;
    public void Execute(int index, TransformAccess transform)
    {
        Quaternion rotation = transform.rotation;
        Vector3 forward = rotation * Vector3.forward;
        rotation *= Quaternion.AngleAxis(rotateSpeed, Vector3.up);
        transform.SetPositionAndRotation(transform.position + (forward * (baseSpeed[index] * moveSpeed)), rotation);
    }
}