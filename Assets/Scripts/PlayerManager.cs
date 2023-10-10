using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : MonoBehaviour
{
    // Components
    public static PlayerManager Instance { get; private set; }
    public InputManager Inputs { get; private set; }
    public Rigidbody RigidBody { get; private set; }
    public Animator Animator { get; private set; }

    public CameraController PlayerCam { get; private set; }

    public StateManager StateManager { get; private set; }
    
    // Controls
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 6.5f;
    public float fallingSpeed = 30f;
    public float inAirTime;
    public float movementMultiplier = 1.0f;
    
    // Grounded Checks
    public bool isGrounded;
    public float groundedOffset = -0.30f; // should be between -0.3 and 0.4f 
    private LayerMask _groundLayers;

    
    // -------------- Animator Stuff -------------- //

    public float AnimatorStateTime { get; private set; }
    public bool IsAnimatorTransitioning { get; private set; }
    public AnimatorStateInfo AnimatorCurrentState { get; private set; }
    public AnimatorStateInfo AnimatorNextState { get; private set; }
    
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Inputs = InputManager.Instance;
        PlayerCam = CameraController.Instance;
        RigidBody = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();
        StateManager = new StateManager();
        // set a max velocity (12 m/s)
        // RigidBody.maxLinearVelocity = 12f;
        // set ground layers
        _groundLayers = LayerMask.GetMask("Environment");

    }

    // Update is called once per frame
    void Update()
    {
        StateManager.LogicUpdate();
    }

    private void FixedUpdate()
    {
        CacheAnimatorInfo();
        StateManager.PhysicsUpdate();
    }

    public void HandleRotation()
    {
        // calculate player rotation based on camera rotation
        Vector3 targetDirection = (PlayerCam.transform.forward * Inputs.MoveInput.z) 
                                  + (PlayerCam.transform.right * Inputs.MoveInput.x);
        targetDirection.Normalize();
        // set rotation to face forward if player is idle
        if (targetDirection == Vector3.zero) targetDirection = transform.forward;
        // set and smooth the rotation
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion smoothRotation = Quaternion.Slerp(transform.rotation, targetRotation, 15 * Time.deltaTime);
        // rotate the player
        transform.rotation = smoothRotation;
    }
    
    public void HandleMovement()
    {
        // calculate horizontal movement based on camera direction
        Vector3 targetDirection = (PlayerCam.transform.forward * Inputs.MoveInput.z) 
                                  + (PlayerCam.transform.right * Inputs.MoveInput.x);
        targetDirection.Normalize();
        // calculate force based on sprint vs walk
        moveSpeed = Inputs.SprintInput ? sprintSpeed : walkSpeed;
        Vector3 velocity = targetDirection * moveSpeed;
        // move the rigidbody via setting the velocity (results in instant change)
        RigidBody.velocity = velocity;
        // snap animation motion speeds for better animation
        var horizontal = Inputs.SprintInput ? Inputs.MoveInput.x : Inputs.MoveInput.x / 2; 
        var vertical = Inputs.SprintInput ? Inputs.MoveInput.z : Inputs.MoveInput.z / 2;
        horizontal = horizontal switch
        {
            > 0 and < 0.55f => 0.5f,
            > 0 and > 0.55f => 1f,
            < 0 and > -0.55f => -0.5f,
            < 0 and < -0.55f => -1f,
            _ => 0f
        };
        vertical = vertical switch
        {
            > 0 and < 0.55f => 0.5f,
            > 0 and > 0.55f => 1f,
            < 0 and > -0.55f => -0.5f,
            < 0 and < -0.55f => -1f,
            _ => 0f
        };
        // animate motion
        Animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        Animator.SetFloat("Vertical", vertical, 0.1f, Time.deltaTime);
    }

    public void GroundedCheck()
    {
        // grounded check via ray casting
        isGrounded = Physics.Raycast(
            new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
            -Vector3.up,
            out var hit,
            1f,
            _groundLayers);
        // if the player is grounded
        if (isGrounded) {
            // snap the y-position of the player to hit-point's y-position
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            // reset the in-air timer
            inAirTime = 0;
        }        
    }

    private void CacheAnimatorInfo()
    {
            AnimatorCurrentState = Animator.GetCurrentAnimatorStateInfo(0);
            AnimatorNextState = Animator.GetNextAnimatorStateInfo(0);
            IsAnimatorTransitioning = Animator.IsInTransition(0);
            AnimatorStateTime = Mathf.Clamp01(AnimatorCurrentState.normalizedTime);
            Animator.SetFloat("StateTime", AnimatorStateTime);
    }

    public void ResetAnimatorStateTime()
    {
        AnimatorStateTime = 0;
        Animator.SetFloat("StateTime", AnimatorStateTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), 0.25f);
    }
}