using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialAnchor
{
    public class AnchorMover : MonoBehaviour
    {
        public OVRInput.Controller controller = OVRInput.Controller.RTouch;
        public LineRenderer laser;
        // only used in this script for fading in from black
        public OVRPassthroughLayer passthrough;

        GameObject hoverObject = null;
        GameObject grabObject = null;

        [Range(-1f, 1f)]
        public float PassthroughBrightness = -1;
        [Range(-1f, 1f)]
        public float PassthroughContrast = -1;

        #region private fields from another mother
        // all-purpose timer to use for blending after object is grabbed/released
        float grabTime = 0.0f;
        // the grabbed object's transform relative to the controller
        Vector3 localGrabOffset = Vector3.zero;
        Quaternion localGrabRotation = Quaternion.identity;
        // the camera and grabbing hand's world position when grabbing
        Vector3 camGrabPosition = Vector3.zero;
        Quaternion camGrabRotation = Quaternion.identity;
        Vector3 handGrabPosition = Vector3.zero;
        Quaternion handGrabRotation = Quaternion.identity;
        Vector3 cursorPosition = Vector3.zero;
        float rotationOffset = 0.0f;
        #endregion


        private void Start()
        {
            if (passthrough)
            {
                //passthrough.colorMapEditorBrightness = -1;
                //passthrough.colorMapEditorContrast = -1;
                passthrough.SetBrightnessContrastSaturation(PassthroughBrightness, PassthroughContrast);
            }
            StartCoroutine(StartDemo());
        }

        private void OnValidate()
        {
            passthrough.SetBrightnessContrastSaturation(PassthroughBrightness, PassthroughContrast);
        }

        void Update()
        {
            CheckForDominantHand();

            Vector3 controllerPos = OVRInput.GetLocalControllerPosition(controller);
            Quaternion controllerRot = OVRInput.GetLocalControllerRotation(controller);

            FindHoverObject(controllerPos, controllerRot);

            if (hoverObject)
            {
                // Grab object when it is hovered
                if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, controller))
                {
                    // grabbing
                    grabObject = hoverObject;
                    GrabHoverObject(grabObject, controllerPos, controllerRot);
                }
            }

            if (!grabObject)
            {
                grabTime -= Time.deltaTime * 5;
                grabTime = Mathf.Clamp01(grabTime);
                return;
            }

            grabTime += Time.deltaTime * 5;
            grabTime = Mathf.Clamp01(grabTime);
            ManipulateObject(grabObject, controllerPos, controllerRot);


            if (!OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controller))
            {
                ReleaseObject();
            }

            TrackButtonInput();
        }

        private void TrackButtonInput()
        {
            AnchorController anchor = null;
            if (grabObject)
                anchor = grabObject.GetComponent<AnchorController>();
            if (anchor == null) return;

            // Place object
            if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
                anchor.Save();
            // Erase Object
            if (OVRInput.GetDown(OVRInput.RawButton.X))
                anchor.EraseAll();
            // Change room type
            if (OVRInput.GetDown(OVRInput.RawButton.Y))
                anchor.ChangeAnchorRoom();

            // Change anchor type
            //if (OVRInput.GetDown(OVRInput.RawButton.A))
            //    anchor.ChangeAnchorLocation(true);
            //if (OVRInput.GetDown(OVRInput.RawButton.B))
            //    anchor.ChangeAnchorLocation(false);
        }

        void GrabHoverObject(GameObject grbObj, Vector3 controllerPos, Quaternion controllerRot)
        {
            localGrabOffset = Quaternion.Inverse(controllerRot) * (grabObject.transform.position - controllerPos);
            localGrabRotation = Quaternion.Inverse(controllerRot) * grabObject.transform.rotation;
            rotationOffset = 0.0f;
            if (grabObject.GetComponent<GrabObject>())
            {
                grabObject.GetComponent<GrabObject>().Grab(controller);
                grabObject.GetComponent<GrabObject>().grabbedRotation = grabObject.transform.rotation;
            }
            handGrabPosition = controllerPos;
            handGrabRotation = controllerRot;
            camGrabPosition = Camera.main.transform.position;
            camGrabRotation = Camera.main.transform.rotation;
        }

        void ReleaseObject()
        {
            if (grabObject.GetComponent<GrabObject>())
            {
                if (grabObject.GetComponent<GrabObject>().objectManipulationType == GrabObject.ManipulationType.Menu)
                {
                    // restore the menu it to its first resting place
                    grabObject.transform.position = handGrabPosition + handGrabRotation * localGrabOffset;
                    grabObject.transform.rotation = handGrabRotation * localGrabRotation;
                }
                grabObject.GetComponent<GrabObject>().Release();
            }
            grabObject = null;
        }

        // wait for systems to get situated, then spawn the objects in front of them
        IEnumerator StartDemo()
        {
            // fade from black
            float timer = 0.0f;
            float fadeTime = 1.0f;
            while (timer <= fadeTime)
            {
                timer += Time.deltaTime;
                float normTimer = Mathf.Clamp01(timer / fadeTime);
                if (passthrough)
                {
                    passthrough.colorMapEditorBrightness = Mathf.Lerp(-1.0f, 0.0f, normTimer);
                    passthrough.colorMapEditorContrast = Mathf.Lerp(-1.0f, 0.0f, normTimer);
                }
                yield return null;
            }
        }

        private void FindHoverObject(Vector3 controllerPos, Quaternion controllerRot)
        {
            RaycastHit[] objectsHit = Physics.RaycastAll(controllerPos, controllerRot * Vector3.forward);
            float closestObject = Mathf.Infinity;
            float rayDistance = 2.0f;
            bool showLaser = true;
            Vector3 labelPosition = Vector3.zero;
            foreach (RaycastHit hit in objectsHit)
            {
                float thisHitDistance = Vector3.Distance(hit.point, controllerPos);
                if (thisHitDistance < closestObject)
                {
                    hoverObject = hit.collider.gameObject;
                    closestObject = thisHitDistance;
                    rayDistance = grabObject ? thisHitDistance : thisHitDistance - 0.1f;
                    labelPosition = hit.point;
                }
            }
            if (objectsHit.Length == 0)
            {
                hoverObject = null;
            }

            // if intersecting with an object, grab it
            Collider[] hitColliders = Physics.OverlapSphere(controllerPos, 0.05f);
            foreach (var hitCollider in hitColliders)
            {
                // use the last object, if there are multiple hits.
                // If objects overlap, this would require improvements.
                hoverObject = hitCollider.gameObject;
                showLaser = false;
                labelPosition = hitCollider.ClosestPoint(controllerPos);
                labelPosition += (Camera.main.transform.position - labelPosition).normalized * 0.05f;
            }

            // show/hide laser pointer
            if (laser)
            {
                laser.positionCount = 2;
                Vector3 pos1 = controllerPos + controllerRot * (Vector3.forward * 0.05f);
                cursorPosition = controllerPos + controllerRot * (Vector3.forward * rayDistance);
                laser.SetPosition(0, pos1);
                laser.SetPosition(1, cursorPosition);
                laser.enabled = (showLaser);
                if (grabObject && grabObject.GetComponent<GrabObject>())
                {
                    grabObject.GetComponent<GrabObject>().CursorPos(cursorPosition);
                }
            }
        }

        // the heavy lifting code for moving objects
        void ManipulateObject(GameObject obj, Vector3 controllerPos, Quaternion controllerRot)
        {
            bool useDefaultManipulation = true;
            Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller);

            if (obj.GetComponent<GrabObject>())
            {
                useDefaultManipulation = false;
                switch (obj.GetComponent<GrabObject>().objectManipulationType)
                {
                    case GrabObject.ManipulationType.Default:
                        useDefaultManipulation = true;
                        break;
                    case GrabObject.ManipulationType.ForcedHand:
                        obj.transform.position = Vector3.Lerp(obj.transform.position, controllerPos, grabTime);
                        obj.transform.rotation = Quaternion.Lerp(obj.transform.rotation, controllerRot, grabTime);
                        break;
                    case GrabObject.ManipulationType.DollyHand:
                        float targetDist = localGrabOffset.z + thumbstick.y * 0.01f;
                        targetDist = Mathf.Clamp(targetDist, 0.1f, 2.0f);
                        localGrabOffset = Vector3.forward * targetDist;
                        obj.transform.position = Vector3.Lerp(obj.transform.position, controllerPos + controllerRot * localGrabOffset, grabTime);
                        obj.transform.rotation = Quaternion.Lerp(obj.transform.rotation, controllerRot, grabTime);
                        break;
                    case GrabObject.ManipulationType.DollyAttached:
                        obj.transform.position = controllerPos + controllerRot * localGrabOffset;
                        obj.transform.rotation = controllerRot * localGrabRotation;
                        ClampGrabOffset(ref localGrabOffset, thumbstick.y);
                        break;
                    case GrabObject.ManipulationType.HorizontalScaled:
                        obj.transform.position = controllerPos + controllerRot * localGrabOffset;
                        if (!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controller))
                        {
                            obj.transform.localScale = ClampScale(obj.transform.localScale, thumbstick);
                        }
                        else
                        {
                            rotationOffset -= thumbstick.x;
                            ClampGrabOffset(ref localGrabOffset, thumbstick.y);
                        }
                        Vector3 newFwd = obj.GetComponent<GrabObject>().grabbedRotation * Vector3.forward;
                        newFwd = new Vector3(newFwd.x, 0, newFwd.z);
                        obj.transform.rotation = Quaternion.Euler(0, rotationOffset, 0) * Quaternion.LookRotation(newFwd);
                        break;
                    case GrabObject.ManipulationType.VerticalScaled:
                        obj.transform.position = controllerPos + controllerRot * localGrabOffset;
                        if (!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controller))
                        {
                            obj.transform.localScale = ClampScale(obj.transform.localScale, thumbstick);
                        }
                        else
                        {
                            rotationOffset -= thumbstick.x;
                            ClampGrabOffset(ref localGrabOffset, thumbstick.y);
                        }
                        Vector3 newUp = obj.GetComponent<GrabObject>().grabbedRotation * Vector3.up;
                        newUp = new Vector3(newUp.x, 0, newUp.z);
                        obj.transform.rotation = Quaternion.LookRotation(Vector3.up, Quaternion.Euler(0, rotationOffset, 0) * newUp);
                        break;
                    case GrabObject.ManipulationType.Menu:
                        Vector3 targetPos = handGrabPosition + (handGrabRotation * Vector3.forward * 0.4f);
                        Quaternion targetRotation = Quaternion.LookRotation(targetPos - camGrabPosition);
                        obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, grabTime);
                        obj.transform.rotation = Quaternion.Lerp(obj.transform.rotation, targetRotation, grabTime);
                        break;
                    default:
                        useDefaultManipulation = true;
                        break;
                }
            }

            if (useDefaultManipulation)
            {
                obj.transform.position = controllerPos + controllerRot * localGrabOffset;
                obj.transform.Rotate(Vector3.up * -thumbstick.x);
                ClampGrabOffset(ref localGrabOffset, thumbstick.y);
            }
        }

        void ClampGrabOffset(ref Vector3 localOffset, float thumbY)
        {
            Vector3 projectedGrabOffset = localOffset + Vector3.forward * thumbY * 0.01f;
            if (projectedGrabOffset.z > 0.1f)
            {
                localOffset = projectedGrabOffset;
            }
        }

        Vector3 ClampScale(Vector3 localScale, Vector2 thumb)
        {
            float newXscale = localScale.x + thumb.x * 0.01f;
            if (newXscale <= 0.1f) newXscale = 0.1f;
            float newZscale = localScale.z + thumb.y * 0.01f;
            if (newZscale <= 0.1f) newZscale = 0.1f;
            return new Vector3(newXscale, 0.0f, newZscale);
        }

        private void CheckForDominantHand()
        {
            // don't switch if hovering or grabbing
            if (hoverObject || grabObject)
                return;

            if (controller == OVRInput.Controller.RTouch)
            {
                if (OVRInput.Get(OVRInput.RawButton.LHandTrigger))
                {
                    controller = OVRInput.Controller.LTouch;
                }
            }
            else
            {
                if (OVRInput.Get(OVRInput.RawButton.RHandTrigger))
                {
                    controller = OVRInput.Controller.RTouch;
                }
            }
        }

    }
}