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
    [SerializeField] private int maxMessage = 13;

    private void Start()
    {
        inputField.onSubmit.AddListener(SendMessage);
    }
    
    private void SendMessage(string text)
    {
        if (text != "")
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
        SendMessageClients(text);
    }
    
    [ObserversRpc(BufferLast = true)]
    private void SendMessageClients(string text)
    {
        var mobj = Instantiate(messagePrefab, transform.transform.position, transform.transform.rotation, messagePanel.transform);
        mobj.GetComponent<MessageScript>().SetMassage("Player", text);
        if (messagePanel.transform.childCount > maxMessage)
        {
            Destroy(messagePanel.transform.GetChild(0).gameObject);
        }
    }
 
}
