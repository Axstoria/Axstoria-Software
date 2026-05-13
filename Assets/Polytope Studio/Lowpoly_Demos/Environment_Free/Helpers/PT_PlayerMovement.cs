using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PT_PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 3;
    public float gravity = -9.18f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // ── Input ────────────────────────────────────────────────────────────
        float x        = 0f;
        float z        = 0f;
        bool  sprint   = false;
        bool  jumpDown = false;

#if ENABLE_INPUT_SYSTEM
        // New Input System (Unity 6 / package com.unity.inputsystem)
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed) z  =  1f;
            if (kb.sKey.isPressed) z  = -1f;
            if (kb.aKey.isPressed) x  = -1f;
            if (kb.dKey.isPressed) x  =  1f;

            sprint   = kb.leftShiftKey.isPressed;
            jumpDown = kb.spaceKey.wasPressedThisFrame;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        // Legacy Input Manager (Unity 2022 and earlier)
        x        = Input.GetAxis("Horizontal");
        z        = Input.GetAxis("Vertical");
        sprint   = Input.GetKey(KeyCode.LeftShift);
        jumpDown = Input.GetButtonDown("Jump");
#endif
        // ─────────────────────────────────────────────────────────────────────

        speed = (sprint && isGrounded) ? 10f : 5f;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (jumpDown && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
