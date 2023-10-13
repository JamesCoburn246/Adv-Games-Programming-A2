using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class EnemyManager : MonoBehaviour
{
    public WeaponManager Weapon { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public Animator Animator { get; private set; }
    public EnemyStateManager StateManager { get; private set; }

    public Transform lookTransform;
    
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 6.5f;
    [FormerlySerializedAs("isChasing")] public bool isLockedOn;

    [Header("AI Settings")] 
    public Transform[] patrolPoints;

    public float timePassed;
    [FormerlySerializedAs("idleTime")] public float idleCooldownTime = 5f;
    public float destCooldownTime = 0.2f;
    public float attackCooldownTime = 2f;
    public float detectionDistance = 10f;
    public float attackDistance = 1f;
    public float stoppingDistance = 0.5f;

    
    // Start is called before the first frame update
    void Start()
    {
        Weapon = GetComponentInChildren<WeaponManager>();
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        StateManager = new EnemyStateManager(this);
        lookTransform = transform.GetChild(0);
        //
        Agent.updateRotation = false;
        // Agent.autoBraking = false;
        isLockedOn = false;
        Agent.speed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        StateManager.LogicUpdate();
    }

    private void FixedUpdate()
    {
        HandleRotation();
        HandleMovement();
        StateManager.PhysicsUpdate();
    }
    
    public void HandleRotation()
    {
        Vector3 targetDirection;
        if (!isLockedOn)
        {
            // calculate agent direction based on velocity
            targetDirection = Agent.velocity.normalized;
            // set rotation to face forward if agent is idle
            if (targetDirection == Vector3.zero) targetDirection = transform.forward;
        }
        else
        {
            // calculate agent direction based on player's position
            targetDirection = PlayerManager.Instance.transform.position - transform.position;
        }
        // set and smooth the rotation
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion smoothRotation = Quaternion.Slerp(transform.rotation, targetRotation, 15 * Time.deltaTime);
        // rotate the agent
        transform.rotation = smoothRotation;
    }

    public void HandleMovement()
    {
        moveSpeed = isLockedOn ? sprintSpeed : walkSpeed;
        Agent.speed = moveSpeed;
        // snap animation motion speeds for better animation
        var horizontal = isLockedOn ? Agent.velocity.normalized.x : Agent.velocity.normalized.x / 2; 
        var vertical = isLockedOn ? Agent.velocity.normalized.z : Agent.velocity.normalized.z / 2;
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
        Animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        Animator.SetFloat("Vertical", vertical, 0.1f, Time.deltaTime);
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
