using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStateManager
{
    protected static PlayerManager player;

    protected static InputManager inputs;

    public readonly DeathState deathState;

    public readonly MoveState moveState;
    public readonly FallState fallState;

    public readonly LandState landState;

    public readonly AttackState attackState;
    private State CurrentState { get; set; }

    public PlayerStateManager()
    {
        inputs = InputManager.Instance;
        player = PlayerManager.Instance;
        moveState = new MoveState(this);
        fallState = new FallState(this);
        landState = new LandState(this);
        attackState = new AttackState(this);
        deathState = new DeathState(this);
        CurrentState = moveState;
        CurrentState?.Enter();
    }
    
    public PlayerStateManager(State initialState)
    {
        inputs = InputManager.Instance;
        player = PlayerManager.Instance;
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
    
    public bool CheckState(State stateToCheck)
    {
        return CurrentState == stateToCheck;
    }
    
    // Base State
    
    public class State
    {
        protected PlayerStateManager stateManager;

        protected State(PlayerStateManager stateManager)
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
    
    public class MoveState : State
    {
        private bool _depleteStaminaLock;
        
        public override void Enter()
        {
            player.Animator.SetTrigger("Move");
            _depleteStaminaLock = false;
        }
        
        public override void FixedUpdate()
        {
            player.GroundedCheck();
            player.HandleRotation();
            player.HandleMovement();
            DepleteStamina();
            // handle switching
            if (!player.IsGrounded)
            {
                stateManager.SwitchState(stateManager.fallState);
            }

            if (player.IsAttacking)
            {
                stateManager.SwitchState(stateManager.attackState);
            }
        }

        private void DepleteStamina()
        {
            if (player.IsSprinting && !_depleteStaminaLock)
            {
                _depleteStaminaLock = true;
                player.StartCoroutine(DepleteStaminaTask());
            }
            else
            {
                player.StopCoroutine(DepleteStaminaTask());
            }
        }

        private IEnumerator<WaitForSeconds> DepleteStaminaTask()
        {
            // reduce stamina by 1 point
            player.Stats.DepleteStamina(1.0f);
            // then wait for 1/10 of a second
            yield return new WaitForSeconds(0.1f);
            // release the lock because the coroutine has ended
            _depleteStaminaLock = false;
        }

        public override void Exit()
        {
            player.ResetAnimatorStateTime();
            player.Animator.ResetTrigger("Move");
        }

        public MoveState(PlayerStateManager stateManager) : base(stateManager)
        {
        }
    }

    public class FallState : State
    {
        public override void Enter()
        {
            player.Animator.SetTrigger("Fall");
        }

        public override void FixedUpdate()
        {
            player.GroundedCheck();
            player.HandleRotation();
            player.HandleMovement();
            
            if (!player.IsGrounded)
            {
                player.RigidBody.AddForce(player.fallingSpeed * player.inAirTime * -Vector3.up);
                player.inAirTime += Time.deltaTime * 2;
            }
            else
            {
                stateManager.SwitchState(stateManager.landState);
            }
        }

        public override void Exit()
        {
            player.Animator.ResetTrigger("Fall");
            player.ResetAnimatorStateTime();
        }

        public FallState(PlayerStateManager stateManager) : base(stateManager)
        {
        }
    }

    public class LandState : State
    {
        private float _inLandTime;
        public LandState(PlayerStateManager stateManager) : base(stateManager) {}
        
        public override void Enter()
        {
            // _inLandTime = 0;
            player.Animator.SetTrigger("Land");
        }

        public override void FixedUpdate() 
        {
            if (player.IsAnimatorTransitioning) return;
            // _inLandTime += Time.deltaTime;
            if (player.AnimatorStateTime >= 0.5f)
            {
                stateManager.SwitchState(stateManager.moveState);
            }
        }

        public override void Exit()
        {
            player.Animator.ResetTrigger("Land");
            player.ResetAnimatorStateTime();
            
        }
    }

    public class AttackState : State
    {
        private bool triggerCombo;

        public AttackState(PlayerStateManager stateManager) : base(stateManager) { }

        public override void Enter()
        {
            player.Animator.applyRootMotion = true;
            triggerCombo = false;
            player.RigidBody.velocity = Vector3.zero;
            player.Animator.SetTrigger("Attack");
        }

        public override void FixedUpdate()
        {
            player.GroundedCheck();
            if (player.IsAnimatorTransitioning) return;
            switch (player.AnimatorStateTime)
            {
                case > 0.15f when player.Inputs.AttackInput:
                    triggerCombo = true;
                    break;
                case >= 0.8f when triggerCombo:
                    stateManager.SwitchState(stateManager.attackState);
                    break;
                case >= 1f when !triggerCombo:
                    stateManager.SwitchState(stateManager.moveState);
                    break;
            }
        }

        public override void Exit()
        {
            player.Animator.applyRootMotion = false;
            player.Animator.ResetTrigger("Attack");
            player.ResetAnimatorStateTime();
        }
    }


    public class DeathState : State
    {
        public DeathState(PlayerStateManager stateManager) : base(stateManager)
        {
        }


        public override void Enter()
        {
            player.ResetMovement();
            player.Animator.SetTrigger("Death");
        }

        public override void Update()
        {
            if (player.IsAnimatorTransitioning) return;
            if (player.AnimatorStateTime >= 1f)
            {
                InputManager.Instance.MenuToggler.ToggleObject.SetActive(true);
                Cursor.visible = true;
            }
        }

        public override void Exit()
        {
        }
    }



}
