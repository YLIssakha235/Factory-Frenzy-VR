using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Gère la caméra spectatrice (FlyCam) du formateur.
/// Par défaut l'écran PC affiche la vue RV ; une touche (ou un bouton UI) bascule sur la FlyCam.
/// La FlyCam a Target Eye = None et une priorité (depth) plus haute que la caméra RV :
/// quand elle est active, elle recouvre le miroir RV sur l'écran de retour, sans rien changer dans le casque.
/// </summary>
public class SpectatorCameraManager : MonoBehaviour
{
    [Header("Caméras")]
    [SerializeField] private Camera vrCamera;   // Main Camera de l'XR Origin
    [SerializeField] private Camera flyCam;     // Caméra issue du unitypackage FlyCam

    [Tooltip("Scripts de déplacement de la FlyCam, désactivés quand elle n'est pas affichée.")]
    [SerializeField] private Behaviour[] flyCamControllers;

    [Header("Bascule")]
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private Key toggleKey = Key.Tab;
#else
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
#endif
    [SerializeField] private bool startInFlyCam = false;

    [Tooltip("À la première activation, place la FlyCam à la position de la tête de l'utilisateur·rice.")]
    [SerializeField] private bool spawnAtVrHead = true;

    public bool IsFlyCamActive { get; private set; }
    public Camera FlyCam => flyCam;
    public event System.Action<bool> OnViewChanged;

    private bool _alreadyPlaced;

    private void Awake()
    {
        // Paramétrage imposé : rendu sur l'écran uniquement, priorité supérieure à la caméra RV.
        flyCam.stereoTargetEye = StereoTargetEyeMask.None;
        flyCam.depth = vrCamera.depth + 1;   // "Priority" dans l'inspecteur en URP

        // Un seul AudioListener dans la scène : celui de la tête RV.
        var listener = flyCam.GetComponent<AudioListener>();
        if (listener != null) listener.enabled = false;

        SetFlyCam(startInFlyCam);
    }

    private void Update()
    {
        if (TogglePressed()) Toggle();
    }

    private bool TogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame;
#else
        return Input.GetKeyDown(toggleKey);
#endif
    }

    /// <summary>Appelable depuis un bouton UI de l'écran de retour.</summary>
    public void Toggle() => SetFlyCam(!IsFlyCamActive);

    public void SetFlyCam(bool active)
    {
        IsFlyCamActive = active;

        if (active && spawnAtVrHead && !_alreadyPlaced)
        {
            // Move the whole FlyCam object (the parent holding the FlyCam script),
            // not the child camera, otherwise it orbits around its parent when turning.
            Transform rig = flyCam.transform.parent != null ? flyCam.transform.parent : flyCam.transform;
            rig.SetPositionAndRotation(vrCamera.transform.position,
                Quaternion.Euler(0f, vrCamera.transform.eulerAngles.y, 0f));
            if (rig != flyCam.transform)
                flyCam.transform.localPosition = Vector3.zero;
            _alreadyPlaced = true;
        }

        flyCam.enabled = active;
        foreach (var c in flyCamControllers)
            if (c != null) c.enabled = active;

        // Le formateur a besoin du curseur pour cliquer.
        Cursor.visible = active;
        Cursor.lockState = CursorLockMode.None;

        OnViewChanged?.Invoke(active);
    }
}