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
        public MarkerLocation AnchorLocation;
        public ContentRoom ContentRoom;
        public TMP_Text TextAnchorLocation;
        public TMP_Text TextContentRoom;
        public TMP_Text TextTransform;
        public bool IsPlacementAnchor;

        private AnchorManager anchorManager;
        private AnchorObjectBinder binder;
        private int currentAnchorLocationIndex;
        private int currentAnchorRoomIndex;
        private Vector3 lastPos;


        void Start()
        {
            anchorManager = FindObjectOfType<AnchorManager>();
            binder = FindObjectOfType<AnchorObjectBinder>();
            if (!IsPlacementAnchor)
                binder.AllAnchors.Add(this);

            TextAnchorLocation.text = AnchorLocation.ToString();
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
            binder.AllAnchors.Remove(this);
        }

        [ContextMenu("Erase")]
        public void Erase()
        {
            anchorManager.EraseAnchor(GetComponent<OVRSpatialAnchor>());
        }

        [ContextMenu("Save")]
        public async void Save()
        {
            foreach (MarkerLocation marker in Enum.GetValues(typeof(MarkerLocation)))
            {
                GameObject copy = Instantiate(gameObject);
                //Destroy(copy.GetComponent<Collider>());

                // Set position
                if(marker != 0)
                {
                    
                }

                AnchorController anchorCon = copy.GetComponent<AnchorController>();
                anchorCon.IsPlacementAnchor = false;
                anchorCon.AnchorLocation = marker;

                OVRSpatialAnchor anchor = copy.AddComponent<OVRSpatialAnchor>();

                await Task.Delay(1000);
                anchorManager.SaveAnchor(anchor, AnchorLocation, ContentRoom);
            }
        }



        public void ChangeAnchorLocation(bool next)
        {
            int length = Enum.GetNames(typeof(MarkerLocation)).Length;

            currentAnchorLocationIndex = next ? currentAnchorLocationIndex + 1 : currentAnchorLocationIndex - 1;

            if (currentAnchorLocationIndex >= length)
                currentAnchorLocationIndex = 0;
            if (currentAnchorLocationIndex < 0)
                currentAnchorLocationIndex = length - 1;

            MarkerLocation current = (MarkerLocation)currentAnchorLocationIndex;

            AnchorLocation = current;
            TextAnchorLocation.text = current.ToString();
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
