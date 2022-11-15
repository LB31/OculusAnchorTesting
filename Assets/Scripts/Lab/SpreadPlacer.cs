using SpatialAnchor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CustomEnums;

public class SpreadPlacer : MonoBehaviour
{
    public Transform Room;
    public GameObject Anchor;
    public float DistanceBetweenAnchors = 3;
    public AnchorObjectBinder Binder;

    private void Start()
    {
        Test();
    }

    [ContextMenu("Test")]
    public void Test()
    {
        Transform startParent = null;
        GameObject room = Binder.GetRoomObject(ContentRoom.LivingRoom);
        LabBinder lb = room.GetComponent<LabBinder>();

        Vector2 anchorAmounts = GetAnchorAmounts(room.transform);
        bool first = true;

        for (int i = 0; i <= anchorAmounts.x; i++)
        {
            for (int j = 0; j <= anchorAmounts.y; j++)
            {
                // x * i + j

                Transform copy = Instantiate(Anchor).transform;

                copy.name += " " + i + " " + j;

                float x;
                float z;

                // Set position
                if (!first)
                {
                    copy.parent = startParent;
                    copy.localPosition = Vector3.zero;

                    float scaleNormalizerX = 1f / startParent.lossyScale.x;
                    float scaleNormalizerZ = 1f / startParent.lossyScale.z;

                    x = i;
                    z = j;

                    copy.localPosition += new Vector3(i * DistanceBetweenAnchors * scaleNormalizerX, 0, j * DistanceBetweenAnchors * scaleNormalizerZ);

                    copy.localRotation = Quaternion.identity;
                    //copy.parent = null;

                    copy.parent = null;
                }
                else
                {
                    startParent = copy;
                    first = false;
                    x = z = 0;
                }

                LabAnchor la = copy.gameObject.AddComponent<LabAnchor>();
                la.PositionInRoom = new Vector2(x, z);
                lb.AllAnchors.Add(la);
            }
        }

        lb.Initialize();
    }

    private Vector2 GetAnchorAmounts(Transform room)
    {
        float width = room.lossyScale.x;
        float length = room.lossyScale.z;
        int neededAnchorsX = (int)(width / DistanceBetweenAnchors);
        int neededAnchorsZ = (int)(length / DistanceBetweenAnchors);

        return new Vector2(neededAnchorsX, neededAnchorsZ);
    }
}
