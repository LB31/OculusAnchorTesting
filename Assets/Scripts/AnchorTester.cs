using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static CustomEnums;

public class AnchorTester : MonoBehaviour
{
    public MarkerLocation AnchorLocation;
    private AnchorManager am;
    private AnchorBinder binder;

    void Start()
    {
        am = FindObjectOfType<AnchorManager>();
        binder = FindObjectOfType<AnchorBinder>();
        binder.AllAnchors.Add(this);

        if (GetComponent<OVRSpatialAnchor>())
            BindRelationObject();
    }

    private void OnDestroy()
    {
        binder.AllAnchors.Remove(this);
    }

    [ContextMenu("DeleteAllPlayerPrefs")]
    public void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [ContextMenu("Erase")]
    public void Erase()
    {
        am.EraseAnchor(GetComponent<OVRSpatialAnchor>());
    }

    [ContextMenu("Save")]
    public async void Save()
    {
        OVRSpatialAnchor anchor = gameObject.AddComponent<OVRSpatialAnchor>();
        await Task.Delay(1000);
        am.SaveAnchor(anchor, AnchorLocation);
    }

    [ContextMenu("Bind")]
    public void BindRelationObject()
    {
        Transform obj = am.RelationObject;
        obj.gameObject.SetActive(true);

        obj.parent = transform;
        obj.localPosition = Vector3.zero;

        obj.localPosition += GetMarkerPosition(obj);

        obj.localRotation = Quaternion.identity;

        obj.parent = null;
    }

    private Vector3 GetMarkerPosition(Transform relationObj)
    {
        Vector3 result = Vector3.zero;
        Vector3 localScale = relationObj.localScale;

        switch (AnchorLocation)
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

}
