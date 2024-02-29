using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkingSpeed = 5;
    [SerializeField] private float runningSpeed = 10;
    [SerializeField] private float sensitivity;
    [SerializeField] private Transform cameraPosition;

    private float _xRotation = 0f;
    private Vector3 _moveDirection = Vector3.zero;
    private CharacterController _characterController;
    private Camera _playerCamera;

    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canModedCamera = true;

    private void Start()
    {
        _playerCamera = Camera.main;
        // _playerCamera.transform.position = cameraPosition.position;
        _playerCamera.transform.SetParent(transform);
        _characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked? CursorLockMode.Confined: CursorLockMode.Locked;
            canModedCamera = CursorLockMode.Locked == Cursor.lockState;
        }
    }

    private void FixedUpdate()
    {
        CharacterMove();
        CameraMove();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void CharacterMove()
    {
        bool isRunning;

        isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = _moveDirection.y;
        _moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && _characterController.isGrounded)
        {
            _moveDirection.y = 6f;
        }
        else
        {
            _moveDirection.y = movementDirectionY;
        }

        if (!_characterController.isGrounded)
        {
            _moveDirection.y -= 10f * Time.fixedDeltaTime;
        }

        if (curSpeedX != 0 || curSpeedY != 0)
        {
            var a = cameraPosition.rotation;
            a.x = a.z = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, a, Time.deltaTime * 20f);
            // transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(0f, cameraPosition.rotation.y, 0), Time.deltaTime * 10);
            // transform.rotation = Quaternion.Euler(0, cameraPosition.rotation.eulerAngles.y, 0);
        }
        
        _characterController.Move(_moveDirection * Time.deltaTime);
        cameraPosition.position = transform.position;
    }

    private void CameraMove()
    {
        if (canModedCamera && _playerCamera != null)
        {
            // float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime;
            // float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime;
            // _xRotation -= mouseY;
            // _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
            //
            // transform.Translate(0, 0, mouseX);
            // // _playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            // //transform.Rotate(Vector3.up*mouseX);
            // transform.rotation *= Quaternion.Euler(0, mouseX, 0);
            
            float aimX = Input.GetAxis("Mouse X");
            float aimY = Input.GetAxis("Mouse Y");
            cameraPosition.rotation *= Quaternion.AngleAxis(aimX * sensitivity,Vector3.up);
            cameraPosition.rotation *= Quaternion.AngleAxis(-aimY * sensitivity, Vector3.right);
            
            var angleX = cameraPosition.localEulerAngles.x;
            if(angleX > 180 && angleX < 310)
            {
                angleX = 310;
            }
            else if (angleX < 180 && angleX > 70)
            {
                angleX = 70;
            }
 
            cameraPosition.localEulerAngles = new Vector3(angleX, cameraPosition.localEulerAngles.y, 0);
        }
    }
}
