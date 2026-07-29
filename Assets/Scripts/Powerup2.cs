using UnityEngine;


public class Powerup2 : MonoBehaviour
{
    private void OnEnable()
    {
        PowerUpTrigger.OnPowerupCollectedEventCode += ShowNewPowerupCollectedEventCodeMessage;
    }

    private void OnDisable()
    {
        PowerUpTrigger.OnPowerupCollectedEventCode -= ShowNewPowerupCollectedEventCodeMessage;
    }

    private void ShowNewPowerupCollectedEventCodeMessage()
    {
        Debug.Log("Showing NEW Powerup Collected Event Code Message");
    }
}