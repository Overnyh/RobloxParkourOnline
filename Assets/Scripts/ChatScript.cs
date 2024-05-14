using System;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ChatScript : NetworkBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int maxMessage = 13;

    private void Start()
    {
        inputField.onSubmit.AddListener(SendMessage);
        scrollRect.verticalNormalizedPosition = 0f;
    }
    
    private void SendMessage(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            inputField.text = "";
            SendMessageServer(text);
        }
    }

    
    [ServerRpc(RequireOwnership = false)]
    private void SendMessageServer(string text)
    {
        var mobj = Instantiate(messagePrefab, transform.transform.position, transform.transform.rotation, messagePanel.transform);
        mobj.GetComponent<MessageScript>().SetMassage("Player", text);
        if (messagePanel.transform.childCount > maxMessage)
        {
            Destroy(messagePanel.transform.GetChild(0).gameObject);
        }
        SendMessageClients(text, OwnerId);
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
    
    [ObserversRpc(BufferLast = true)]
    private void SendMessageClients(string text, int id)
    {
        if(id == OwnerId) return;
        var mobj = Instantiate(messagePrefab, transform.transform.position, transform.transform.rotation, messagePanel.transform);
        mobj.GetComponent<MessageScript>().SetMassage("Player", text);
        if (messagePanel.transform.childCount > maxMessage)
        {
            Destroy(messagePanel.transform.GetChild(0).gameObject);
        }
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
 
}
