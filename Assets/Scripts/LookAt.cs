using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] private Transform toRotate;
    [SerializeField] private Transform target;

    [SerializeField] private Vector3 worldUp = Vector3.up;


    void Start()
    {
        if (target == null)
            target = Camera.main.transform;
        if (toRotate == null)
            toRotate = transform;
    }

    void Update()
    {
        Vector3 dirToTarget = (target.position - toRotate.position).normalized;
        toRotate.LookAt(toRotate.position - dirToTarget, worldUp);
    }
}
