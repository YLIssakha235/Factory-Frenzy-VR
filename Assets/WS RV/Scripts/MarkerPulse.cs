using UnityEngine;

/// <summary>
/// Anime le marqueur (pulsation + flottement + rotation) pour qu'il attire l'œil
/// dans le champ de vision périphérique de l'utilisateur·rice.
/// </summary>
public class MarkerPulse : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float pulseAmount = 0.25f;
    [SerializeField] private float bobHeight = 0.1f;
    [SerializeField] private float spinSpeed = 90f;
    [Tooltip("Enfant visuel animé (le root reste collé à la surface).")]
    [SerializeField] private Transform visual;

    private Vector3 _baseScale;
    private Vector3 _baseLocalPos;

    private void Awake()
    {
        if (visual == null) visual = transform;
        _baseScale = visual.localScale;
        _baseLocalPos = visual.localPosition;
    }

    private void Update()
    {
        float t = Time.time * pulseSpeed;
        visual.localScale = _baseScale * (1f + Mathf.Sin(t) * pulseAmount);
        visual.localPosition = _baseLocalPos + Vector3.up * (Mathf.Sin(t * 0.5f) * bobHeight);
        visual.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
    }
}
