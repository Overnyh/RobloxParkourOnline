using System;
using System.Linq;
using FishNet.Component.Spawning;
using FishNet.Object;
using FishNet.Transporting.Bayou;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using UnityEngine;

public class FishNetStartup : MonoBehaviour
{
    
    private PlayerSpawner ps;
    private void Start()
    {
        Multipass mp = GetComponent<Multipass>();
        
#if UNITY_WEBGL && !UNITY_EDITOR
        mp.SetClientTransport<Bayou>();
#else
        mp.SetClientTransport<Tugboat>();
#endif
    }
    
}
