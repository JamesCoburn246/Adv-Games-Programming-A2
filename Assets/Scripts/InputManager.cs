using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public Vector2 LookInput { get; private set; }
    
    public Vector3 MoveInput { get; private set; }
    
    public bool SprintInput { get; private set; }

    public bool AttackInput { get; set; }

    public bool TurnCamInput { get; set; }

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

    private void Update()
    {
        // get move input
        MoveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0,Input.GetAxisRaw("Vertical")).normalized;
        // get sprint input
        SprintInput = Input.GetKey(KeyCode.Space);
        // get attack input
        AttackInput = Input.GetMouseButton(0);
    }

    public void OnMove(InputValue value)
    {
        
    }
    
    public void OnLook(InputValue value)
    {
        LookInput = value.Get<Vector2>();
    }

    public void OnTurnCam(InputValue value)
    {
        TurnCamInput = value.isPressed;
        Debug.Log(value.Get());
        Debug.Log(value.isPressed);
    }
}