using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float moveAcceleration;
    public float groundDrag;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // �Estoy en el suelo?
        grounded = Physics.Raycast(
            transform.position, 
            Vector3.down, 
            playerHeight * 0.5f + 0.2f, 
            whatIsGround
        );
        

        MyInput();
        SpeedControl();

        // Lidia con la fricci�n del suelo
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
                
    }

    private void FixedUpdate()
    {
        // el personaje se mueve aqu� porque est� basado en f�sicas y FixedUpdate es la funci�n que se ejecuta para las f�sicas
        MovePlayer();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 direction = transform.TransformDirection(Vector3.down) * (playerHeight * 0.5f + 0.2f);
        Gizmos.DrawRay(transform.position, direction);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }
    
    private void MovePlayer()
    {
        // calcula la direcci�n en la que se mueve el personaje
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(moveDirection.normalized * moveSpeed * moveAcceleration, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Evito sobrepasar la velocidad m�xima
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}
