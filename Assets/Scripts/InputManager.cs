using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public Vector2 LookInput { get; private set; }

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