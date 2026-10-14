using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(BoxCollider))]
public class SpawnGrabDispenser : XRBaseInteractable
{
    [Header("3D Prefab to Spawn from this 2D Card")]
    [SerializeField] private GameObject prefabToSpawn;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (prefabToSpawn == null) return;

        // 1. Get the controller/ray interactor that grabbed this 2D card
        IXRSelectInteractor interactor = args.interactorObject;
        if (interactor == null) return;

        // 2. Position the new object directly at the hand/controller transform
        Transform handTransform = interactor.transform;
        GameObject spawned = Instantiate(
            prefabToSpawn,
            handTransform.position,
            handTransform.rotation
        );

        // 3. Hand over the new object to the same controller immediately
        if (spawned.TryGetComponent<XRGrabInteractable>(out var grabInteractable))
        {

            // Grab the real 3D platform or trap
            interactionManager.SelectEnter(interactor, grabInteractable);
        }
    }
}