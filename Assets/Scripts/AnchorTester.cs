using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static CustomEnums;

public class AnchorTester : MonoBehaviour
{
    public MarkerLocation AnchorLocation;
    public TMP_Text TextAnchorLocation;
    public TMP_Text TextTransform;
    public TMP_Text TextTutorial; // Needed?
    public bool IsPlacementAnchor;

    private AnchorManager anchorManager;
    private AnchorBinder binder;
    private int currentAnchorIndex;
    private Vector3 lastPos;


    void Start()
    {
        anchorManager = FindObjectOfType<AnchorManager>();
        binder = FindObjectOfType<AnchorBinder>();
        if (!IsPlacementAnchor)
            binder.AllAnchors.Add(this);

        TextAnchorLocation.text = AnchorLocation.ToString();

        TextTransform.text = transform.position.ToString();

        if (GetComponent<OVRSpatialAnchor>())
            BindRelationObject();
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

    [ContextMenu("DeleteAllPlayerPrefs")]
    public void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [ContextMenu("Erase")]
    public void Erase()
    {
        anchorManager.EraseAnchor(GetComponent<OVRSpatialAnchor>());
    }

    [ContextMenu("Save")]
    public async void Save()
    {
        GameObject copy = Instantiate(gameObject);
        Destroy(copy.GetComponent<Collider>());
        copy.GetComponent<AnchorTester>().IsPlacementAnchor = false;
        OVRSpatialAnchor anchor = copy.AddComponent<OVRSpatialAnchor>();
        await Task.Delay(1000);
        anchorManager.SaveAnchor(anchor, AnchorLocation);
    }

    [ContextMenu("Bind")]
    public void BindRelationObject()
    {
        Transform obj = anchorManager.RelationObject;
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

    public void ChangeAnchorLocation(bool next)
    {
        int length = Enum.GetNames(typeof(MarkerLocation)).Length;

        currentAnchorIndex = next ? currentAnchorIndex + 1 : currentAnchorIndex - 1;

        if (currentAnchorIndex >= length)
            currentAnchorIndex = 0;
        if (currentAnchorIndex < 0)
            currentAnchorIndex = length - 1;

        MarkerLocation current = (MarkerLocation)currentAnchorIndex;

        AnchorLocation = current;
        TextAnchorLocation.text = current.ToString();
    }
}
