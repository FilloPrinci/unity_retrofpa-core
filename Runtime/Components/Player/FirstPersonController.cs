using UnityEngine;
using UnityEngine.InputSystem;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Minimal first-person movement: WASD-style move via
    /// <see cref="CharacterController"/>, mouse look (yaw on this transform,
    /// pitch on <see cref="cameraTransform"/>). No jump/crouch/sprint/head-bob
    /// yet — deliberately minimal, extend later.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float gravity = -20f;

        [Header("Look")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float lookSensitivity = 0.1f;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            moveAction?.action.Enable();
            lookAction?.action.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            lookAction?.action.Disable();
        }

        private void Update()
        {
            ApplyLook();
            ApplyMove();
        }

        private void ApplyLook()
        {
            if (lookAction == null)
            {
                return;
            }

            Vector2 lookDelta = lookAction.action.ReadValue<Vector2>() * lookSensitivity;

            transform.Rotate(Vector3.up, lookDelta.x);

            pitch = Mathf.Clamp(pitch - lookDelta.y, minPitch, maxPitch);
            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void ApplyMove()
        {
            Vector2 input = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
            Vector3 move = transform.right * input.x + transform.forward * input.y;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = move * moveSpeed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
