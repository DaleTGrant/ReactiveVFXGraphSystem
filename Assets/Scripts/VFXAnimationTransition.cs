using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;
using UnityEngine.PlayerLoop;

public class VFXAnimationTransition : MonoBehaviour
{
    public enum AnimationName
    {
        Idle,
        Shoot,
        Walk,
        Run,
        Jump,
        Fall,
        Land
    };

    public AnimationName currentAnimation = AnimationName.Idle;
    public AnimationName nextAnimation = AnimationName.Shoot;
    [SerializeField] private AnimationName previousAnimation = AnimationName.Idle;

    public VisualEffect visualEffect;

    [SerializeField] private float fps = 30.0f;
    private float old_fps = 30.0f;
    
    // Arrays of all textures, dimensions, and frame count for each animation for the model
    public Texture2D[] vatArray = new Texture2D[5];
    public Vector2[] textureDimensionArray = new Vector2[5];
    public uint[] frameCountArray = new uint[5];

    private Texture2D currentVAT;
    private Texture2D nextVAT;
    
    private float transitionLerpValue;
    
    private float currentAnimationTimer;
    private float nextAnimationTimer;

    private Vector2 currentTextureDims;
    private Vector2 nextTextureDims;

    private uint currentFrameCount;
    private uint nextFrameCount;
    
    // Is the next animation to only be played once and return to the previous animation?
    private bool isOneShot = false;

    // For testing purposes
    public bool testLoopAnimation = false;
    public bool testOneShotAnimation = false;

    
    void Start()
    {
        // Intialize current animation variables
        SetCurrentVAT();
        currentAnimationTimer = 0f;
        visualEffect.SetFloat("Current Animation Timer",currentAnimationTimer);
        
        /// For testing ///////////////////
        testLoopAnimation = false;
        testOneShotAnimation = false;
        ///////////////////////////////////
    }
    
    void Update()
    {
        // Updates the FPS value during runtime in the case it changes
        if (!old_fps.Equals(fps))
        {
            visualEffect.SetFloat("FPS",fps);
            old_fps = fps;
        }
        
        // Update current animation
        currentAnimationTimer += Time.deltaTime;
        visualEffect.SetFloat("Current Animation Timer",currentAnimationTimer);

        // Revert back to the previous animation at the end of the current One-shot animation
        if (isOneShot && currentAnimationTimer >= currentFrameCount/fps && nextAnimationTimer.Equals(0.0f) && previousAnimation!=currentAnimation)
        {
            ChangeAnimation(previousAnimation,0.1f, false);
        }

        ////// For Testing /////////////////////////////////////////////////
        if (testLoopAnimation & !testOneShotAnimation)
        {
            var ani = currentAnimation == AnimationName.Idle? AnimationName.Shoot:AnimationName.Idle;
            
            ChangeAnimation(ani,0.25f, false);
            testLoopAnimation = false;
        }
        
        if (!testLoopAnimation & testOneShotAnimation)
        {
            var ani = currentAnimation == AnimationName.Idle? AnimationName.Shoot:AnimationName.Idle;
            
            ChangeAnimation(ani,0.25f, true);
            testOneShotAnimation = false;
        }
        ///////////A//////////////////////////////////////////////////////////
    }

    // Set the next animation and transition to it.
    // Can set how long the transition will take and if the new transition will only be played once or loop
    public void ChangeAnimation(AnimationName nextAnimationName, float lerpTime, bool isOneShot)
    {
        this.isOneShot = isOneShot;
        nextAnimation = nextAnimationName;
        SetNextVAT();
        transitionLerpValue = 0f;

        // Use if oneshot to set the return animation
        if(isOneShot && currentAnimation!=nextAnimation)
        {
            previousAnimation = currentAnimation;
        }
        
        // Lerp the transition value over the course of the lerp time
        DOTween.To(() => transitionLerpValue, x => transitionLerpValue = x, 1.0f, lerpTime)
            .OnStart(StartTransition)
            .OnUpdate(UpdateTransition)
            .OnComplete(ChangeCurrentVAT)
            .SetEase(Ease.Linear);
    }
    

    // resets the next animation timer so that it begins at the start
    private void StartTransition()
    {
        nextAnimationTimer = 0f;
        visualEffect.SetFloat("Next Animation Timer",nextAnimationTimer);
    }

    // Increments the next animation timer over the course of the lerp
    // Updates the next animation timer and lerp value in the VFX system each frame of the lerp
    private void UpdateTransition()
    {
        nextAnimationTimer += Time.deltaTime;
        visualEffect.SetFloat("Next Animation Timer",nextAnimationTimer);
        visualEffect.SetFloat("Transition Lerp Value",transitionLerpValue);
    }

    // Once the lerp is complete, change the current VAT to the next VAT and reset all related timers and lerps
    private void ChangeCurrentVAT()
    {
        currentAnimationTimer = nextAnimationTimer;
        nextAnimationTimer = 0f;
        transitionLerpValue = 0f;
        UpdateVFXParameters();
        
        currentAnimation = nextAnimation;
        SetCurrentVAT();
    }

    // Updates all timers and the lerp value in the vfx system
    private void UpdateVFXParameters()
    {
        visualEffect.SetFloat("Transition Lerp Value",transitionLerpValue);
        visualEffect.SetFloat("Current Animation Timer",currentAnimationTimer);
        visualEffect.SetFloat("Next Animation Timer",nextAnimationTimer);
    }

    // Extracts all values from the arrays according to the currentAnimation enum value
    // It then sets these values in the vfx system
    private void SetCurrentVAT()
    {
        var index = (int) currentAnimation;
        currentVAT = vatArray[index];
        currentTextureDims = textureDimensionArray[index];
        currentFrameCount = frameCountArray[index];
        
        visualEffect.SetTexture("Current VAT",currentVAT);
        visualEffect.SetVector2("Current Texture Dims", currentTextureDims);
        visualEffect.SetUInt("Current Frame Count",currentFrameCount);
    }
    
    // Same as for SetCurrentVAT except for the nextAnimation enum
    private void SetNextVAT()
    {
        var index = (int) nextAnimation;
        nextVAT = vatArray[index];
        nextTextureDims = textureDimensionArray[index];
        nextFrameCount = frameCountArray[index];
        
        visualEffect.SetTexture("Next VAT",nextVAT);
        visualEffect.SetVector2("Next Texture Dims", nextTextureDims);
        visualEffect.SetUInt("Next Frame Count",nextFrameCount);
    }
}
