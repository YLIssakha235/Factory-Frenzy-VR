using System.Collections;
using UnityEngine;
using Cinemachine;

// Camera flight around the whole level before the countdown.
// Put it on an empty GameObject called "LevelIntro". RaceManager plays it automatically.
public class LevelIntro : MonoBehaviour
{
    public float duration = 5f;  // seconds
    public float turns = 0.75f;  // 0.75 = three quarters of a circle

    public IEnumerator Play()
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        // Cinemachine normally moves the camera: we turn it off during the intro
        CinemachineBrain brain = cam.GetComponent<CinemachineBrain>();
        if (brain != null) brain.enabled = false;

        // the level goes from the Start platform to the Finish platform
        Vector3 start = Vector3.zero;
        Vector3 finish = Vector3.zero;
        StartLightsFX startPlatform = FindObjectOfType<StartLightsFX>();
        FinishFX finishPlatform = FindObjectOfType<FinishFX>();
        if (startPlatform != null) start = startPlatform.transform.position;
        if (finishPlatform != null) finish = finishPlatform.transform.position;

        Vector3 center = (start + finish) / 2;
        float levelSize = Vector3.Distance(start, finish);
        float radius = levelSize * 0.6f + 10;
        float height = levelSize * 0.35f + 8;

        // where the camera must end: behind the player
        PlayerPhysics player = FindObjectOfType<PlayerPhysics>();
        Vector3 endPosition = center;
        Vector3 endLookAt = center;
        if (player != null)
        {
            endPosition = player.transform.position - player.transform.forward * 5 + Vector3.up * 2.5f;
            endLookAt = player.transform.position + Vector3.up * 1.2f;
        }

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float u = time / duration; // 0 at the start, 1 at the end

            // 1. turn around the level, high in the sky
            float angle = u * turns * 360;
            Vector3 orbitPosition = center + Quaternion.Euler(0, angle, 0) * new Vector3(0, height, -radius);

            // 2. in the last 30%, come down smoothly behind the player
            float landing = Mathf.Clamp01((u - 0.7f) / 0.3f);
            landing = landing * landing * (3 - 2 * landing); // smooth start and stop

            cam.transform.position = Vector3.Lerp(orbitPosition, endPosition, landing);
            Vector3 lookAt = Vector3.Lerp(center, endLookAt, landing);
            cam.transform.LookAt(lookAt);

            yield return null; // wait for the next frame
        }

        // give the camera back to Cinemachine (it follows the player again)
        if (brain != null) brain.enabled = true;
    }
}
