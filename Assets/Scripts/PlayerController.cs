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
    [SerializeField] private float jumpForse = 6f;
    [SerializeField] private float sensitivity;
    [SerializeField] private Transform cameraPosition;
    
    private Vector3 _moveDirection = Vector3.zero;
    private CharacterController _characterController;
    private Animator _animator;
    private Camera _playerCamera;
    private bool _isClimbing;
    private bool _fromLadder;
    private bool _isOnPlatform;
    private Transform _tempPos;

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
	CameraMove();
    }

    private void FixedUpdate()
    {
        if (_isClimbing)
        {
            LadderMove();
        }
        else if (_fromLadder)
        {
            LadderForce();
        }
        else
        {
            CharacterMove();
        }

	
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void CharacterMove()
    {
        bool isRunning;

        isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 forward = cameraPosition.TransformDirection(Vector3.forward);
        Vector3 right = cameraPosition.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxisRaw("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxisRaw("Horizontal") : 0;
        float movementDirectionY = _moveDirection.y;
        _moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && _characterController.isGrounded)
        {
            _moveDirection.y = jumpForse;
            _animator.Play("jump");
        }
        else
        {
            _moveDirection.y = movementDirectionY;
        }
        
        if (!_characterController.isGrounded && !_isOnPlatform)
        {
            _moveDirection.y -= 10f * 2 * Time.fixedDeltaTime;
        
        }
        
        _animator.SetBool("is_fall", !_characterController.isGrounded);
        if (curSpeedX != 0 || curSpeedY != 0)
        {
            var a = Quaternion.LookRotation(_moveDirection.normalized);
            a.x = a.z = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, a, Time.fixedDeltaTime * 20f);
        }
        _animator.SetBool("is_walk", curSpeedX != 0 || curSpeedY != 0);
        Vector3 direction = Vector3.zero;
        if (_tempPos != null)
        {
            // direction = ;
            // transform.position = Vector3.Lerp(transform.position, _tempPos.position, 0.1f);
            _characterController.Move(_tempPos.position - transform.position);
            direction.y = 0;
        }
        _characterController.Move(direction +(_moveDirection * Time.fixedDeltaTime));
        if (_tempPos != null) _tempPos.position = transform.position;
        cameraPosition.position = transform.position;
    }
    
    

    private void LadderMove()
    {
        Vector3 forward = transform.TransformDirection(Vector3.up);
        Vector3 backward = transform.TransformDirection(Vector3.back + Vector3.up);
        
        float curSpeedX = canMove ?  walkingSpeed * Input.GetAxisRaw("Vertical") : 0;
        _moveDirection = (forward * curSpeedX);

        if (Input.GetButton("Jump") || _characterController.isGrounded)
        {
            _moveDirection = backward*jumpForse;
        }
        if (curSpeedX != 0)
        {
            _animator.speed = 1;
        }
        else
        {
            _animator.speed = 0;
        }
        _characterController.Move(_moveDirection * Time.fixedDeltaTime);
        cameraPosition.position = transform.position;
    }

    private void LadderForce()
    {
        print("w: "+Input.GetAxisRaw("Vertical"));
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        if (!_characterController.isGrounded)
        {
            _moveDirection.y -= 10f * 2 * Time.fixedDeltaTime;
            _moveDirection += forward * 10f * 2 * Time.fixedDeltaTime;
        }
        _characterController.Move(_moveDirection * Time.fixedDeltaTime);
        cameraPosition.position = transform.position;
        if (Vector3.Dot(_moveDirection, forward) > 0 || Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
        {
            _fromLadder = false;
            _moveDirection = Vector3.zero;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        print(other);
        if (other.CompareTag("Ladder"))
        {
            _animator.SetBool("is_walk", false);
            _animator.SetBool("is_fall", false);
            _animator.SetBool("is_on_ladder", true);
            _animator.Play("ladder");
            _characterController.Move(Vector3.up*0.1f);
            _isClimbing = true;
        }
        
        if (other.CompareTag("Platform"))
        {
            GameObject emptyObject = new GameObject("EmptyObject");
            _tempPos = emptyObject.transform;
            _tempPos.position = transform.position;
            _tempPos.parent = other.transform;
            _isOnPlatform = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            _animator.speed = 1;
            _animator.SetBool("is_on_ladder", false);
            _isClimbing = false;
            _fromLadder = true;
        }
        
        if (other.CompareTag("Platform"))
        {
            Destroy(_tempPos.gameObject);
            _tempPos = null;
            _isOnPlatform = false;
        }
    }


    private void CameraMove()
    {
        if (canModedCamera && _playerCamera != null)
        {
            float aimX = Input.GetAxis("Mouse X");
            float aimY = Input.GetAxis("Mouse Y");
            cameraPosition.rotation *= Quaternion.AngleAxis(aimX * sensitivity * Time.deltaTime,Vector3.up);
            cameraPosition.rotation *= Quaternion.AngleAxis(-aimY * sensitivity * Time.deltaTime, Vector3.right);
            
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
