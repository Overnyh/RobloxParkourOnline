using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MessageScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI authorUI;
    [SerializeField] private TextMeshProUGUI messageUI;

    public void SetMassage(string author, string message)
    {
        authorUI.text = author;
        messageUI.text = message;
    }
}
