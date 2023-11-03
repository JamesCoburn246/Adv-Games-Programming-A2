using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PatrolManager : MonoBehaviour
{
    public static PatrolManager Instance { get; private set; }
    public List<Transform> patrolPoints;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        patrolPoints = GetComponentsInChildren<Transform>().ToList();
        patrolPoints.RemoveAt(0);
        
    }
}
