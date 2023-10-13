using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager
{
    protected static PlayerManager player;

    protected static InputManager inputs;

    public MoveState moveState { get; private set; }
    public FallState fallState { get; private set; }

    public LandState landState { get; private set; }
    
    public AttackState attackState { get; private set; }


    protected State CurrentState;
    
    public PlayerStateManager()
    {
        inputs = InputManager.Instance;
        player = PlayerManager.Instance;
        moveState = new MoveState(this);
        fallState = new FallState(this);
        landState = new LandState(this);
        attackState = new AttackState(this);
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
        public override void Enter()
        {
            player.Animator.SetTrigger("Move");
        }
        
        public override void FixedUpdate()
        {
            player.GroundedCheck();
            player.HandleRotation();
            player.HandleMovement();
            // handle switching
            if (!player.isGrounded)
            {
                stateManager.SwitchState(stateManager.fallState);
            }

            if (inputs.AttackInput)
            {
                stateManager.SwitchState(stateManager.attackState);
            }
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
            
            if (!player.isGrounded)
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
            player.Animator.SetTrigger("Attack");
        }

        public override void FixedUpdate()
        {
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






}
