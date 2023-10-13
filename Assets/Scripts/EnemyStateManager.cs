using UnityEngine;

// -------------------------- ENEMY STATE MANAGER ----------------------------
public class EnemyStateManager
{
    protected static PlayerManager player;
    // public AttackState attackState;
    public ChaseState chaseState;
    public IdleState idleState;

    public PatrolState patrolState;

    public EnemyStateManager(EnemyManager enemy)
    {
        player = PlayerManager.Instance;
        patrolState = new PatrolState(this, enemy);
        idleState = new IdleState(this, enemy);
        chaseState = new ChaseState(this, enemy);
        // attackState = new AttackState(this, enemy);
        CurrentState = patrolState;
        CurrentState?.Enter();
    }

    public EnemyStateManager(State initialState)
    {
        CurrentState = initialState;
        CurrentState?.Enter();
    }

    public State CurrentState { get; private set; }

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

    public bool CheckState(State stateToCheck)
    {
        return CurrentState == stateToCheck;
    }

    // -------------------------------- INNER BASE STATE CLASS ---------------------------------- //

    public abstract class State
    {
        protected readonly EnemyManager enemy;
        protected readonly EnemyStateManager stateManager;
        protected float timePassed;

        protected State(EnemyStateManager stateManager, EnemyManager enemy)
        {
            this.stateManager = stateManager;
            this.enemy = enemy;
        }

        // Enter is called when the state is first entered
        public virtual void Enter()
        {
        }

        // Update is called once per frame
        public virtual void Update()
        {
        }

        // FixedUpdate is called once per physics update
        public virtual void FixedUpdate()
        {
        }

        // Exit is called when the state is exited
        public virtual void Exit()
        {
        }
    }

    // ------------------------------ ALL THE DIFFERENT STATES ----------------------------------- //

    public class IdleState : State
    {
        public IdleState(EnemyStateManager stateManager, EnemyManager enemy) : base(stateManager, enemy)
        {
        }

        public override void Enter()
        {
            timePassed = enemy.idleCooldownTime;
        }

        public override void FixedUpdate()
        {
            enemy.HandleRotation(false);
            enemy.HandleMovement();

            timePassed -= Time.deltaTime;

            if (timePassed <= 0) stateManager.SwitchState(stateManager.patrolState);
            // if the player is not within enemy's FOV, don't do anything
            if (!enemy.IsPlayerInView()) return;
            // if the player is not within detection range, don't do anything
            if (!enemy.RayCastToPlayer(enemy.detectionDistance)) return;
            // If the player is in the enemy's FOV, start chasing
            if (enemy.rayHit.collider.gameObject.layer == LayerMask.NameToLayer("Player")) stateManager.SwitchState(stateManager.chaseState);
        }
    }

    public class PatrolState : State
    {
        private int _destPoint;


        public PatrolState(EnemyStateManager stateManager, EnemyManager enemy) : base(stateManager, enemy)
        {
        }


        public override void Enter()
        {
            // enemy.Agent.stoppingDistance = enemy.stoppingDistance;
            GoToNextPoint();
        }

        public override void FixedUpdate()
        {
            enemy.HandleRotation(false);
            enemy.HandleMovement();

            if (enemy.Agent.remainingDistance < enemy.Agent.stoppingDistance)
                stateManager.SwitchState(stateManager.idleState);
            // if the player is not within enemy's FOV, don't do anything
            if (!enemy.IsPlayerInView()) return;
            // if the player is not within detection range, don't do anything
            if (!enemy.RayCastToPlayer(enemy.detectionDistance)) return;
            // If the player is in the enemy's FOV, start chasing
            if (enemy.rayHit.collider.gameObject.layer == LayerMask.NameToLayer("Player")) stateManager.SwitchState(stateManager.chaseState);
        }

        private void GoToNextPoint()
        {
            // Returns if no points have been set up
            if (enemy.patrolPoints.Length == 0)
                return;

            // Set the agent to go to the currently selected destination.
            enemy.Agent.SetDestination(enemy.patrolPoints[_destPoint].position);

            // Choose the next point in the array as the destination,
            // cycling to the start if necessary.
            _destPoint = (_destPoint + 1) % enemy.patrolPoints.Length;
        }
    }

    public class ChaseState : State
    {
        public ChaseState(EnemyStateManager stateManager, EnemyManager enemy) : base(stateManager, enemy)
        {
        }


        public override void Enter()
        {
            timePassed = enemy.attackCooldownTime;
        }

        public override void FixedUpdate()
        {
            timePassed -= Time.deltaTime;
            enemy.HandleRotation(true);
            enemy.HandleMovement();
            // set destination to player
            enemy.SetDestinationToPlayer();
            // perform a raycast towards the player 
            if (enemy.RayCastToPlayer(enemy.detectionDistance))
                // if the player is not within detection range, switch to the idle state
                if (enemy.rayHit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
                    stateManager.SwitchState(stateManager.idleState);
            // if the player is within attacking range
            if (Vector3.Distance(player.transform.position, enemy.transform.position) <= enemy.attackDistance &&
                timePassed <= 0)
            {
                enemy.Animator.SetTrigger("Attack");
                timePassed = enemy.attackCooldownTime;
            }
        }
    }

    // public class AttackState : State
    // {
    //     public AttackState(EnemyStateManager stateManager, EnemyManager enemy) : base(stateManager, enemy)
    //     {
    //     }
    //
    //
    //     public override void Enter()
    //     {
    //         enemy.Animator.SetTrigger("Attack");
    //     }
    //
    //     public override void FixedUpdate()
    //     {
    //         enemy.HandleRotation(false);
    //         enemy.HandleMovement();
    //         enemy.SetDestinationToPlayer();
    //
    //         timePassed -= Time.deltaTime;
    //         // if the player is within attack range 
    //         if (enemy.Agent.remainingDistance < enemy.Agent.stoppingDistance)
    //         {
    //             if (timePassed <= 0)
    //             {
    //                 timePassed = enemy.attackCooldownTime;
    //                 stateManager.SwitchState(stateManager.attackState);
    //             }
    //         }
    //         else
    //         {
    //             stateManager.SwitchState(stateManager.chaseState);
    //         }
    //     }
    //
    //
    //     public override void Exit()
    //     {
    //         enemy.Animator.ResetTrigger("Attack");
    //     }
    // }
}