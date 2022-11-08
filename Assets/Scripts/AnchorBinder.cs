using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorBinder : MonoBehaviour
{
    public Transform Player;
    public List<AnchorTester> AllAnchors = new();
    public float TimeTillNextCheck = 10;

    private float t;

    private void Update()
    {
        t += Time.deltaTime;
        if (t >= TimeTillNextCheck)
        {
            t = 0;
            // Find nearest anchor
            AnchorTester nearestAnchor = FindNearestAnchor();

            // Bind play area
            if (nearestAnchor != null)
            {
                nearestAnchor.BindRelationObject();
                Debug.Log(nearestAnchor.AnchorLocation, nearestAnchor.gameObject);
            }

        }
    }

    private AnchorTester FindNearestAnchor()
    {
        float nearest = float.MaxValue;
        AnchorTester nearestAnchor = null;
        foreach (AnchorTester anchor in AllAnchors)
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
}
