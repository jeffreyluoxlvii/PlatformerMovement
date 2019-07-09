using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour
{
    public float timeToJumpApex = 0.4f;
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    float accelerationTimeAirborne = 0.2f;
    float accelerationTimeGrounded = 0.1f;
    float moveSpeed = 8;

    // Quality of life jumping variables
    float jumpPressedRemember;
    float jumpPressedRememberTime = 0.25f;

    public float wallSlideSpeedMax = 3f;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallJumpLeap;
    public float wallStickTime = 0.10f;
    float timeToWallUnstick;

    float maxJumpVelocity;
    float minJumpVelocity;
    float gravity;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;

    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D>();
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        print("Gravity: " + gravity + " Jump Velocity: " + maxJumpVelocity);
    }
    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnJumpInputDown()
    {

        jumpPressedRemember = jumpPressedRememberTime;
    }

    public void OnJumpInputUp()
    {
        if (minJumpVelocity < velocity.y)
        {
            velocity.y = minJumpVelocity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        wallDirX = (controller.collisions.left) ? -1 : 1;

        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
            (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;

        wallSliding = false;
        if((controller.collisions.left || controller.collisions.right) && !controller.collisions.above && !controller.collisions.below)
        {
            wallSliding = true;

            if(velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if(timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if(directionalInput.x != wallDirX && directionalInput.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
                
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }

        if(controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        jumpPressedRemember -= Time.deltaTime;

        if (jumpPressedRemember > 0)
        {
            if (wallSliding)
            {
                if(wallDirX == directionalInput.x)
                {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if(directionalInput.x == 0)
                {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else
                {
                    velocity.x = -wallDirX * wallJumpLeap.x;
                    velocity.y = wallJumpLeap.y;
                }
            }
            if (controller.collisions.below)
            {
                velocity.y = maxJumpVelocity;
                jumpPressedRemember = 0;
            }
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
