using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PT_MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    public Transform playerBody;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // ── Input ────────────────────────────────────────────────────────────
        float mouseX = 0f;
        float mouseY = 0f;

#if ENABLE_INPUT_SYSTEM
        // New Input System (Unity 6 / package com.unity.inputsystem)
        var mouse = Mouse.current;
        if (mouse != null)
        {
            // Mouse.current.delta is already in pixels/frame — no Time.deltaTime needed,
            // but we keep the same scaling feel as the legacy path by dividing by a
            // reference frame-time (0.02 ≈ 50 fps) so sensitivity values stay comparable.
            Vector2 delta = mouse.delta.ReadValue();
            mouseX = delta.x * mouseSensitivity * 0.02f;
            mouseY = delta.y * mouseSensitivity * 0.02f;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        // Legacy Input Manager (Unity 2022 and earlier)
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
#endif
        // ─────────────────────────────────────────────────────────────────────

        xRotation -= mouseY;
        xRotation  = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
