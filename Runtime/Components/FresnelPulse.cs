using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Continuously pulses a shader "fresnel intensity" property on this
    /// object's <see cref="Renderer"/> via a <see cref="MaterialPropertyBlock"/>,
    /// so the effect is per-instance without cloning or animating the shared
    /// Material (see the template's "never animate a shared Material
    /// directly" rule).
    ///
    /// Expects the renderer's material to use the retro 2-layer Shader
    /// Graph, which exposes a float property (named <see cref="propertyName"/>,
    /// "_FresnelPulse" by default) that gets multiplied into the shader's
    /// always-on fresnel rim-light output.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class FresnelPulse : MonoBehaviour
    {
        [Tooltip("Name of the exposed shader float property driving the fresnel rim-light intensity.")]
        [SerializeField]
        private string propertyName = "_FresnelPulse";

        [SerializeField] private float minValue;
        [SerializeField] private float maxValue = 1f;
        [SerializeField] private float speed = 1f;

        private Renderer targetRenderer;
        private MaterialPropertyBlock propertyBlock;
        private int propertyId;

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
            propertyId = Shader.PropertyToID(propertyName);
        }

        private void Update()
        {
            float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f; // 0..1
            float value = Mathf.Lerp(minValue, maxValue, t);

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(propertyId, value);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
