using Oculus.Interaction;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ScaleCup : MonoBehaviour
{
    [SerializeField] private EvidenceSide tagCheck;

    private readonly List<WeightObj> currObjs = new List<WeightObj>();
    [SerializeField] private int initWeight;
    public float TotalWeight { get; private set; }
    public Action OnWeightChanged;
    private void Start()
    {
        TotalWeight = initWeight;
    }
    private void OnTriggerEnter(Collider other)
    {
        WeightObj prop = other.GetComponent<WeightObj>();
        if (prop != null && !currObjs.Contains(prop))
        {
            currObjs.Add(prop);
            if (tagCheck == prop.GetSide())
            {
                TotalWeight++;
                OnWeightChanged?.Invoke();
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        WeightObj prop = other.GetComponent<WeightObj>();
        if (prop != null && currObjs.Contains(prop))
        {
            currObjs.Remove(prop);
            if (tagCheck == prop.GetSide())
            {
                TotalWeight--;
                if (TotalWeight < 0) TotalWeight = 0;
                OnWeightChanged?.Invoke();
            }
        }
    }

    public void ResetZone()
    {
        currObjs.Clear();
        TotalWeight = initWeight;

        OnWeightChanged?.Invoke();
    }
}
