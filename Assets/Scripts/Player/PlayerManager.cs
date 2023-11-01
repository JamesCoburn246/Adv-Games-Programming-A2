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
    public WeaponManager Weapon { get; private set; }

    public PlayerStats Stats { get; private set; }
    public Rigidbody RigidBody { get; private set; }
    public Animator Animator { get; private set; }

    public CameraController PlayerCam { get; private set; }

    public PlayerStateManager StateManager { get; private set; }

    public Transform lookTransform;
    
    // Inputs
    public Vector3 Movement { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsSprinting { get; private set; }

    public bool IsDead { get; set; }
    public bool IsVictorious { get; set; }

    // Controls
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 6.5f;
    public float fallingSpeed = 30f;
    public float weaponDamage = 10;
    public float inAirTime;
    
    // Grounded Checks
    public bool IsGrounded { get; private set; }
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
    private void Start()
    {
        Inputs = InputManager.Instance;
        PlayerCam = CameraController.Instance;
        RigidBody = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();
        StateManager = new PlayerStateManager();
        Stats = GetComponent<PlayerStats>();
        Weapon = GetComponentInChildren<WeaponManager>();
        Weapon.SetDamage(weaponDamage);
        lookTransform = transform.GetChild(0);
        _groundLayers = LayerMask.GetMask("Environment");

    }

    // Update is called once per frame
    private void Update()
    {
        HandleAllInputs();
        StateManager.LogicUpdate();
    }

    private void FixedUpdate()
    {
        CacheAnimatorInfo();
        StateManager.PhysicsUpdate();
    }

    private void HandleAllInputs()
    {
        Movement = IsDead || IsVictorious ? Vector3.zero : Inputs.MoveInput;
        IsAttacking =  Inputs.AttackInput && !IsDead && !IsVictorious;
        IsSprinting = Inputs.SprintInput && Stats.HasStamina();
    }

    public void HandleRotation()
    {
        // calculate player rotation based on camera rotation
        Vector3 targetDirection = (PlayerCam.transform.forward * Movement.z) 
                                  + (PlayerCam.transform.right * Movement.x);
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
        Vector3 targetDirection = (PlayerCam.transform.forward * Movement.z) + (PlayerCam.transform.right * Movement.x);
        targetDirection.Normalize();
        // calculate force based on sprint vs walk
        moveSpeed = IsSprinting ? sprintSpeed : walkSpeed;
        Vector3 velocity = targetDirection * moveSpeed;
        // move the rigidbody via setting the velocity (results in instant change)
        RigidBody.velocity = velocity;
        // snap animation motion speeds for better animation
        var horizontal = IsSprinting ? Movement.x : Movement.x / 2; 
        var vertical = IsSprinting ? Movement.z : Movement.z / 2;
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

    public void ResetMovement()
    {
        Movement = Vector3.zero;
        RigidBody.velocity = Vector3.zero;
    }

    public void GroundedCheck()
    {
        // grounded check via ray casting
        IsGrounded = Physics.Raycast(
            new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
            -Vector3.up,
            out var hit,
            1f,
            _groundLayers);
        // if the player is grounded
        if (IsGrounded) {
            // snap the y-position of the player to hit-point's y-position
            transform.position = new Vector3(transform.position.x, hit.point.y + 0.035f, transform.position.z);
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
    
    // Animation Events
    
    public void EnableDamage()
    {
        Weapon.EnableDamage();
    }

    public void DisableDamage()
    {
        Weapon.DisableDamage();
    }

}
