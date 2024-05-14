using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MessageScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textUI;

    private void Start()
    {
        var rectTransform = GetComponent<RectTransform>();
        Vector2 size = rectTransform.sizeDelta;

        // Update the width component of the size
        size.x = 700;

        // Apply the new size to the RectTransform
        rectTransform.sizeDelta = size;
    }

    public void SetMassage(string author, string message)
    {
        textUI.text = author + ": " + message;
    }
}
