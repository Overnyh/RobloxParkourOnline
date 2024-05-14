using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;


public class MapManager : NetworkBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float timeToReGen;

    [SyncObject] private readonly SyncTimer _timeRemaining = new SyncTimer();

    public override void OnStartServer()
    {
        base.OnStartServer();
        _timeRemaining.StartTimer(timeToReGen, true);
        _timeRemaining.OnChange += _timeRemaining_OnChange;
    }
    
    private void OnDestroy()
    {
        _timeRemaining.OnChange -= _timeRemaining_OnChange;
    }

    private void _timeRemaining_OnChange(SyncTimerOperation op, float prev, float next, bool asServer)
    {
        if (op == SyncTimerOperation.Finished)
        {
            Debug.Log($"The timer has completed!{prev}");
            mapGenerator.GenerateMap();
            _timeRemaining.StartTimer(timeToReGen, true);
        }
            
        
    }

    private void FixedUpdate()
    {
        _timeRemaining.Update(Time.deltaTime);
        timerText.text = _timeRemaining.Remaining.ToString("0");
    }
}