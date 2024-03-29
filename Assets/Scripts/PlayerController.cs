using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FishNet.Object;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float walkingSpeed = 5;
    [SerializeField] private float runningSpeed = 10;
    [SerializeField] private float sensitivity;
    [SerializeField] private Transform cameraPosition;
    
    private Vector3 _moveDirection = Vector3.zero;
    private CharacterController _characterController;
    private Animator _animator;
    private Camera _playerCamera;

    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canModedCamera = true;

    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            _playerCamera = Camera.main;
            _playerCamera.transform.SetParent(transform);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            gameObject.transform.parent.GetComponentInChildren<CinemachineVirtualCamera>().enabled = false;
           gameObject.GetComponent<PlayerController>().enabled = false;
        }
    }

    private void Start()
    {
        _characterController = GetComponentInChildren<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
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

        Vector3 forward = cameraPosition.TransformDirection(Vector3.forward);
        Vector3 right = cameraPosition.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = _moveDirection.y;
        _moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && _characterController.isGrounded)
        {
            _moveDirection.y = 6f;
            _animator.Play("jump");
        }
        else
        {
            _moveDirection.y = movementDirectionY;
        }

        if (!_characterController.isGrounded)
        {
            _moveDirection.y -= 10f * Time.fixedDeltaTime;
            
        }
        
        _animator.SetBool("is_fall", !_characterController.isGrounded);
        if (curSpeedX != 0 || curSpeedY != 0)
        {
            var a = Quaternion.LookRotation(_moveDirection.normalized);
            a.x = a.z = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, a, Time.fixedDeltaTime * 20f);
        }
        _animator.SetBool("is_walk", curSpeedX != 0 || curSpeedY != 0);
        _characterController.Move(_moveDirection * Time.fixedDeltaTime);
        cameraPosition.position = transform.position;
    }
    

    private void CameraMove()
    {
        if (canModedCamera && _playerCamera != null)
        {
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
