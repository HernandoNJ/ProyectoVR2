using System;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    private void OnEnable()
    {
        PowerUpTrigger.OnPowerupCollectedEventCode += ShowPowerupCollectedEventCodeMessage;
    }

    private void OnDisable()
    {
        PowerUpTrigger.OnPowerupCollectedEventCode -= ShowPowerupCollectedEventCodeMessage;
    }

    private void ShowPowerupCollectedEventCodeMessage()
    {
        Debug.Log("Showing Powerup Collected Event Code Message");
    }
}
