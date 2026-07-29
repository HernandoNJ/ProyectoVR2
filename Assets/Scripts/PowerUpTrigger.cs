using System;
using UnityEngine;
using UnityEngine.Events;

public class PowerUpTrigger : MonoBehaviour
{
    public UnityEvent OnPowerupCollected;
    public string powerupCollectedMessage = "Powerup Collected";
    
    // C# event
    public static event Action OnPowerupCollectedEventCode;
    
    private void Start()
    {
        if(OnPowerupCollected != null) OnPowerupCollected.Invoke();
        OnPowerupCollectedEventCode?.Invoke();
    }

    public void ShowPowerupCollectedMessage()
    {
        Debug.Log(powerupCollectedMessage);
    }
}