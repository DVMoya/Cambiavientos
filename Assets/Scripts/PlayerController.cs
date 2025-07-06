using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float moveAcceleration;
    public float airFriction;
    public float groundDrag;
    public float maxSlopeAngle;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Stair Check")]
    public Transform stairCheck;
    public LayerMask whatIsStair;
    public float stairJumpStrength;
    bool staired = false;

    [Header("Climbing")]
    public LayerMask whatIsLedge;
    public float climbDuration;
    bool isClimbing = false;

    public Transform orientation;

    // --- INPUT HANDLING ---
    float horizontalInput;
    float verticalInput;

    // --- MOVEMENT HANDLING ---
    Vector3 moveDirection;
    Collider ledgeCollider = null;

    // --- RAYCAST HANDLING ---
    Rigidbody rb;
    private RaycastHit slopeHit;
    private RaycastHit stairHit;
    

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // si estoy subiendo a un saliente me espero a terminar de subir
        if (isClimbing) return;

        // ¿Estoy en el suelo?
        grounded = Physics.Raycast(
            transform.position, 
            Vector3.down, 
            playerHeight * 0.5f + 0.2f, 
            whatIsGround
        );
        

        MyInput();
        Climb();
        SpeedControl();

        // Lidia con la fricción del suelo
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        // el personaje se mueve aquí porque está basado en físicas y FixedUpdate es la función que se ejecuta para las físicas
        MovePlayer();
    }

    private void OnDrawGizmos()
    {
        // ground check raycast
        Gizmos.color = Color.red;
        Vector3 direction = transform.TransformDirection(Vector3.down) * (playerHeight * 0.5f + 0.2f);
        Gizmos.DrawRay(transform.position, direction);

        // stair check raycast
        Gizmos.color = Color.blue;
        direction = stairCheck.TransformDirection(Vector3.down) * (playerHeight);
        Gizmos.DrawRay(stairCheck.position + moveDirection.normalized * 0.2f, direction);
        Gizmos.color = Color.yellow;
        direction = stairCheck.TransformDirection(Vector3.down) * (playerHeight * 0.5f - 0.3f);
        Gizmos.DrawRay(stairCheck.position + moveDirection.normalized * 0.2f, direction);

        // ledge check
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, moveDirection.normalized * 1f);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }
    
    private void MovePlayer()
    {
        if (Input.GetKey(KeyCode.Mouse0) && RadialMenu.canOpen)   // no quiero que magus se pueda mover mientras esté abierto el menú del cambiavientos
        {
            rb.velocity = Vector3.zero;
            return;
        }

        // calcula la dirección en la que se mueve el personaje
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // en cuesta
        if (OnSlope())
        {
            rb.AddForce(GetSlopeMovedirection() * moveSpeed * moveAcceleration, ForceMode.Force);
            
            if(rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 10f, ForceMode.Force);   // para compensar la falta de gravedad
        }
        // en no cuesta
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * moveAcceleration, ForceMode.Force);
        }
        // en el aire
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * moveAcceleration * (1f/airFriction), ForceMode.Force);
        }

        // en caso de estar subiendo escaleras empujo al jugador hacia arriba
        if (OnStair())
        {
            if(stairHit.distance > playerHeight * 0.5f - 0.3f &&
               stairHit.distance < playerHeight * 0.5f) {
                staired = true;
                rb.AddForce(Vector3.up * stairJumpStrength, ForceMode.Force);
            } else {
                staired = false;
                if(stairHit.distance > playerHeight * 0.5f + 0.05f)
                    rb.AddForce(Vector3.down * 20f, ForceMode.Force);    // hace que se pedue a las escaleras cuando las baja
            }
        }

        // activar/desactivar la gravedad dependiendo del terreno
        rb.useGravity = !OnSlope() && !staired;
    }

    private void SpeedControl()
    {
        // limita la velocidad sobre una pendiente
        if (OnSlope())
        {
            if(rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        // limita la velocidad cuando no esta en una pendiente
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // Evito sobrepasar la velocidad máxima
            if(flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }

            if (rb.velocity.y > moveSpeed)
                rb.velocity = new Vector3(rb.velocity.x, moveSpeed, rb.velocity.x);
        }
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMovedirection()
    {
        // Proyectar la dirección de movimiento sobre el plano inclinado por el que se mueve el personaje
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private bool OnStair()
    {
        return Physics.Raycast(
            stairCheck.position + moveDirection.normalized * 0.2f,
            Vector3.down,
            out stairHit,
            playerHeight,
            whatIsStair);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Crate"))
        {
            ledgeCollider = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other == ledgeCollider)
        {
            ledgeCollider = null;
        }
    }

    private void Climb()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(Physics.Raycast(
                transform.position, 
                moveDirection.normalized, 
                out var firstHit, 
                1f, 
                whatIsLedge))
            {
                // encontrado saliente al que subirse
                Debug.Log("encontrado saliente al que subirse");
                if(Physics.Raycast(
                    firstHit.point + (Vector3.forward * 0.3f) + (Vector3.up * 1f * playerHeight), // el 1f es para modificar el tamaño final del rayo en caso de querer modificarlo
                    Vector3.down, 
                    out var secondHit, 
                    playerHeight))
                {
                    // encontrado punto al que subirse
                    Debug.Log("encontrado punto al que subirse");
                    Vector3 targetPosition = secondHit.point + moveDirection.normalized * 0.75f + Vector3.up * (playerHeight * 0.5f + 0.1f);
                    StartCoroutine(LerpClimb(targetPosition, climbDuration));
                }
            }
        }
    }

    IEnumerator LerpClimb(Vector3 targetPosition, float duration)
    {
        float time = 0f;
        Vector3 startPosition = transform.position;

        while (time < duration) 
        { 
            transform.position = Vector3.Lerp(startPosition, targetPosition, time/duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        Debug.Log("terminado de subir al saliente");
    }
}
