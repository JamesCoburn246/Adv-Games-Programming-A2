using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager
{
    private static PlayerManager _player;

    private static InputManager _inputs;

    public MoveState moveState { get; private set; }
    public FallState fallState { get; private set; }

    public LandState landState { get; private set; }
    
    public AttackState attackState { get; private set; }


    protected State CurrentState;
    
    public PlayerStateManager()
    {
        _inputs = InputManager.Instance;
        _player = PlayerManager.Instance;
        moveState = new MoveState(this);
        fallState = new FallState(this);
        landState = new LandState(this);
        attackState = new AttackState(this);
        CurrentState = moveState;
        CurrentState?.Enter();
    }
    
    public PlayerStateManager(State initialState)
    {
        _inputs = InputManager.Instance;
        _player = PlayerManager.Instance;
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
            _player.Animator.SetTrigger("Move");
        }
        
        public override void FixedUpdate()
        {
            _player.GroundedCheck();
            _player.HandleRotation();
            _player.HandleMovement();
            // handle switching
            if (!_player.isGrounded)
            {
                stateManager.SwitchState(stateManager.fallState);
            }

            if (_inputs.AttackInput)
            {
                stateManager.SwitchState(stateManager.attackState);
            }
        }

        public override void Exit()
        {
            _player.ResetAnimatorStateTime();
            _player.Animator.ResetTrigger("Move");
        }

        public MoveState(PlayerStateManager stateManager) : base(stateManager)
        {
        }
    }

    public class FallState : State
    {
        public override void Enter()
        {
            _player.Animator.SetTrigger("Fall");
        }

        public override void FixedUpdate()
        {
            _player.GroundedCheck();
            _player.HandleRotation();
            _player.HandleMovement();
            
            if (!_player.isGrounded)
            {
                _player.RigidBody.AddForce(_player.fallingSpeed * _player.inAirTime * -Vector3.up);
                _player.inAirTime += Time.deltaTime * 2;
            }
            else
            {
                stateManager.SwitchState(stateManager.landState);
            }
        }

        public override void Exit()
        {
            _player.Animator.ResetTrigger("Fall");
            _player.ResetAnimatorStateTime();
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
            _player.Animator.SetTrigger("Land");
        }

        public override void FixedUpdate() 
        {
            if (_player.IsAnimatorTransitioning) return;
            // _inLandTime += Time.deltaTime;
            if (_player.AnimatorStateTime >= 0.5f)
            {
                stateManager.SwitchState(stateManager.moveState);
            }
        }

        public override void Exit()
        {
            _player.Animator.ResetTrigger("Land");
            _player.ResetAnimatorStateTime();
            
        }
    }

    public class AttackState : State
    {
        private bool triggerCombo;

        public AttackState(PlayerStateManager stateManager) : base(stateManager) { }

        public override void Enter()
        {
            _player.Animator.applyRootMotion = true;
            triggerCombo = false;
            _player.Animator.SetTrigger("Attack");
        }

        public override void FixedUpdate()
        {
            if (_player.IsAnimatorTransitioning) return;
            switch (_player.AnimatorStateTime)
            {
                case > 0.15f when _player.Inputs.AttackInput:
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
            _player.Animator.applyRootMotion = false;
            _player.Animator.ResetTrigger("Attack");
            _player.ResetAnimatorStateTime();
        }
    }






}
