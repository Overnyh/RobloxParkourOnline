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

    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canModedCamera = true;
    private float _xRotation = 0f;
    private float _yRotation = 0f;
    private Vector3 _spawnPoint;


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
            enabled = false;
        }
    }

    private void Start()
    {
        _characterController = transform.parent.GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _spawnPoint = transform.parent.transform.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked? CursorLockMode.Confined: CursorLockMode.Locked;
            canModedCamera = CursorLockMode.Locked == Cursor.lockState;
        }
        _yRotation += Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime;
        _xRotation -= Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime;
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
	    CameraMove();
        
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void CharacterMove()
    {
        Vector3 forward = cameraPosition.TransformDirection(Vector3.forward);
        Vector3 right = cameraPosition.TransformDirection(Vector3.right);
        forward.y = 0;
        forward.Normalize();
        right.y = 0;
        right.Normalize();

        float curSpeedX = canMove ? walkingSpeed * Input.GetAxisRaw("Vertical") : 0;
        float curSpeedY = canMove ? walkingSpeed * Input.GetAxisRaw("Horizontal") : 0;
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
            transform.rotation = Quaternion.Slerp(transform.rotation, a, 0.4f);
        }
        _animator.SetBool("is_walk", curSpeedX != 0 || curSpeedY != 0);
        _characterController.Move(_moveDirection * Time.fixedDeltaTime);
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
        if (Vector3.Dot(_moveDirection, forward) > 0 || Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
        {
            _fromLadder = false;
            _moveDirection = Vector3.zero;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
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
            transform.parent.parent = other.transform;
            _isOnPlatform = true;
        }

        if (other.CompareTag("Dead"))
        {
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator Respawn()
    {
        _animator.Play("die");
        canMove = false;
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        _animator.Rebind();
        _characterController.transform.position = _spawnPoint;
        canMove = true;
        
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
            transform.parent.parent = null;
            _isOnPlatform = false;
        }
    }


    private void CameraMove()
    {
        if (canModedCamera && _playerCamera != null)
        {
            cameraPosition.transform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
            
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
