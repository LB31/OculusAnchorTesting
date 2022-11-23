using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using System.Linq;
using static CustomEnums;

namespace SpatialAnchor
{
    public class AnchorController : MonoBehaviour
    {
        public Vector2 LocalPosition;
        public ContentRoom ContentRoom;
        public TMP_Text TextContentRoom;
        public TMP_Text TextTransform;
        public bool IsPlacementAnchor;
        [HideInInspector]
        public AnchorObjectBinder Binder;

        private AnchorManager anchorManager;
        private int currentAnchorLocationIndex;
        private int currentAnchorRoomIndex;
        private Vector3 lastPos;
        private Quaternion lastRot;

        private Transform contentRoomObj;

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

            TextContentRoom.text = ContentRoom.ToString();
            TextTransform.text = transform.position.ToString();
            TextTransform.text += "\n" + transform.rotation.eulerAngles.ToString();

            contentRoomObj = Binder.GetRoomObject(ContentRoom).transform;
            //if (GetComponent<OVRSpatialAnchor>())
            //    BindRelationObject();
        }

        private void Update()
        {
            if (transform.position != lastPos || transform.rotation != lastRot)
            {
                TextTransform.text = transform.position.ToString();
                TextTransform.text += "\n" + transform.rotation.eulerAngles.ToString();
                lastPos = transform.position;
                lastRot = transform.rotation;
            }
        }

        private void OnDestroy()
        {
            Binder.AllAnchors.Remove(this);
        }

        [ContextMenu("Erase")]
        public void Erase()
        {
            Binder.AllAnchors.Remove(this);
            Binder.UnbindRoom(contentRoomObj);

            anchorManager.EraseAnchor(GetComponent<OVRSpatialAnchor>());
        }

        [ContextMenu("EraseAll")]
        public void EraseAll()
        {
            // Find all anchors und filter to current ContentRoom
            List<OVRSpatialAnchor> allAnchors = FindObjectsOfType<OVRSpatialAnchor>().ToList();
            allAnchors = allAnchors.Where(a => a.GetComponent<AnchorController>().ContentRoom.Equals(ContentRoom)).ToList();

            foreach (OVRSpatialAnchor anchor in allAnchors)
            {
                Binder.AllAnchors.Remove(anchor.GetComponent<AnchorController>());
                anchorManager.EraseAnchor(anchor);
            }
            
            Binder.UnbindRoom(contentRoomObj);
        }

        [ContextMenu("Save")]
        public async void Save()
        {
            Transform startParent = null;
            Transform copy = null;
            List<OVRSpatialAnchor> createdAnchors = new();

            Vector2 anchorAmounts = GetAnchorAmountsForRoom(contentRoomObj);

            bool firstAnchor = true;

            float x;
            float z;

            // Create spatial anchors according to selected room
            for (int i = 0; i <= anchorAmounts.x; i++)
            {
                for (int j = 0; j <= anchorAmounts.y; j++)
                {
                    copy = Instantiate(gameObject).transform;
                    copy.name += $"{ContentRoom} {i} {j}";

                    // Set position
                    if (!firstAnchor)
                    {
                        copy.parent = startParent;
                        copy.localPosition = Vector3.zero;

                        float scaleNormalizerX = 1f / startParent.lossyScale.x;
                        float scaleNormalizerZ = 1f / startParent.lossyScale.z;

                        x = i * distanceBetweenAnchors * scaleNormalizerX;
                        z = j * distanceBetweenAnchors * scaleNormalizerZ;

                        copy.localPosition += new Vector3(x, 0, z);

                        copy.localRotation = Quaternion.identity;

                        copy.parent = null;
                    }
                    else
                    {
                        startParent = copy;
                        firstAnchor = false;
                        x = z = 0;
                    }

                    AnchorController anchorcontrollerCopy = copy.GetComponent<AnchorController>();
                    anchorcontrollerCopy.IsPlacementAnchor = false;
                    anchorcontrollerCopy.LocalPosition = new Vector2(x, z);

                    anchorcontrollerCopy.TryInitialize();
                    Binder.TryInitialize();

                    OVRSpatialAnchor anchor = copy.gameObject.AddComponent<OVRSpatialAnchor>();
                    createdAnchors.Add(anchor);


                    await Task.Delay(1000);
                    anchorManager.SaveAnchor(anchor, anchorcontrollerCopy.LocalPosition, ContentRoom);
                }
            }
        }

        private Vector2 GetAnchorAmountsForRoom(Transform room)
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
            GameObject newRoom = Binder.GetRoomObject(ContentRoom);
            if (newRoom)
                contentRoomObj = newRoom.transform;

            TextContentRoom.text = current.ToString();

        }
    }
}
