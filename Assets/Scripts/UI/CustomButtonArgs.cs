using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomButtonArgs : MonoBehaviour
{
    protected Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        AssignButtonEvents();
    }

    protected virtual void AssignButtonEvents()
    {
        button.onClick.AddListener(() => ButtonWasClicked());
    }

    protected virtual void ButtonWasClicked()
    {
        Debug.Log("Button was clicked");
    }
}
