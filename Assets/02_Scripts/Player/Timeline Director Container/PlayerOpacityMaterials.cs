using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Owns temporary materials for one Timeline graph. Shared project assets and
// renderer/Animator activation are never changed by the fade.
internal sealed class PlayerOpacityMaterials : IDisposable
{
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private readonly List<RendererMaterials> _renderers = new();
    private bool _isTransparent;

    public PlayerOpacityMaterials(Animator animator, Material fadeMaterial)
    {
        foreach (Renderer renderer in animator.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer is not SkinnedMeshRenderer && renderer is not MeshRenderer)
                continue;

            Material[] originals = renderer.sharedMaterials;
            var fades = new Material[originals.Length];
            var colors = new Color[originals.Length];
            for (int i = 0; i < originals.Length; i++)
            {
                Material source = originals[i];
                if (source == null)
                    continue;
                if (!source.HasProperty(BaseColor) || !source.HasProperty("_Surface"))
                {
                    fades[i] = source;
                    Debug.LogWarning($"Player Opacity: {source.name} must use a URP surface shader with _BaseColor and _Surface.", renderer);
                    continue;
                }

                var material = new Material(fadeMaterial != null ? fadeMaterial : source);
                material.CopyPropertiesFromMaterial(source);
                material.name = source.name + " (Timeline Fade)";
                material.hideFlags = HideFlags.HideAndDontSave;
                ConfigureTransparency(material);
                fades[i] = material;
                colors[i] = source.GetColor(BaseColor);
            }
            _renderers.Add(new RendererMaterials(renderer, originals, fades, colors));
        }
    }

    public void SetOpacity(float opacity)
    {
        opacity = Mathf.Clamp01(opacity);
        bool transparent = opacity < 1f;
        foreach (RendererMaterials entry in _renderers)
        {
            if (entry.Renderer == null)
                continue;
            if (transparent)
            {
                for (int i = 0; i < entry.Fades.Length; i++)
                {
                    if (entry.Fades[i] == null || entry.Fades[i] == entry.Originals[i])
                        continue;
                    Color color = entry.Colors[i];
                    color.a *= opacity;
                    entry.Fades[i].SetColor(BaseColor, color);
                }
            }
            if (transparent != _isTransparent)
                entry.Renderer.sharedMaterials = transparent ? entry.Fades : entry.Originals;
        }
        _isTransparent = transparent;
    }

    private static void ConfigureTransparency(Material material)
    {
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.SetFloat("_AlphaClip", 0f);
        material.SetFloat("_AlphaToMask", 0f);
        material.SetFloat("_BlendModePreserveSpecular", 0f);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.DisableKeyword("_ALPHAMODULATE_ON");
        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)RenderQueue.Transparent;
        // Invisible characters must not leave opaque shadows or depth silhouettes.
        material.SetShaderPassEnabled("ShadowCaster", false);
        material.SetShaderPassEnabled("DepthOnly", false);
        material.SetShaderPassEnabled("DepthNormals", false);
        material.SetShaderPassEnabled("MotionVectors", false);
    }

    public void Dispose()
    {
        foreach (RendererMaterials entry in _renderers)
        {
            if (_isTransparent && entry.Renderer != null)
                entry.Renderer.sharedMaterials = entry.Originals;
            for (int i = 0; i < entry.Fades.Length; i++)
            {
                Material material = entry.Fades[i];
                if (material == null || material == entry.Originals[i])
                    continue;
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(material);
                else
                    UnityEngine.Object.DestroyImmediate(material);
            }
        }
        _renderers.Clear();
        _isTransparent = false;
    }

    private sealed class RendererMaterials
    {
        public readonly Renderer Renderer;
        public readonly Material[] Originals;
        public readonly Material[] Fades;
        public readonly Color[] Colors;

        public RendererMaterials(Renderer renderer, Material[] originals, Material[] fades, Color[] colors)
        {
            Renderer = renderer;
            Originals = originals;
            Fades = fades;
            Colors = colors;
        }
    }
}
