using System.Collections;
using UnityEngine;

// TEST ONLY – shows buttons on the screen in Play mode to test every animation.
// Put it on any object in the scene. Remove it from the final game.
public class FXTester : MonoBehaviour
{
    int launcherState = 0; // 0 = search, 1 = orient, 2 = fire

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 600));
        GUILayout.Label("ANIMATION TESTS");

        if (GUILayout.Button("Bumper: Pulse" + Check<BumperFX>()))
        {
            BumperFX bumper = FindObjectOfType<BumperFX>();
            if (bumper != null) bumper.Pulse();
        }

        if (GUILayout.Button("Trampoline: Bounce" + Check<TrampolineFX>()))
        {
            TrampolineFX trampoline = FindObjectOfType<TrampolineFX>();
            if (trampoline != null) trampoline.Bounce();
        }

        if (GUILayout.Button("Launcher: next eye color" + Check<LauncherFX>()))
        {
            LauncherFX launcher = FindObjectOfType<LauncherFX>();
            if (launcher != null)
            {
                launcherState = (launcherState + 1) % 3; // search → orient → fire → search
                if (launcherState == 0) launcher.ShowSearch();
                if (launcherState == 1) launcher.ShowOrient();
                if (launcherState == 2) launcher.ShowFire();
            }
        }

        if (GUILayout.Button("Launcher: Recoil" + Check<LauncherFX>()))
        {
            LauncherFX launcher = FindObjectOfType<LauncherFX>();
            if (launcher != null) launcher.Recoil();
        }

        if (GUILayout.Button("Start lights: countdown" + Check<StartLightsFX>()))
        {
            StartCoroutine(TestCountdown());
        }

        if (GUILayout.Button("Finish: confetti" + Check<FinishFX>()))
        {
            FinishFX finish = FindObjectOfType<FinishFX>();
            if (finish != null) finish.Celebrate();
        }

        if (GUILayout.Button("Checkpoint: Activate" + Check<CheckpointFX>()))
        {
            CheckpointFX checkpoint = FindObjectOfType<CheckpointFX>();
            if (checkpoint != null) checkpoint.Activate();
        }

        if (GUILayout.Button("Bean: Celebrate" + Check<BeanAnimator>()))
        {
            BeanAnimator bean = FindObjectOfType<BeanAnimator>();
            if (bean != null) bean.Celebrate();
        }

        if (GUILayout.Button("Bean: get hit" + Check<PlayerPhysics>()))
        {
            PlayerPhysics player = FindObjectOfType<PlayerPhysics>();
            if (player != null)
            {
                // push backwards and a bit up, like a launcher hit
                Vector3 push = -player.transform.forward * 10 + Vector3.up * 4;
                player.AddImpulse(push);
            }
        }

        GUILayout.EndArea();
    }

    // shows " (MISSING)" next to a button when the scene has no object with that script
    string Check<T>() where T : Component
    {
        if (FindObjectOfType<T>() == null) return "  (MISSING)";
        return "";
    }

    IEnumerator TestCountdown()
    {
        StartLightsFX lights = FindObjectOfType<StartLightsFX>();
        if (lights == null) yield break;

        for (int number = 3; number >= 0; number--)
        {
            lights.SetCountdown(number);
            yield return new WaitForSeconds(1);
        }
    }
}
