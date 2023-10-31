using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(EnemyStats))]
public class EnemyManager : MonoBehaviour
{
    private WeaponManager Weapon { get; set; }
    public NavMeshAgent Agent { get; private set; }
    public Animator Animator { get; private set; }
    public EnemyStateManager StateManager { get; set; }
    private EnemyStats stats;

    private static PlayerManager Player { get; set; }

    public Transform lookTransform;

    public LayerMask enemyExcludeMask;
    private Ray _rayToPlayer;
    public RaycastHit rayHit;
    public float fieldOfView = 55.0f;
    
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 6.5f;

    [Header("AI Settings")] 
    public Transform[] patrolPoints;

    public float attackDistance = 1.5f;
    public float detectionDistance = 7.5f;

    private float _timePassed;
    public float idleCooldownTime = 5f;
    public float destCooldownTime = 0.1f;
    public float attackCooldownTime = 3f;

    // This state is now handled by the EnemyStats class.
    public bool IsDead
    {
        get
        {
            return !stats.IsAlive();
        }
        set
        {
            if (value)
            {
                stats.Die();
            }
            else
            {
                stats.Revive();
            }
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        stats = GetComponent<EnemyStats>();
        Weapon = GetComponentInChildren<WeaponManager>();
        Weapon.SetDamage(25);
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        StateManager = new EnemyStateManager(this);
        Player = PlayerManager.Instance;
        lookTransform = transform.GetChild(0);
        enemyExcludeMask = ~LayerMask.GetMask("Enemy");
        Agent.updateRotation = false;
        Agent.speed = walkSpeed;
        Agent.stoppingDistance = attackDistance;
        _timePassed = destCooldownTime;
    }

    // Update is called once per frame
    private void Update()
    {
        _timePassed -= Time.deltaTime;
        StateManager.LogicUpdate();
    }

    private void FixedUpdate()
    {
        StateManager.PhysicsUpdate();
    }
    
    public void HandleRotation(bool isLockedOn)
    {
        Vector3 targetDirection;
        if (!isLockedOn || Player.IsDead)
        {
            // calculate agent direction based on velocity
            targetDirection = Agent.velocity.normalized;
            // set rotation to face forward if agent is idle
            if (targetDirection == Vector3.zero) targetDirection = transform.forward;
        }
        else
        {
            // calculate agent direction based on player's position
            targetDirection = (Player.transform.position - transform.position).normalized;
            // set rotation to face forward if agent is idle
            // if (targetDirection == Vector3.zero) targetDirection = transform.forward;
        }
        // set and smooth the rotation
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion smoothRotation = Quaternion.Slerp(transform.rotation, targetRotation, 10 * Time.deltaTime);
        // rotate the agent
        transform.rotation = smoothRotation;
    }

    public void HandleMovement()
    {
        moveSpeed = StateManager.CurrentState == StateManager.chaseState ? sprintSpeed : walkSpeed;
        Agent.speed = moveSpeed;
        // snap animation motion speeds for better animation
        var horizontal = StateManager.CheckState(StateManager.chaseState) ? Agent.velocity.normalized.x : Agent.velocity.normalized.x / 2; 
        var vertical = StateManager.CheckState(StateManager.chaseState) ? Agent.velocity.normalized.z : Agent.velocity.normalized.z / 2;
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

    public bool IsPlayerInView()
    {
        if (Player.IsDead) return false;
        _rayToPlayer = new Ray(lookTransform.position,Player.lookTransform.position - lookTransform.position);
        return Mathf.Abs(Vector3.Angle(lookTransform.forward, _rayToPlayer.direction)) <= fieldOfView;
    }

    public bool RayCastToPlayer(float maxDistance)
    {
        if (Player.IsDead) return false;
        _rayToPlayer = new Ray(lookTransform.position,Player.lookTransform.position - lookTransform.position);
        return Physics.Raycast(_rayToPlayer, out rayHit, maxDistance, enemyExcludeMask);
    }

    public void SetDestinationToPlayer()
    {
        if (!(_timePassed <= 0)) return;
        Agent.SetDestination(Player.transform.position);
        _timePassed = destCooldownTime;
        
    }

    public void Die()
    {
        KillsIndicator.Instance.IncrementCount();
        if (KillsIndicator.Instance.killsCount == 3)
        {
            Player.IsVictorious = true;
            Player.StateManager.SwitchState(Player.StateManager.victoryState);
        }
        Destroy(gameObject);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (lookTransform != null)
        {
            Gizmos.DrawRay(lookTransform.position, _rayToPlayer.direction.normalized * detectionDistance);
        }

        if (StateManager != null && StateManager.CheckState(StateManager.idleState))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
        if (StateManager != null && StateManager.CheckState(StateManager.patrolState))
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
        if (StateManager != null && StateManager.CheckState(StateManager.chaseState))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}
