using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Added for XR components

public class Fire : MonoBehaviour
{
    [Tooltip("The maximum value of health")]
    [SerializeField] private float maxHealth;

    [Tooltip("How much health per secondes the fire gain when not extinguished")]
    [SerializeField] private float healthIncreasePerSec;

    [Tooltip("What scale to apply according to the health")]
    [SerializeField] private float scalePerHealth;

    [SerializeField] private float health;
    public float Health
    {
        get { return health; }
        set
        {
            health = value;
            UpdateHealth();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (health > 0 && health < maxHealth)
        {
            health += healthIncreasePerSec * Time.deltaTime;
        }
    }

    void UpdateHealth()
    {
        // activate/deactivate gameObject if health > 0
        gameObject.SetActive(health > 0);

        // --- NEW LOGIC: Unlock the head if the fire is put out ---
        if (health <= 0)
        {
            UnlockRobotHead();
        }

        // Gain health if fire is not fully extinguished
        if (health > 0 && health < maxHealth)
        {
            health += healthIncreasePerSec * Time.deltaTime;
        }

        // Update scale according to Health
        transform.localScale = Vector3.one * health * scalePerHealth;
    }

    // --- NEW METHOD: Finds the head and enables grabbing ---
    private void UnlockRobotHead()
    {
        GameObject robotHead = GameObject.Find("head_1_link");
        if (robotHead != null)
        {
            XRGrabInteractable grabInteractable = robotHead.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.enabled = true;
            }
        }
    }
}