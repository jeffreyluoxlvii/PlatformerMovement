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

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D>();
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        print("Gravity: " + gravity + " Jump Velocity: " + maxJumpVelocity);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        int wallDirX = (controller.collisions.left) ? -1 : 1;

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
            (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;

        bool wallSliding = false;
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

                if(input.x != wallDirX && input.x != 0)
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

        // Sets jumpPressedRemember when the player presses jump
        if (Input.GetButtonDown("Jump"))
        {
            jumpPressedRemember = jumpPressedRememberTime;
        }
        jumpPressedRemember -= Time.deltaTime;

        if (jumpPressedRemember > 0)
        {
            if (wallSliding)
            {
                if(wallDirX == input.x)
                {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if(input.x == 0)
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
        if (Input.GetButtonUp("Jump"))
        {
            if(minJumpVelocity < velocity.y) {
                velocity.y = minJumpVelocity;
            }
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
