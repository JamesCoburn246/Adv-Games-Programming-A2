using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public Vector2 LookInput { get; private set; }
    
    public Vector3 MoveInput { get; private set; }
    
    public bool SprintInput { get; private set; }

    public bool AttackInput { get; private set; }
    
    WaitForSeconds _attackCooldown;
    Coroutine _attackWaitCoroutine;
    const float AttackInputDuration = 0.03f;

    public bool TurnCamInput { get; set; }
    
    public bool PauseInput { get; set; }

    public VisibilityToggler MenuToggler { get; set; }

    private void Awake()
    {
        _attackCooldown = new WaitForSeconds(AttackInputDuration);
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

    private void Start()
    {
        Cursor.visible = false;
        MenuToggler = GetComponent<VisibilityToggler>();
        if (MenuToggler.ToggleObject == null)
        {
            GameObject mainMenu = GameObject.FindWithTag("InGameMenu");
            if (mainMenu != null) MenuToggler.ToggleObject = mainMenu;
            else Debug.Log("Error: Menu toggle is not set!");
        }
    }

    private void Update()
    {
        // get pause input
        if (Input.GetKeyDown(KeyCode.P))
        {
            PauseInput = !PauseInput;
            MenuToggler.ToggleVisibility();
            if (!PauseInput)
            {
                // unpause
                Time.timeScale = 1;
                Cursor.visible = false;
            }
            else
            {
                // pause
                Time.timeScale = 0;
                Cursor.visible = true;
            }
        }
        // if not paused
        if (!PauseInput)
        {
            // poll all inputs
            MoveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0,Input.GetAxisRaw("Vertical")).normalized;
            SprintInput = Input.GetKey(KeyCode.Space);
            if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0))
            {
                if (_attackWaitCoroutine != null) StopCoroutine(_attackWaitCoroutine);
                _attackWaitCoroutine = StartCoroutine(AttackWait());
            }
            // AttackInput = Input.GetMouseButton(0) || Input.GetKey(KeyCode.F);
            LookInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            TurnCamInput = Input.GetKeyDown(KeyCode.Tab);            
        }
        else
        {
            // disable all inputs
            MoveInput = Vector3.zero;
            SprintInput = false;
            AttackInput = false;
            LookInput = Vector2.zero;
            TurnCamInput = false;            
        }
    }
    
    IEnumerator AttackWait()
    {
        AttackInput = true;
        yield return _attackCooldown;
        AttackInput = false;
    }
}