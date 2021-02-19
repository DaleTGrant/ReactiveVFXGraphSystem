using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class Movement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed =2f;
    [SerializeField] private float jumpForce = 1;
    [SerializeField] private float turnTime = 0.5f;
    private PlayerInput playerInput;
    private InputActionMap playerActionMap;
    private Rigidbody _rigidbody;

    private InputAction move;
    private InputAction jump;
    private InputAction fire;

    [SerializeField] private bool canFire = true;
    [SerializeField] private float fireDelayTimer = 0f;
    
    [SerializeField] private float fireDelay = 0.5f;
    
    public VFXAnimationTransition _vfxAnimationTransition;
    public VisualEffect visualEffect;

    [SerializeField] private bool isGrounded = false;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private float raycastDistance;
    private Vector3 raycastDirection;
    [SerializeField] private LayerMask raycastMask;
    
    [SerializeField] private ParticleSystem shootSystem;
    [SerializeField] private Transform shotOrigin;
    [SerializeField] private GameObject shotEmitter;
    private GameObject tempShot;
    [SerializeField] private Animator animator;
    [SerializeField] private bool isJump = false;
    private bool isLanding = false;

    #region Initialization

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        isGrounded = false;
        SetupPlayerInput();
        IntializeVFXAnimation();
    }

    private void IntializeVFXAnimation()
    {
        if(!_vfxAnimationTransition) _vfxAnimationTransition = GetComponent<VFXAnimationTransition>();
        if(_vfxAnimationTransition) _vfxAnimationTransition.ChangeAnimation(VFXAnimationTransition.AnimationName.Idle,0f,false);
    }
    
    // Initialize the player input system and the input actions to be used during update and for methods to listen to.
    private void SetupPlayerInput()
    {
        // Cache player input and action map
        playerInput = GetComponent<PlayerInput>();
        if (playerInput) playerActionMap = playerInput.currentActionMap;
        
        // Cache input actions from action map
        if (playerActionMap!=null)
        {
            move = playerActionMap["Move"];
            jump = playerActionMap["Jump"];
            fire = playerActionMap["Fire"];
            
        }
        
        // Assign delegates to the started event for the input actions
        if (jump != null) jump.started += Jump;
        if (fire != null) fire.started += Fire;
    }

    #endregion

    #region Each Frame
    
    void Update()
    {
        if (move != null)
        {
            if (!isLanding)
            {
                Move(move.ReadValue<Vector2>().normalized);
            }
            
        }
        CheckForIsGrounded();
        FireDelay();
        DestroyIfStopped();
        SetAnimatorGrounding();
        CheckIsJump();

        if (animator.gameObject != gameObject)
        {
            animator.gameObject.transform.SetPositionAndRotation(transform.position,transform.rotation);
        }
    }
    
    // Move character laterally based on input Vector2 via translation.
    private void Move(Vector2 direction)
    {
        RaycastHit hit;
        float dir = direction.y;
        if(_rigidbody.SweepTest(transform.forward, out hit, 1f))
        {
            Mathf.Clamp(dir, -1, 0);
        }

        Vector3 movement = speed * dir * transform.forward;
        
        
        Vector3 turnVector = rotationSpeed * direction.x*transform.right;
        movement += turnVector;

        Vector3 position = _rigidbody.position + movement*Time.deltaTime;
        
        _rigidbody.MovePosition(position);
        

        // if new direction, rotate to face that direction over time
        if (IsMovingInNewDirection(movement))
        {
            float time = turnTime*(1-Vector3.Dot(transform.forward, movement.normalized));
            transform.DOLookAt(transform.position + movement, time).SetEase(Ease.Linear);
        }
        
        // Update animator float to transition to walking state
        if (animator) animator.SetFloat("Speed",movement.sqrMagnitude);
        
        // Pass Transform TRS Matrix of movement GO to VFX system for Target Position.
        if (visualEffect)
        {
            if (visualEffect.HasMatrix4x4("Movement TRS"))
            {
                visualEffect.SetMatrix4x4("Movement TRS",Matrix4x4.TRS(transform.localPosition,transform.localRotation,transform.localScale));
            }
            
        }
    }
    
    // Methods to Transition to the appropriate VFX animation
    public void VFXRun(float lerpTime)
    {
        if (_vfxAnimationTransition.currentAnimation != VFXAnimationTransition.AnimationName.Run)
        {
            _vfxAnimationTransition.ChangeAnimation(VFXAnimationTransition.AnimationName.Run, lerpTime, false);
        }
        
    }

    public void VFXIdle(float lerpTime)
    {
        if(_vfxAnimationTransition.currentAnimation != VFXAnimationTransition.AnimationName.Idle)
        {
            _vfxAnimationTransition.ChangeAnimation(VFXAnimationTransition.AnimationName.Idle, lerpTime, false);
        }
        
    }
    
    private void VFXJump(float lerpTime)
    {
        if(_vfxAnimationTransition.currentAnimation != VFXAnimationTransition.AnimationName.Jump)
        {
            _vfxAnimationTransition.ChangeAnimation(VFXAnimationTransition.AnimationName.Jump, lerpTime, true);
        }
    }
    
    private void VFXFall(float lerpTime)
    {
        if(_vfxAnimationTransition.currentAnimation != VFXAnimationTransition.AnimationName.Fall)
        {
            _vfxAnimationTransition.ChangeAnimation(VFXAnimationTransition.AnimationName.Fall, lerpTime, false);
        }
    }
    
    public void VFXLanding(float lerpTime)
    {
        if(_vfxAnimationTransition.currentAnimation != VFXAnimationTransition.AnimationName.Land)
        {
            isLanding = true;
            _vfxAnimationTransition.ChangeAnimation(VFXAnimationTransition.AnimationName.Land, lerpTime, false);
        }
    }
    
    // Change to Idle after landing
    public void IsNotLanding()
    {
        isLanding = false;
        _vfxAnimationTransition.ChangeAnimation(VFXAnimationTransition.AnimationName.Idle, 0.1f, false);
    }

    // Returns true if player is moving and that movement is not the direction it is currently facing
    private bool IsMovingInNewDirection(Vector3 movement)
    {
        return !movement.sqrMagnitude.Equals(0f) && movement.normalized != transform.forward.normalized;
    }

    // If not already grounded, cast ray downwards to check for ground.
    private void CheckForIsGrounded()
    {
        if (isJump)
        {
            isGrounded = false;
            return;
        }
        raycastDirection = -transform.up;
        isGrounded = Physics.Raycast(raycastOrigin.position, raycastDirection, raycastDistance, raycastMask.value);
    }

    private void CheckIsJump()
    {
        if (!isJump) return;
        
        isJump = _rigidbody.velocity.y > -0.01f;
        animator.SetBool("IsJump", isJump);
    }

    private void SetAnimatorGrounding()
    {
        if (!animator) return;
        animator.SetBool("IsGrounded",isGrounded);
    }
    
    // Increment timer to delay fire rate by. Allows for firing once timer has reached the delayTime.
    private void FireDelay()
    {
        if (canFire) return;
        
        fireDelayTimer += Time.deltaTime;

        if (fireDelayTimer >= fireDelay)
        {
            canFire = true;
            fireDelayTimer = 0f;
        }
        
        if (animator) animator.SetBool("IsShooting",!canFire);
    }

    #endregion

    #region Event Listening methods

    // If the player is grounded, apply force upwards to jump (independent of mass)
    private void Jump(InputAction.CallbackContext context)
    {
        if (!isGrounded) return;
        if (animator)
        {
            isJump = true;
            animator.SetBool("IsJump", isJump);
            // VFXJump(0);
        }

        if (animator.gameObject != gameObject)
        {
            ApplyJumpForce();
        }
    }

    // Called by Animation Event to apply force on rigidbody
    public void ApplyJumpForce()
    {
        // if (!isGrounded) return;
        Vector3 movement = new Vector3(0f,1f,0f);
        movement *= jumpForce;
        _rigidbody.AddForce(movement,ForceMode.VelocityChange);
    }

    // If shooting is possible,
    // Plays the Shooting VAT Animation and sets the bool in the Animator to transition to the Shoot State.
    private void Fire(InputAction.CallbackContext context)
    {
        if (!canFire) return;
        if (_vfxAnimationTransition)
        {
            _vfxAnimationTransition.ChangeAnimation(VFXAnimationTransition.AnimationName.Shoot,0.1f,true); 
        }
        
        canFire = false;

        if (animator) animator.SetBool("IsShooting",!canFire);
        
    }

    // Spawn a particle system to shoot when called.
    private void FireProjectile()
    {
        tempShot = Instantiate(shotEmitter);
        tempShot.transform.SetPositionAndRotation(shotOrigin.position,shotOrigin.rotation);
        shootSystem = tempShot.GetComponent<ParticleSystem>();
        shootSystem.Play();
    }

    // Destroy spawned particle system if all shots are no longer alive
    private void DestroyIfStopped()
    {
        if (!tempShot) return;
        if (!shootSystem) return;
        if (!shootSystem.IsAlive())
        {
            Destroy(tempShot);
        }
    }

    #endregion

    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = raycastOrigin.position;
        Gizmos.DrawLine(pos,pos+raycastDirection*raycastDistance);
    }

    
}
