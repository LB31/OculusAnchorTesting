using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static CustomEnums;

/// <summary>
/// Let target objects follow anchors that are responsible for them
/// </summary>

namespace SpatialAnchor
{
    public class AnchorBinder : MonoBehaviour
    {
        public AnchorManager AnchorManager;
        public Transform Player;
        public float TimeTillNextCheck = 10;
        public List<AnchorTester> AllAnchors = new();

        public List<RoomObject> RoomObjects = new();

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
                    BindRelationObject(nearestAnchor);
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

        public void BindRelationObject(AnchorTester nearestAnchor)
        {
            Transform obj = GetRoomObject(nearestAnchor.ContentRoom).transform; // TODO 
            obj.gameObject.SetActive(true);

            obj.parent = nearestAnchor.transform;
            obj.localPosition = Vector3.zero;

            obj.localPosition += GetMarkerPosition(obj, nearestAnchor.AnchorLocation);

            obj.localRotation = Quaternion.identity;

            obj.parent = null;
        }

        private Vector3 GetMarkerPosition(Transform relationObj, MarkerLocation location)
        {
            Vector3 result = Vector3.zero;
            Vector3 localScale = relationObj.localScale;

            switch (location)
            {
                case MarkerLocation.DownLeft:
                    return new Vector3(localScale.x * 0.5f, 0, localScale.z * 0.5f);
                case MarkerLocation.MiddleLeft:
                    return new Vector3(localScale.x * 0.5f, 0, 0);
                case MarkerLocation.UpLeft:
                    return new Vector3(localScale.x * 0.5f, 0, -localScale.z * 0.5f);
                case MarkerLocation.DownRight:
                    return new Vector3(-localScale.x * 0.5f, 0, localScale.z * 0.5f);
                case MarkerLocation.MiddleRight:
                    return new Vector3(-localScale.x * 0.5f, 0, 0);
                case MarkerLocation.UpRight:
                    return new Vector3(-localScale.x * 0.5f, 0, -localScale.z * 0.5f);
                default:
                    break;
            }

            return result;
        }

        private GameObject GetRoomObject(ContentRoom type)
        {
            return RoomObjects.FirstOrDefault(obj => obj.RoomType.Equals(type)).Prefab;
        }
    }

    [Serializable]
    public struct RoomObject
    {
        public ContentRoom RoomType;
        public GameObject Prefab;
    }
}