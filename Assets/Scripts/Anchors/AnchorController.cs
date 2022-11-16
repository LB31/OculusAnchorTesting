using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static CustomEnums;

namespace SpatialAnchor
{
    public class AnchorController : MonoBehaviour
    {
        public Vector2 LocalPosition;
        public ContentRoom ContentRoom;
        public TMP_Text TextLocalPosition;
        public TMP_Text TextContentRoom;
        public TMP_Text TextTransform;
        public bool IsPlacementAnchor;
        [HideInInspector]
        public AnchorObjectBinder Binder;

        private AnchorManager anchorManager;
        private int currentAnchorLocationIndex;
        private int currentAnchorRoomIndex;
        private Vector3 lastPos;

        private float distanceBetweenAnchors = 3;

        private void Start()
        {
            TryInitialize();
        }

        public void TryInitialize()
        {
            anchorManager = FindObjectOfType<AnchorManager>();
            Binder = FindObjectOfType<AnchorObjectBinder>();
            if (!IsPlacementAnchor)
                Binder.AllAnchors.Add(this);

            TextLocalPosition.text = LocalPosition.ToString();
            TextContentRoom.text = ContentRoom.ToString();

            TextTransform.text = transform.position.ToString();

            //if (GetComponent<OVRSpatialAnchor>())
            //    BindRelationObject();
        }

        private void Update()
        {
            if (transform.position != lastPos)
            {
                TextTransform.text = transform.position.ToString();
                lastPos = transform.position;
            }
        }

        private void OnDestroy()
        {
            Binder.AllAnchors.Remove(this);
        }

        [ContextMenu("Erase")]
        public void Erase()
        {
            anchorManager.EraseAnchor(GetComponent<OVRSpatialAnchor>());
        }

        [ContextMenu("Save")]
        public async void Save()
        {
            Transform startParent = null;
            Transform copy = null;
            GameObject room = Binder.GetRoomObject(ContentRoom);

            Vector2 anchorAmounts = GetAnchorAmounts(room.transform);
            bool first = true;

            float x;
            float z;

            for (int i = 0; i <= anchorAmounts.x; i++)
            {
                for (int j = 0; j <= anchorAmounts.y; j++)
                {
                    copy = Instantiate(gameObject).transform;

                    copy.name += " " + i + " " + j;

                    // Set position
                    if (!first)
                    {
                        copy.parent = startParent;
                        copy.localPosition = Vector3.zero;

                        float scaleNormalizerX = 1f / startParent.lossyScale.x;
                        float scaleNormalizerZ = 1f / startParent.lossyScale.z;

                        x = i;
                        z = j;

                        copy.localPosition += new Vector3(i * distanceBetweenAnchors * scaleNormalizerX, 0, j * distanceBetweenAnchors * scaleNormalizerZ);

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

                    AnchorController acCopy = copy.GetComponent<AnchorController>();
                    acCopy.IsPlacementAnchor = false;
                    acCopy.TryInitialize();
                    acCopy.LocalPosition = new Vector2(x, z);
                }
            }


            OVRSpatialAnchor anchor = copy.gameObject.AddComponent<OVRSpatialAnchor>();
            await Task.Delay(1000);
            Binder.Initialize();
            anchorManager.SaveAnchor(anchor, LocalPosition, ContentRoom);

        }

        private Vector2 GetAnchorAmounts(Transform room)
        {
            float width = room.lossyScale.x;
            float length = room.lossyScale.z;
            int neededAnchorsX = (int)(width / distanceBetweenAnchors);
            int neededAnchorsZ = (int)(length / distanceBetweenAnchors);

            return new Vector2(neededAnchorsX, neededAnchorsZ);
        }

        public void ChangeAnchorRoom()
        {
            int length = Enum.GetNames(typeof(ContentRoom)).Length;

            currentAnchorRoomIndex++;

            if (currentAnchorRoomIndex >= length)
                currentAnchorRoomIndex = 0;
            if (currentAnchorRoomIndex < 0)
                currentAnchorRoomIndex = length - 1;

            ContentRoom current = (ContentRoom)currentAnchorRoomIndex;

            ContentRoom = current;
            TextContentRoom.text = current.ToString();
        }
    }
}
