using UnityEngine;

// PC-PLA-P3 – the trigger on the finish line.
// RaceManager adds this script to the "FinishLine" object automatically.
public class FinishLine : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        RaceManager race = FindObjectOfType<RaceManager>();
        if (race != null)
        {
            race.PlayerFinished();
        }
    }
}
