using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(BoxCollider))]
public class SpawnGrab : XRBaseInteractable
{
    [Header("3D Prefab to Spawn from this 2D Card")]
    [SerializeField] private GameObject prefabToSpawn;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (prefabToSpawn == null) return;

        IXRSelectInteractor interactor = args.interactorObject;
        if (interactor == null) return;

        Transform handTransform = interactor.transform;
        GameObject spawned = Instantiate(
            prefabToSpawn,
            handTransform.position,
            handTransform.rotation
        );

        if (spawned.TryGetComponent<XRGrabInteractable>(out var grabInteractable))
        {

            interactionManager.SelectEnter(interactor, grabInteractable);
        }
    }
}