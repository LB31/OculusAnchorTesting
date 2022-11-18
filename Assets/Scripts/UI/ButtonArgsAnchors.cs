using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonArgsAnchors : CustomButtonArgs
{
    public Transform Target;
    public Vector3 PositionChange;

    protected override void AssignButtonEvents()
    {
        button.onClick.AddListener(() => ButtonWasClicked(PositionChange));
    }

    private void ButtonWasClicked(Vector3 positionChange)
    {
        Target.localPosition += Target.rotation * positionChange * 0.01f;
    }

}
