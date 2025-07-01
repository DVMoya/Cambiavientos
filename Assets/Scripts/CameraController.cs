using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public Rigidbody rb;

    public float rotationSpeed;


    [SerializeField] private CinemachineFreeLook freeLook;
    [SerializeField] private RadialMenu cambiavientosMenu;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Detectar si se abre el menú radial del Cambiavientos
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            freeLook.enabled = true;
            cambiavientosMenu.ShowSelectedWeather();
            cambiavientosMenu.Toggle();
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            freeLook.enabled = false;
            cambiavientosMenu.Toggle();
            return;
        } else if (Input.GetKey(KeyCode.Mouse0))
        {
            return; // esto evita que magus pueda girar mientras este habierto el menu radial
        }

        // Obtener la dirección en la que apunto
        Vector3 viewDirection = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDirection.normalized;

        // Calcular la orientación
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput   = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
    
        //
        if(inputDirection != Vector3.zero)
        {
            playerObject.forward = Vector3.Slerp(playerObject.forward, inputDirection.normalized, Time.deltaTime * rotationSpeed);
        }
    }
}
