using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Pulses a shader "fresnel intensity" property on this object's
    /// <see cref="Renderer"/> via a <see cref="MaterialPropertyBlock"/>, so
    /// the effect is per-instance without cloning or animating the shared
    /// Material (see the template's "never animate a shared Material
    /// directly" rule).
    ///
    /// A pulse is one full swing from <see cref="minValue"/> up to
    /// <see cref="maxValue"/> and back down to <see cref="minValue"/>. Once a
    /// pulse completes, the value holds at <see cref="minValue"/> for
    /// <see cref="pulsePause"/> seconds before the next pulse starts.
    ///
    /// Expects the renderer's material to use the retro 2-layer Shader
    /// Graph, which exposes a float property (named <see cref="propertyName"/>,
    /// "_FresnelPulse" by default) that gets multiplied into the shader's
    /// always-on fresnel rim-light output.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class FresnelPulse : MonoBehaviour
    {
        private const float TwoPi = Mathf.PI * 2f;

        [Tooltip("Name of the exposed shader float property driving the fresnel rim-light intensity.")]
        [SerializeField]
        private string propertyName = "_FresnelPulse";

        [SerializeField] private float minValue;
        [SerializeField] private float maxValue = 1f;

        [Tooltip("Angular speed of a single pulse (min -> max -> min), in radians/second.")]
        [SerializeField]
        private float speed = 1f;

        [Tooltip("Seconds to hold at minValue after a pulse completes, before the next one starts.")]
        [SerializeField]
        private float pulsePause;

        private Renderer targetRenderer;
        private MaterialPropertyBlock propertyBlock;
        private int propertyId;

        private float phase;
        private float pauseTimeRemaining;
        private bool isPausing;

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
            propertyId = Shader.PropertyToID(propertyName);
        }

        private void Update()
        {
            ApplyValue(Tick(Time.deltaTime));
        }

        private float Tick(float deltaTime)
        {
            if (isPausing)
            {
                pauseTimeRemaining -= deltaTime;
                if (pauseTimeRemaining <= 0f)
                {
                    isPausing = false;
                    phase = 0f;
                }

                return minValue;
            }

            phase += deltaTime * speed;
            if (phase >= TwoPi)
            {
                phase = TwoPi;
                isPausing = true;
                pauseTimeRemaining = pulsePause;
            }

            // 0 at the start/end of the pulse, 1 at its midpoint.
            float t = (1f - Mathf.Cos(phase)) * 0.5f;
            return Mathf.Lerp(minValue, maxValue, t);
        }

        private void ApplyValue(float value)
        {
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(propertyId, value);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
