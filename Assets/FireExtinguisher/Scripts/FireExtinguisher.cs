using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FireExtinguisher : MonoBehaviour
{
    [SerializeField] GameObject foamGo;
    [SerializeField] GameObject colliderGo;
    [Tooltip("Numer of point per second it kills a Fire")]
    public float power;
    XRGrabInteractable xrGrab;
    private readonly object fire;



    // Start is called before the first frame update
    void Start()
    {
        // Récupérer le composant XRGrabInteractable
        xrGrab = GetComponent<XRGrabInteractable>();
        // s’enregistrer sur l’événement activated pour activer le FireExtinguisher 
        xrGrab.activated.AddListener(_ => setActivation(true));
        xrGrab.deactivated.AddListener(_ => setActivation(false));

        // s’enregistrer sur l’événement deactivated pour activer le FireExtinguisher 
        setActivation(false);

    }

    private void setActivation(bool active)
    {
        // Make foam appear / disappear
        foamGo.SetActive(active);

        // Make collider appear / disappear
        colliderGo.SetActive(active);


    }

    private void OnTriggerStay(Collider other)
    {
        // find the fire component hit by the FireExtinguisher
        Fire fire = other.GetComponent<Fire>();

        if (fire != null)
        {
            // Reduce the Fire Health according to the power of the Extinguisher
            fire.Health -= power * Time.deltaTime;
        }
    }
}