using com.lineact.lit.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using UnityEngine.XR.Interaction.Toolkit;

public class UIAction : MonoBehaviour
{
    [SerializeField] GameObject tiago;
    [SerializeField] Transform xrRig;
    [SerializeField] State patrolState;
    [SerializeField] XRRayInteractor leftRayInteractor, rightRayInteractor;
    [SerializeField] InputActionReference activateLeft, activateRight;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Calling()
    {
        var callRobotConfig = tiago.GetComponent<CallRobotConfig>();
        callRobotConfig?.CallMe(xrRig.transform.position + xrRig.transform.right);
    }

    public void Patrol()
    {
        var stateMachine = tiago.GetComponent<BaseStateMachine>();
        stateMachine?.ChangeState(patrolState);
    }

    /// <summary>
    /// Method to call when you want to specify the position where the robot must go to
    /// </summary>
    public void GoToInit()
    {
        activateLeft.action.started -= Action_started;
        activateRight.action.started -= Action_started;

        activateLeft.action.started += Action_started;
        activateRight.action.started += Action_started;

        // the next trigger will get the position of the ray interactor and ask the robot to move to.
    }

    private void Action_started(InputAction.CallbackContext obj)
    {
        activateLeft.action.started -= Action_started;
        activateRight.action.started -= Action_started;

        Vector3 pos, normal;
        if (rightRayInteractor.TryGetHitInfo(out pos, out normal, out _, out _))
        {
            Debug.Log($"At {pos}");
            GoToPosition(pos);
        }
    }

    public void GoToPosition(Vector3 position)
    {
        var callRobotConfig = tiago.GetComponent<CallRobotConfig>();
        callRobotConfig?.CallMe(position);
    }
}

