using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] private Transform toRotate;
    [SerializeField] private Transform target;


    void Start()
    {
        if (target == null)
            target = Camera.main.transform;
    }

    void Update()
    {
        Vector3 dirToTarget = (target.position - toRotate.position).normalized;
        toRotate.LookAt(toRotate.position - dirToTarget, Vector3.up);
    }
}
