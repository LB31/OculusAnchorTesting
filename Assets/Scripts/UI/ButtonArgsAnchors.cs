using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonArgsAnchors : CustomButtonArgs
{
    public Transform Target;
    public Vector3 PositionChange;
    public Vector3 RotationChange;

    protected override void AssignButtonEvents()
    {
        button.onClick.AddListener(() => ButtonWasClicked(PositionChange, RotationChange));
    }

    private void ButtonWasClicked(Vector3 positionChange, Vector3 rotationChange)
    {
        Target.localPosition += Target.rotation * positionChange * 0.01f;

        Target.Rotate(rotationChange, Space.World);
    }

}
