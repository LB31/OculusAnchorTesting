using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using static CustomEnums;
/// <summary>
/// Is responsible for saving and loading anchors into and out of the system and a json file
/// </summary>

namespace SpatialAnchor
{
    public class AnchorManager : MonoBehaviour
    {
        public GameObject AnchorPrefab;

        public AnchorDatabase anchorDatabase = new();

        public string anchorFile;


        private void Awake()
        {
            anchorFile = Application.persistentDataPath + "/anchorData.json";

            ReadFile();

            // Check if markers exist
            LoadAnchorsByUuid();
        }

        public void SaveAnchor(OVRSpatialAnchor _spatialAnchor, Vector2 location, ContentRoom room)
        {
            _spatialAnchor.Save((anchor, success) =>
            {
                if (!success) return;

            // Write uuid of saved anchor to file
            int playerNumUuids = anchorDatabase.AnchorData.Count;
                AnchorData data = new AnchorData(anchor.Uuid.ToString(), null, location, room);
                anchorDatabase.AnchorData.Add(data);

                WriteFile();
            });
        }

        public void LoadAnchorsByUuid()
        {
            ReadFile();

            if (anchorDatabase == null) return;

            // Get number of saved anchor uuids
            int playerUuidCount = anchorDatabase.AnchorData.Count;
            Debug.Log($"Attempting to load {playerUuidCount} saved anchors.");
            if (playerUuidCount == 0)
                return;

            Guid[] uuids = new Guid[playerUuidCount];
            for (int i = 0; i < playerUuidCount; ++i)
            {
                uuids[i] = new Guid(anchorDatabase.AnchorData[i].SpaceUuid);
            }

            LoadAnchors(new OVRSpatialAnchor.LoadOptions
            {
                MaxAnchorCount = 100,
                Timeout = 0,
                StorageLocation = OVRSpace.StorageLocation.Local,
                Uuids = uuids
            });
        }

        private void LoadAnchors(OVRSpatialAnchor.LoadOptions options) => OVRSpatialAnchor.LoadUnboundAnchors(options, anchors =>
        {
            if (anchors == null)
            {
                Debug.Log("Query failed.");
                return;
            }

            foreach (var anchor in anchors)
            {
                if (anchor.Localized)
                {
                    OnLocalized(anchor, true);
                }
                else if (!anchor.Localizing)
                {
                    anchor.Localize(OnLocalized);
                }
            }
        });

        // Restore object when loaded from storage
        private void OnLocalized(OVRSpatialAnchor.UnboundAnchor unboundAnchor, bool success)
        {
            if (!success)
            {
                Debug.Log($"{unboundAnchor} Localization failed!");
                return;
            }

            Pose pose = unboundAnchor.Pose;
            OVRSpatialAnchor spatialAnchor = Instantiate(AnchorPrefab, pose.position, pose.rotation).AddComponent<OVRSpatialAnchor>();
            unboundAnchor.BindTo(spatialAnchor);

            RestoreAnchor(spatialAnchor);
        }

        private void RestoreAnchor(OVRSpatialAnchor anchor)
        {
            AnchorController anchorCon = anchor.GetComponent<AnchorController>();
            AnchorData anchorData = GetAnchorFromDatabase(anchor);
            anchorCon.LocalPosition = anchorData.MarkerLocation;
            anchorCon.ContentRoom = anchorData.ContentRoom;
            anchor.name += $"{anchorData.MarkerLocation} {anchorCon.LocalPosition}";
        }

        public void EraseAnchor(OVRSpatialAnchor _spatialAnchor)
        {
            if (!_spatialAnchor) return;

            _spatialAnchor.Erase((anchor, success) =>
            {
                if (success)
                {
                    Debug.Log("erased anchor " + _spatialAnchor.name);
                    AnchorData data = GetAnchorFromDatabase(_spatialAnchor);
                    anchorDatabase.AnchorData.Remove(data);
                    Destroy(_spatialAnchor.gameObject);

                    WriteFile();
                }
            });
        }

        private void ReadFile()
        {
            // Does the file exist?
            if (File.Exists(anchorFile))
            {
                // Read the entire file and save its contents
                string fileContents = File.ReadAllText(anchorFile);

                // Work with JSON
                anchorDatabase = JsonUtility.FromJson<AnchorDatabase>(fileContents);
            }
        }

        private void WriteFile()
        {
            // Serialize the object into JSON and save string
            string jsonString = JsonUtility.ToJson(anchorDatabase);

            // Write JSON to file
            File.WriteAllText(anchorFile, jsonString);
        }

        private AnchorData GetAnchorFromDatabase(OVRSpatialAnchor _spatialAnchor)
        {
            return anchorDatabase.AnchorData.FirstOrDefault(id => id.SpaceUuid == _spatialAnchor.Uuid.ToString());
        }

        [ContextMenu("EraseAllAnchors")]
        public void EraseAllAnchors()
        {
            // TODO
        }

    }
}