using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotionController : MonoBehaviour
{
    [Header("Unity Objects")]
    [SerializeField] public Transform playerTransform;
    [SerializeField] public Rigidbody rigidBody;

    [Header("Controls")]
    [SerializeField] public KeyCode jumpCode = KeyCode.Space;

    [Header("Movement")]
    [SerializeField] public float movementSpeed = 10.0f;
    [SerializeField] public float attackingMovementSpeedMultiplier = 0.825f;

    [Header("Attacking")]
    [SerializeField] public bool southpaw = false;
    [SerializeField] public float attackCooldown;
    /*
     * When an attack is made, elapsed cooldown is set to attack cooldown.
     * As time passed, elapsed cooldown is decreased until it becomes 0.
     * The player can only attack when elapsedCooldown is 0.
     */
    private float elapsedCooldown;

    // Animation Variables
    private bool _isGrounded;

    // Start is called before the first frame update
    private void Start()
    {
       this.playerTransform = GetComponentInParent<Transform>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        float vAxis = Input.GetAxis("Vertical");
        float hAxis = Input.GetAxis("Horizontal");
        bool attack = Input.GetMouseButton(southpaw ? 2 : 1);
        if (attack)
        {
            DoAttack();
        }
        DoMotion(vAxis, hAxis);
    }

    private void DoMotion(float vAxis, float hAxis)
    {
        Vector3 directionOfMotion = (playerTransform.forward * vAxis) + (playerTransform.right * hAxis);
        // Not in an attack animation.
        if (elapsedCooldown == 0)
        {
            this.rigidBody.AddForce(directionOfMotion.normalized * (movementSpeed), ForceMode.Force);
        }
        // In an attack animation.
        else
        {
            this.rigidBody.AddForce(directionOfMotion.normalized * (movementSpeed * attackingMovementSpeedMultiplier), ForceMode.Force);
        }
    }

    private void DoAttack()
    {

    }
}
