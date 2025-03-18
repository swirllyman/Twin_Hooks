using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLevelStats : MonoBehaviour
{
    public delegate void HealthChangeCallback(int _h);
    public event HealthChangeCallback onHealthChanged;

    public int playerHealth = 5;
    internal int currentSpecialCount = 0;
    internal void UpdateSpecialCount(int amountToAdd)
    {
        currentSpecialCount += amountToAdd;
    }



    //Event delegate to call onHealthChanged when hit by enemy
}
