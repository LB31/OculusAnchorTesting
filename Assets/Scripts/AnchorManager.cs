using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorManager : MonoBehaviour
{
    public GameObject AnchorPrefab;
    public Transform RelationObject;

    public const string NumUuidsPlayerPref = "numUuids";

    private AnchorData anchorData = new();


    private void Start()
    {
        // Check if markers exist
        LoadAnchorsByUuid();
        anchorData = AnchorDataManager.Instance.AnchorData;
    }

    public void SaveAnchor(OVRSpatialAnchor _spatialAnchor)
    {
        _spatialAnchor.Save((anchor, success) =>
        {
            if (!success) return;

            // Write uuid of saved anchor to file
            if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
            {
                PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
                Debug.Log(PlayerPrefs.GetInt(NumUuidsPlayerPref));
            }

            int playerNumUuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
            Debug.Log(playerNumUuids);
            Debug.Log(anchor.Uuid.ToString());

            PlayerPrefs.SetString("uuid" + playerNumUuids, anchor.Uuid.ToString());
            PlayerPrefs.SetInt(NumUuidsPlayerPref, ++playerNumUuids);
            Debug.Log(playerNumUuids);
        });
    }

    public void LoadAnchorsByUuid()
    {
        if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
        }

        // Get number of saved anchor uuids
        int playerUuidCount = PlayerPrefs.GetInt(NumUuidsPlayerPref);
        Debug.Log($"Attempting to load {playerUuidCount} saved anchors.");
        if (playerUuidCount == 0)
            return;

        Guid[] uuids = new Guid[playerUuidCount];
        for (int i = 0; i < playerUuidCount; ++i)
        {
            string uuidKey = "uuid" + i;
            string currentUuid = PlayerPrefs.GetString(uuidKey);
            Debug.Log("QueryAnchorByUuid: " + currentUuid);

            uuids[i] = new Guid(currentUuid);
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
    }

    public void EraseAnchor(OVRSpatialAnchor _spatialAnchor)
    {
        if (!_spatialAnchor) return;

        _spatialAnchor.Erase((anchor, success) =>
        {
            if (success)
            {
                Debug.Log("erased anchor " + _spatialAnchor.name);
                Destroy(_spatialAnchor);
            }
        });
    }

}
