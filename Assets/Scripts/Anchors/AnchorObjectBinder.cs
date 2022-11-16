using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static CustomEnums;
using System.Threading.Tasks;

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

        private float width, length;

        private bool start;

        private float t;

        public void TryInitialize()
        {
            if (AllAnchors.Count == 0) return;

            foreach (RoomObject room in RoomObjects)
            {
                AnchorController zeroElement = AllAnchors.FirstOrDefault(a => a.LocalPosition.Equals(Vector2.zero) && a.ContentRoom.CompareTo(room.RoomType) == 0);
                if (zeroElement && !room.Binded)
                    Initialize(room, zeroElement.transform);
            }

            // Are all rooms binded?
            if(RoomObjects.All(r => r.Binded))
                start = true;
        }

        public void Initialize(RoomObject contentRoom, Transform startParent)
        {
            Debug.Log("Initialize " + contentRoom.Prefab.name);
            contentRoom.Binded = true;

            Transform room = contentRoom.Prefab.transform;
            width = room.lossyScale.x;
            length = room.lossyScale.z;

            room.parent = startParent;

            room.localRotation = Quaternion.identity;

            float scaleNormalizerX = 1f / startParent.lossyScale.x;
            float scaleNormalizerZ = 1f / startParent.lossyScale.z;

            room.localPosition = Vector3.zero;
            room.localPosition += new Vector3(width * 0.5f * scaleNormalizerX, 0, length * 0.5f * scaleNormalizerZ);

            room.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!start)
            {
                TryInitialize();
                return;
            }

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

        public void BindRelationObject(AnchorController target)
        {
            Transform room = GetRoomObject(target.ContentRoom).transform;
            room.transform.parent = target.transform;
            room.gameObject.SetActive(true);            
        }

        public GameObject GetRoomObject(ContentRoom type)
        {
            return RoomObjects.FirstOrDefault(obj => obj.RoomType.Equals(type)).Prefab;
        }


    }

    [Serializable]
    public class RoomObject
    {
        public ContentRoom RoomType;
        public GameObject Prefab;
        public bool Binded;
    }
}