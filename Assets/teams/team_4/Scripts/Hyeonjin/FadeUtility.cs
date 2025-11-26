using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FadeUtility : MonoBehaviour
{
    public static FadeUtility Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ========== 기존 함수 (그대로 유지) ==========
    public void FadeIn(GameObject target, float duration = 1f, float delay = 0f)
    {
        StartCoroutine(FadeRoutine(target, 0f, 1f, duration, delay, true));
    }

    public void FadeOut(GameObject target, float duration = 1f, float delay = 0f)
    {
        StartCoroutine(FadeRoutine(target, 1f, 0f, duration, delay, false));
    }

    private IEnumerator FadeRoutine(GameObject target, float startAlpha, float endAlpha, float duration, float delay, bool isFadeIn)
    {
        if (target == null) yield break;
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        SetAlpha(renderers, startAlpha);

        if (isFadeIn)
            SetRendererEnabled(renderers, false);

        if (delay > 0)
            yield return new WaitForSeconds(delay);

        if (isFadeIn)
            SetRendererEnabled(renderers, true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (target == null) yield break;
            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            SetAlpha(renderers, alpha);

            yield return null;
        }

        SetAlpha(renderers, endAlpha);

        if (!isFadeIn)
            SetRendererEnabled(renderers, false);
    }

    // ========== 새로운 Opaque용 함수 ==========
    public void FadeInOpaque(GameObject target, float duration = 1f, float delay = 0f)
    {
        StartCoroutine(FadeOpaqueRoutine(target, 0f, 1f, duration, delay, true));
    }

    public void FadeOutOpaque(GameObject target, float duration = 1f, float delay = 0f)
    {
        StartCoroutine(FadeOpaqueRoutine(target, 1f, 0f, duration, delay, false));
    }

    private IEnumerator FadeOpaqueRoutine(GameObject target, float startAlpha, float endAlpha, float duration, float delay, bool isFadeIn)
    {
        if (target == null) yield break;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        // ⭐ 1. 원래 Material 설정 저장
        Dictionary<Material, OpaqueMaterialState> originalStates = new Dictionary<Material, OpaqueMaterialState>();
        
        foreach (var r in renderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
            {
                if (mat == null || originalStates.ContainsKey(mat)) continue;
                
                originalStates[mat] = new OpaqueMaterialState
                {
                    surfaceType = mat.HasProperty("_Surface") ? mat.GetFloat("_Surface") : 0,
                    blendMode = mat.HasProperty("_Blend") ? mat.GetFloat("_Blend") : 0,
                    renderQueue = mat.renderQueue,
                    srcBlend = mat.HasProperty("_SrcBlend") ? mat.GetInt("_SrcBlend") : 1,
                    dstBlend = mat.HasProperty("_DstBlend") ? mat.GetInt("_DstBlend") : 0,
                    zWrite = mat.HasProperty("_ZWrite") ? mat.GetFloat("_ZWrite") : 1,
                    alphaClip = mat.HasProperty("_AlphaClip") ? mat.GetFloat("_AlphaClip") : 0,
                    color = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : 
                            (mat.HasProperty("_Color") ? mat.color : Color.white)
                };
            }
        }

        // ⭐ 2. Transparent로 변경
        ConvertToTransparent(renderers);
        SetAlphaOpaque(renderers, startAlpha);

        if (isFadeIn)
            SetRendererEnabled(renderers, false);

        if (delay > 0)
            yield return new WaitForSeconds(delay);

        if (isFadeIn)
            SetRendererEnabled(renderers, true);

        // ⭐ 3. Fade 애니메이션
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (target == null) yield break;
            elapsed += Time.deltaTime;

            float t = elapsed / duration;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            SetAlphaOpaque(renderers, alpha);

            yield return null;
        }

        SetAlphaOpaque(renderers, endAlpha);

        // ⭐ 4. 원래 Opaque 상태로 복구
        if (isFadeIn)
        {
            RestoreToOpaque(renderers, originalStates);
        }
        else
        {
            SetRendererEnabled(renderers, false);
        }
    }

    // ⭐ Opaque → Transparent 변환
    private void ConvertToTransparent(Renderer[] renderers)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
            {
                if (mat == null) continue;

                // ⭐ Surface Type: Transparent
                mat.SetFloat("_Surface", 1);
                
                // ⭐ Blend Mode: Alpha
                mat.SetFloat("_Blend", 0); // 0=Alpha, 1=Premultiply, 2=Additive, 3=Multiply
                
                // ⭐ Blend 설정
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                
                // ⭐ ZWrite Off
                mat.SetFloat("_ZWrite", 0);
                
                // ⭐ Alpha Clip Off
                mat.SetFloat("_AlphaClip", 0);
                
                // ⭐ Render Queue: Transparent
                mat.renderQueue = 3000;
                
                // ⭐ Shader Keywords
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                
                // ⭐ Render Type Tag
                mat.SetOverrideTag("RenderType", "Transparent");
                
                // ⭐ Render Face: Both (양면 렌더링)
                mat.SetFloat("_Cull", 0);
            }
        }
    }

    // ⭐ Transparent → Opaque 복구
    private void RestoreToOpaque(Renderer[] renderers, Dictionary<Material, OpaqueMaterialState> originalStates)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
            {
                if (mat == null || !originalStates.ContainsKey(mat)) continue;

                var state = originalStates[mat];

                // 설정 복구
                mat.SetFloat("_Surface", state.surfaceType);
                mat.SetFloat("_Blend", state.blendMode);
                mat.SetInt("_SrcBlend", state.srcBlend);
                mat.SetInt("_DstBlend", state.dstBlend);
                mat.SetFloat("_ZWrite", state.zWrite);
                mat.SetFloat("_AlphaClip", state.alphaClip);
                mat.renderQueue = state.renderQueue;
                
                // Color 복구 (Alpha = 1)
                Color restoredColor = state.color;
                restoredColor.a = 1f;
                
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", restoredColor);
                else if (mat.HasProperty("_Color"))
                    mat.color = restoredColor;
                
                // Shader Keywords 제거
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.SetOverrideTag("RenderType", "Opaque");
                
                // Cull 복구
                mat.SetFloat("_Cull", 2); // Back
            }
        }
    }

    // ⭐ Alpha 설정 (URP용)
    private void SetAlphaOpaque(Renderer[] renderers, float alpha)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
            {
                if (mat == null) continue;
                
                // URP: _BaseColor
                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = alpha;
                    mat.SetColor("_BaseColor", c);
                }
                // Legacy: _Color
                else if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }
        }
    }

    // ========== 기존 Helper 함수 ==========
    private void SetAlpha(Renderer[] renderers, float alpha)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
            {
                if (mat == null) continue;
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }
        }
    }

    private void SetRendererEnabled(Renderer[] renderers, bool enabled)
    {
        foreach (var r in renderers)
        {
            if (r != null)
                r.enabled = enabled;
        }
    }

    // ⭐ Material 상태 저장용 클래스
    private class OpaqueMaterialState
    {
        public float surfaceType;
        public float blendMode;
        public int renderQueue;
        public int srcBlend;
        public int dstBlend;
        public float zWrite;
        public float alphaClip;
        public Color color;
    }
}