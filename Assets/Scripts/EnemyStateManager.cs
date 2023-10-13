using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyStateManager
{
    protected EnemyManager enemy;
    protected State CurrentState;

    public PatrolState patrolState;
    public IdleState idleState;
    public ChaseState chaseState;
    public AttackState attackState;
    
    public EnemyStateManager(EnemyManager enemy)
    {
        this.enemy = enemy;
        patrolState = new PatrolState(this);
        idleState = new IdleState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        CurrentState = patrolState;
        CurrentState?.Enter();
    }
    
    public EnemyStateManager(State initialState)
    {
        CurrentState = initialState;
        CurrentState?.Enter();
    }
    
    public void LogicUpdate()
    {
        CurrentState?.Update();
    }

    public void PhysicsUpdate()
    {
        CurrentState?.FixedUpdate();
    }

    public void SwitchState(State newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();
    }
    
    // Base State
    
    public class State
    {
        protected EnemyStateManager stateManager;

        protected State(EnemyStateManager stateManager)
        {
            this.stateManager = stateManager;
        }
        
        // Enter is called when the state is first entered
        public virtual void Enter() {}

        // Update is called once per frame
        public virtual void Update() {}
    
        // FixedUpdate is called once per physics update
        public virtual void FixedUpdate() {}
    
        // Exit is called when the state is exited
        public virtual void Exit() {}
    }
    
    // States

    public class IdleState : State
    {
        public IdleState(EnemyStateManager stateManager) : base(stateManager){}

        public override void Enter()
        {
            stateManager.enemy.timePassed = stateManager.enemy.idleCooldownTime;
        }

        public override void FixedUpdate()
        {
            stateManager.enemy.timePassed -= Time.deltaTime;
            
            if (stateManager.enemy.timePassed <= 0)
            {
                stateManager.SwitchState(stateManager.patrolState);
            }
            // generate a ray from the enemy to the player
            Ray ray = new Ray(stateManager.enemy.lookTransform.position, PlayerManager.Instance.lookTransform.position - stateManager.enemy.lookTransform.position);
        
            // check if the player is within the field of view
            if (Mathf.Abs(Vector3.Angle(stateManager.enemy.lookTransform.forward, ray.direction)) <= 55.0f)
            {
                Debug.DrawRay(stateManager.enemy.lookTransform.position, PlayerManager.Instance.lookTransform.position - stateManager.enemy.lookTransform.position, Color.blue);
                
                // if it is, perform a raycast
                if (Physics.Raycast(ray, out var hit, stateManager.enemy.detectionDistance)) {
                    // Check whether the player is in the guard's line of sight
                    if (hit.collider.CompareTag("PlayerHitbox"))
                    {
                        stateManager.SwitchState(stateManager.chaseState);
                    }
                }
            
            }
        }
        
        
    }

    public class PatrolState : State
    {
        private int _destPoint;

        public PatrolState(EnemyStateManager stateManager) : base(stateManager)
        {
        }

        public override void Enter()
        {
            // stateManager.enemy.Agent.stoppingDistance = stateManager.enemy.stoppingDistance;
            GoToNextPoint();
        }

        public override void FixedUpdate()
        {
            if (stateManager.enemy.Agent.remainingDistance < stateManager.enemy.Agent.stoppingDistance)
            {
                stateManager.SwitchState(stateManager.idleState);
            }
            // generate a ray from the enemy to the player
            Ray ray = new Ray(stateManager.enemy.lookTransform.position, PlayerManager.Instance.lookTransform.position - stateManager.enemy.lookTransform.position);
        
            // check if the player is within the field of view
            if (Mathf.Abs(Vector3.Angle(stateManager.enemy.lookTransform.forward, ray.direction)) <= 55.0f)
            {
                Debug.DrawRay(stateManager.enemy.lookTransform.position + Vector3.up, PlayerManager.Instance.lookTransform.position - stateManager.enemy.lookTransform.position, Color.blue);
                // if it is, perform a raycast towards the player
                if (Physics.Raycast(ray, out var hit, stateManager.enemy.detectionDistance)) {
                    // If the player is in the guard's line of sight, start chasing
                    if (hit.collider.CompareTag("PlayerHitbox")) stateManager.SwitchState(stateManager.chaseState);
                }
            
            }
        
        }
        
        private void GoToNextPoint()
        {
            // Returns if no points have been set up
            if (stateManager.enemy.patrolPoints.Length == 0)
                return;

            // Set the agent to go to the currently selected destination.
            stateManager.enemy.Agent.SetDestination(stateManager.enemy.patrolPoints[_destPoint].position);

            // Choose the next point in the array as the destination,
            // cycling to the start if necessary.
            _destPoint = (_destPoint + 1) % stateManager.enemy.patrolPoints.Length;
        }
        
        
    }

    public class ChaseState : State
    {
        public ChaseState(EnemyStateManager stateManager) : base(stateManager) {}

        public override void Enter()
        {
            stateManager.enemy.isLockedOn = true;
            stateManager.enemy.timePassed = stateManager.enemy.destCooldownTime;
        }

        public override void FixedUpdate()
        {
            stateManager.enemy.timePassed -= Time.deltaTime;
            
            if (stateManager.enemy.timePassed <= 0)
            {
                stateManager.enemy.Agent.SetDestination(PlayerManager.Instance.transform.position);
                stateManager.enemy.timePassed = stateManager.enemy.destCooldownTime;
            }
            
            // generate a ray from the enemy to the player 
            Ray ray = new Ray(stateManager.enemy.lookTransform.position, PlayerManager.Instance.lookTransform.position - stateManager.enemy.lookTransform.position);
            Debug.DrawRay(stateManager.enemy.lookTransform.position + Vector3.up, PlayerManager.Instance.lookTransform.position - stateManager.enemy.lookTransform.position, Color.yellow);

            // and perform a ray cast
            if (Physics.Raycast(ray, out var hit, stateManager.enemy.detectionDistance)) {
                // if the player is not within the detection radius, switch to the patrol state
                if (!hit.collider.CompareTag("PlayerHitbox")) stateManager.SwitchState(stateManager.patrolState);
            }

            if (stateManager.enemy.Agent.remainingDistance < stateManager.enemy.Agent.stoppingDistance)
            {
                stateManager.SwitchState(stateManager.attackState);
            }

            
        }

        public override void Exit()
        {
            stateManager.enemy.isLockedOn = false;
        }
    }

    public class AttackState : State
    {
        private float _attackCooldown;
        
        public AttackState(EnemyStateManager stateManager) : base(stateManager) {}

        public override void Enter()
        {
            stateManager.enemy.Animator.SetTrigger("Attack");
            stateManager.enemy.isLockedOn = true;
            stateManager.enemy.timePassed = stateManager.enemy.attackCooldownTime;
        }

        public override void FixedUpdate()
        {
            stateManager.enemy.timePassed -= Time.deltaTime;
            // if attack cooldown has passed
            if (stateManager.enemy.timePassed <= 0)
            {
                if (Vector3.Distance(PlayerManager.Instance.transform.position, stateManager.enemy.transform.position) <= stateManager.enemy.attackDistance)
                {
                    stateManager.SwitchState(stateManager.attackState);
                }
                else
                {
                    // generate a ray from the enemy to the player
                    Ray ray = new Ray(stateManager.enemy.lookTransform.position, PlayerManager.Instance.lookTransform.position - stateManager.enemy.lookTransform.position);
                    if (!Physics.Raycast(ray, out var hit, stateManager.enemy.detectionDistance)) stateManager.SwitchState(stateManager.idleState);
                    Debug.DrawRay(stateManager.enemy.lookTransform.position + Vector3.up, PlayerManager.Instance.lookTransform.position - stateManager.enemy.lookTransform.position, Color.red);

                    if (hit.collider.CompareTag("PlayerHitbox"))
                    {
                        stateManager.SwitchState(stateManager.chaseState);
                    }
                    else
                    {
                        stateManager.SwitchState(stateManager.patrolState);
                    }

                }
            }
        }


        public override void Exit()
        {
            stateManager.enemy.isLockedOn = false;
            stateManager.enemy.Animator.ResetTrigger("Attack");
        }
    }
    
    

}
