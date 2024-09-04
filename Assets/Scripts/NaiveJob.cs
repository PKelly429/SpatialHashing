using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

public class NaiveJob : MonoBehaviour, IFindNearestNeighbour
{
    private List<Transform> unitsToComapre;
    private TransformAccessArray unitTransforms;
    private NativeArray<Vector3> unitPositions;
    private NativeArray<int> result;
    private JobHandle _positionJobHandle;
    private JobHandle _searchJobHandle;
    private bool _needsCleanup;
    
    public void Setup(List<Transform> units)
    {
        unitsToComapre = units;
        _needsCleanup = true;
        unitTransforms = new TransformAccessArray(units.ToArray());
        unitPositions = new NativeArray<Vector3>(unitTransforms.length, Allocator.Persistent);
        result = new NativeArray<int>(1, Allocator.Persistent);
    }

    private void OnDestroy()
    {
        if (_needsCleanup)
        {
            _positionJobHandle.Complete();
            _searchJobHandle.Complete();
            
            unitTransforms.Dispose();
            unitPositions.Dispose();
            result.Dispose();
        }
    }

    public Transform FindNearestNeighbour(Vector3 position)
    {
        var setPosJob = new SetPositionsJob()
        {
            positions = unitPositions
        };

        _positionJobHandle = setPosJob.Schedule(unitTransforms);
        
        Profiler.BeginSample("NaiveSearch - Job");
        var job = new FindNearestNeighbourNaiveJob()
        {
            positions = unitPositions,
            position = position,
            resultIndex = result
        };
        
        _searchJobHandle = job.Schedule(_positionJobHandle);
        
        _searchJobHandle.Complete();

        Profiler.EndSample();
        return unitTransforms[result[0]];
    }
}

[BurstCompile]
public struct SetPositionsJob : IJobParallelForTransform
{
    [WriteOnly] public NativeArray<Vector3> positions;
    
    public void Execute(int index, TransformAccess transform)
    {
        positions[index] = transform.position;
    }
}

[BurstCompile]
public struct FindNearestNeighbourNaiveJob : IJob
{
    [ReadOnly] public NativeArray<Vector3> positions;
    public Vector3 position;
    [WriteOnly] public NativeArray<int> resultIndex;
    
    public void Execute()
    {
        float best = float.MaxValue;
        for(int i=0; i<positions.Length; i++)
        {
            Vector3 dir = positions[i] - position;
            float distance = dir.sqrMagnitude;
            if(distance > best) continue;
            best = distance;
            resultIndex[0] = i;
        }
    }
}
