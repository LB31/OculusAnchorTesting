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

        public async void Initialize()
        {
            while (AllAnchors.Count == 0)
            {
                await Task.Yield();
            }

            AnchorController first = AllAnchors.FirstOrDefault(a => a.LocalPosition.Equals(Vector2.zero));

            Transform startParent = first.transform;

            // Set parent
            BindRelationObject(first);
            Transform room = GetRoomObject(first.ContentRoom).transform;
            room.gameObject.SetActive(true);

            width = room.transform.lossyScale.x;
            length = room.transform.lossyScale.z;

            room.localRotation = Quaternion.identity;

            float scaleNormalizerX = 1f / startParent.lossyScale.x;
            float scaleNormalizerZ = 1f / startParent.lossyScale.z;

            room.localPosition = Vector3.zero;
            room.localPosition += new Vector3(width * 0.5f * scaleNormalizerX, 0, length * 0.5f * scaleNormalizerZ);

            start = true;
        }

        private void Update()
        {
            if (!start) return;

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
            GetRoomObject(target.ContentRoom).transform.parent = target.transform;
        }

        public GameObject GetRoomObject(ContentRoom type)
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