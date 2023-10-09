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
    [SerializeField] public float baseMovementSpeed;
    [SerializeField] public float attackingMovementSpeedMultiplier = 0.825f;
    public float movementMultiplier = 1.0f;

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
        Vector3 force = directionOfMotion.normalized * (baseMovementSpeed * movementMultiplier); ;

        // In an attack animation.
        if (elapsedCooldown != 0)
        {
            force *= attackingMovementSpeedMultiplier;
        }

        this.rigidBody.AddForce(force, ForceMode.Force);
    }

    private void DoAttack()
    {

    }
}
