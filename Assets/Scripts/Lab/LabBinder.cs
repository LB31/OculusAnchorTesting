using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabBinder : MonoBehaviour
{
    public Transform Player;
    public List<LabAnchor> AllAnchors = new();

    public float width, length;

    private bool start;

    private void Start()
    {
        width = transform.lossyScale.x;
        length = transform.lossyScale.z;
    }

    public void Initialize()
    {
        Transform startParent = AllAnchors[0].transform;

        transform.parent = startParent;
        transform.localRotation = Quaternion.identity;

        float scaleNormalizerX = 1f / startParent.lossyScale.x;
        float scaleNormalizerZ = 1f / startParent.lossyScale.z;

        transform.localPosition = Vector3.zero;
        transform.localPosition += new Vector3(width * 0.5f * scaleNormalizerX, 0, length * 0.5f * scaleNormalizerZ);

        start = true;
    }

    private void Update()
    {
        if (!start) return;

        LabAnchor nearest = FindNearestAnchor();
        SetPosition(nearest);
    }

    private LabAnchor FindNearestAnchor()
    {
        float nearest = float.MaxValue;
        LabAnchor nearestAnchor = null;
        foreach (LabAnchor anchor in AllAnchors)
        {
            float distnace = Vector3.Distance(Player.position, anchor.transform.position);
            if (distnace < nearest)
            {
                nearest = distnace;
                nearestAnchor = anchor;
            }
        }

        return nearestAnchor;
    }

    private void SetPosition(LabAnchor target)
    {
        transform.parent = target.transform;
    }
}
