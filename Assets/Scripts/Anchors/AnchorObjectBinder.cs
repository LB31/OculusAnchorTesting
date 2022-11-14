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
    public class AnchorObjectBinder : MonoBehaviour
    {
        public AnchorManager AnchorManager;
        public Transform Player;
        public float TimeTillNextCheck = 10;
        public List<AnchorController> AllAnchors = new();

        public List<RoomObject> RoomObjects = new();

        private float t;

        private void Update()
        {
            t += Time.deltaTime;
            if (t >= TimeTillNextCheck)
            {
                t = 0;
                // Find nearest anchor
                AnchorController nearestAnchor = FindNearestAnchor();

                // Bind play area
                if (nearestAnchor != null && !nearestAnchor.IsPlacementAnchor)
                {
                    BindRelationObject(nearestAnchor);
                    Debug.Log(nearestAnchor.AnchorLocation, nearestAnchor.gameObject);
                }

            }
        }

        private AnchorController FindNearestAnchor()
        {
            float nearest = float.MaxValue;
            AnchorController nearestAnchor = null;
            foreach (AnchorController anchor in AllAnchors)
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

        public void BindRelationObject(AnchorController nearestAnchor)
        {
            Transform roomObj = GetRoomObject(nearestAnchor.ContentRoom).transform; // TODO 
            roomObj.gameObject.SetActive(true);

            roomObj.parent = nearestAnchor.transform;
            roomObj.localPosition = Vector3.zero;

            roomObj.localPosition += GetMarkerPosition(roomObj, nearestAnchor.AnchorLocation);

            roomObj.localRotation = Quaternion.identity;

            roomObj.parent = null;
        }

        private Vector3 GetMarkerPosition(Transform relationObj, MarkerLocation location)
        {
            Vector3 result = Vector3.zero;
            Vector3 localScale = relationObj.localScale;

            switch (location)
            {
                case MarkerLocation.DownLeft:
                    result = new Vector3(localScale.x * 0.5f, 0, localScale.z * 0.5f);
                    break;
                case MarkerLocation.MiddleLeft:
                    result = new Vector3(localScale.x * 0.5f, 0, 0);
                    break;
                case MarkerLocation.UpLeft:
                    result = new Vector3(localScale.x * 0.5f, 0, -localScale.z * 0.5f);
                    break;
                case MarkerLocation.DownRight:
                    result = new Vector3(-localScale.x * 0.5f, 0, localScale.z * 0.5f);
                    break;
                case MarkerLocation.MiddleRight:
                    result = new Vector3(-localScale.x * 0.5f, 0, 0);
                    break;
                case MarkerLocation.UpRight:
                    result = new Vector3(-localScale.x * 0.5f, 0, -localScale.z * 0.5f);
                    break;
                default:
                    break;
            }

            return result;
        }

        private GameObject GetRoomObject(ContentRoom type)
        {
            return RoomObjects.FirstOrDefault(obj => obj.RoomType.Equals(type)).Prefab;
        }

        public Vector3 GetNextAnchorPosition(MarkerLocation location)
        {
            switch (location)
            {
                case MarkerLocation.DownLeft:
                    break;
                case MarkerLocation.MiddleLeft:
                    break;
                case MarkerLocation.UpLeft:
                    break;
                case MarkerLocation.DownRight:
                    break;
                case MarkerLocation.MiddleRight:
                    break;
                case MarkerLocation.UpRight:
                    break;
                default:
                    break;
            }
        }
    }

    [Serializable]
    public struct RoomObject
    {
        public ContentRoom RoomType;
        public GameObject Prefab;
    }
}