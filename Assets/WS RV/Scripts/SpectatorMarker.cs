using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Quand le formateur clique dans la vue FlyCam, un marqueur est placé à l'endroit visé
/// dans la scène 3D. Le marqueur étant un objet du monde, il est visible dans le casque.
/// </summary>
public class SpectatorMarker : MonoBehaviour
{
    [SerializeField] private SpectatorCameraManager manager;
    [SerializeField] private GameObject markerPrefab;

    [Header("Raycast")]
    [Tooltip("Exclure les calques de l'XR Origin / mains et celui du marqueur.")]
    [SerializeField] private LayerMask raycastMask = ~0;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float surfaceOffset = 0.01f;

    [Header("Comportement")]
    [Tooltip("Durée d'affichage en secondes (0 = reste jusqu'au prochain clic).")]
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private bool singleMarker = true;
    [Tooltip("Son spatialisé joué au point cliqué, pour orienter l'utilisateur·rice.")]
    [SerializeField] private AudioClip clickSound;

    private GameObject _current;

    private void Update()
    {
        if (manager == null || !manager.IsFlyCamActive) return;
        if (!LeftClickDown(out Vector2 screenPos)) return;

        // Ignore les clics sur l'UI de l'écran de retour (bouton de bascule, etc.).
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // Cursor locked (FPS-style look): aim from the crosshair, i.e. the centre of the FlyCam's view.
        if (Cursor.lockState == CursorLockMode.Locked)
            screenPos = manager.FlyCam.pixelRect.center;

        // In picture-in-picture mode, only clicks inside the FlyCam's window count.
        if (!manager.FlyCam.pixelRect.Contains(screenPos)) return;

        Ray ray = manager.FlyCam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastMask, QueryTriggerInteraction.Ignore))
            Place(hit.point + hit.normal * surfaceOffset, hit.normal);
    }

    private bool LeftClickDown(out Vector2 pos)
    {
#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        pos = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
        return mouse != null && mouse.leftButton.wasPressedThisFrame;
#else
        pos = Input.mousePosition;
        return Input.GetMouseButtonDown(0);
#endif
    }

    private void Place(Vector3 position, Vector3 normal)
    {
        if (singleMarker) ClearMarker();

        _current = Instantiate(markerPrefab, position, Quaternion.FromToRotation(Vector3.up, normal));
        if (lifetime > 0f) Destroy(_current, lifetime);

        if (clickSound != null) AudioSource.PlayClipAtPoint(clickSound, position);
    }

    public void ClearMarker()
    {
        if (_current != null) Destroy(_current);
        _current = null;
    }
}
