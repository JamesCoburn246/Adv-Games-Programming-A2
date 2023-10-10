using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    // singleton instance
    public static CameraController Instance { get; private set; }

    private Transform _cameraPivot;
    private Camera _mainCam;

    public bool isoView;
    
    [Range(0.01f, 0.5f)]
    public float camMoveSpeed = 0.5f;
    [Range(5f, 15f)]
    public float camTurnSpeed = 15f;
    [Range(-10f, 0f)]
    public float camDistance = -5f;

    public Transform followTarget;

    private float _targetRotX, _targetRotY;

    private Quaternion _targetRot;

    private Vector3 _camVelocity;
    
    // iso view stuff
    
    private float _camAngularVelocity;

    private readonly float[] _camAngles = {45f,135f,225f,315f};
    
    private int _currCamIndex;

    [Range(0f, 10f)]
    public float camIsoDistance = 5f;

    public float camIsoSmoothTime = 0.3f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _cameraPivot = transform.GetChild(0);
        _mainCam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isoView)
        {
            if (_mainCam.orthographic)
            {
                // set orthographic to false
                _mainCam.orthographic = false;
                // adjust the near and far clip planes
                _mainCam.nearClipPlane = 0.01f;
                _mainCam.farClipPlane = 1000f;
                // set the right z-distance
                _mainCam.transform.localPosition = new Vector3(0, 0, camDistance);
            }
            // rotation around the x-axis (i.e up-down)
            _targetRotX = Mathf.Clamp(_targetRotX-InputManager.Instance.LookInput.y * Time.deltaTime * camTurnSpeed, -10, 60);
            // rotation around the y-axis (i.e. left-right)
            _targetRotY += InputManager.Instance.LookInput.x * Time.deltaTime * camTurnSpeed;
            // combined rotation (order ZYX)
            _targetRot = Quaternion.AngleAxis(_targetRotY, Vector3.up) * Quaternion.AngleAxis(_targetRotX, Vector3.right);
        }
        else
        {
            if (!_mainCam.orthographic)
            {
                // set orthographic to true
                _mainCam.orthographic = true;
                // adjust the near and far clip planes
                _mainCam.nearClipPlane = -100f;
                _mainCam.farClipPlane = 100f;
                // set the right z-distance
                _mainCam.orthographicSize = camIsoDistance;
            }
            // turn the cam if tab is pressed
            if (InputManager.Instance.TurnCamInput)
            {
                InputManager.Instance.TurnCamInput = false;
                _currCamIndex++;
                if (_currCamIndex > 3) _currCamIndex = 0;
            }
        }
    }

    private void LateUpdate()
    {
        if (!isoView)
        {
            // rotate the camera to the target rotation if using 3rd person view
            _cameraPivot.rotation = _targetRot;
            // rotate the camera manager object only around the y-axis
            transform.rotation = Quaternion.AngleAxis(_cameraPivot.transform.eulerAngles.y, Vector3.up);
            // move camera manager object to follow target position
            transform.position = Vector3.SmoothDamp(transform.position, followTarget.position, ref _camVelocity, camMoveSpeed);
        }
        else
        {
            var smoothAngle = Mathf.SmoothDampAngle(_cameraPivot.rotation.eulerAngles.y, _camAngles[_currCamIndex],
                ref _camAngularVelocity, camIsoSmoothTime);
            // rotate the camera pivot
            _cameraPivot.rotation = Quaternion.Euler(35.264f, smoothAngle, 0);
            // rotate the camera manager object only around the y-axis
            transform.rotation = Quaternion.AngleAxis(smoothAngle, Vector3.up);
            // move camera manager object to follow target position
            transform.position = Vector3.SmoothDamp(transform.position, followTarget.position, ref _camVelocity, camMoveSpeed);
        }

    }
}
