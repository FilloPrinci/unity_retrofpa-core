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
    public class FirstPersonController : MonoBehaviour, ITeleportable
    {
        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float maxFallSpeed = 20f;

        [Header("Look")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float lookSensitivity = 0.1f;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;

        /// <summary>True while the cursor is locked — i.e. while no UI screen is capturing input.</summary>
        public static bool IsCursorLocked { get; private set; }

        private static int gameplayInputBlockCount;

        /// <summary>
        /// True while gameplay input (look/move/interact/attack) should run:
        /// the cursor is locked and nothing has blocked gameplay input via
        /// <see cref="BlockGameplayInput"/> (e.g. a modal description box,
        /// which halts the player without freeing the cursor).
        /// </summary>
        public static bool IsGameplayInputEnabled => IsCursorLocked && gameplayInputBlockCount == 0;

        /// <summary>Halts gameplay input, keeping the cursor locked. Counted: each call needs a matching <see cref="UnblockGameplayInput"/>.</summary>
        public static void BlockGameplayInput() => gameplayInputBlockCount++;

        public static void UnblockGameplayInput() => gameplayInputBlockCount = Mathf.Max(0, gameplayInputBlockCount - 1);

        // Statics survive entering Play mode when domain reload is disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => gameplayInputBlockCount = 0;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            moveAction?.action.Enable();
            lookAction?.action.Enable();
            SetCursorLocked(true);
            SettingsManager.LookSensitivityChanged += HandleLookSensitivityChanged;
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            lookAction?.action.Disable();
            SetCursorLocked(false);
            SettingsManager.LookSensitivityChanged -= HandleLookSensitivityChanged;
        }

        private void HandleLookSensitivityChanged(float value) => lookSensitivity = value;

        /// <summary>
        /// Locks/hides (or frees/shows) the cursor, and gates gameplay input
        /// (look/move/interact/attack) on it via <see cref="IsGameplayInputEnabled"/>.
        /// Exposed so a UI screen (pause menu, inventory, ...) can release
        /// the cursor — and gameplay input with it — while open, without
        /// this controller needing to know about that UI.
        /// </summary>
        public static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
            IsCursorLocked = locked;
        }

        /// <summary>Moves this controller to <paramref name="position"/>/<paramref name="rotation"/>, resetting accumulated fall velocity (a plain Transform move would leave stale velocity from before the teleport).</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            verticalVelocity = 0f;
            controller.enabled = true;
        }

        private void Update()
        {
            if (!IsGameplayInputEnabled)
            {
                return;
            }

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
            verticalVelocity = Mathf.Max(verticalVelocity + gravity * Time.deltaTime, -maxFallSpeed);

            Vector3 velocity = move * moveSpeed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
