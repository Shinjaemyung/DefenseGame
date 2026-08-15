using System;
using System.Collections;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }

    public enum Environment
    {
        Environment1,
        Environment2,
        Environment3,
    }

    [Serializable]
    public class EnvironmentPreset
    {
        public Environment id;

        [Header("Skybox")]
        public Material skyboxMaterial;

        [Header("Ambient Light")]
        public Color ambientColor = Color.gray;
        [Range(0f, 8f)] public float ambientIntensity = 1f;

        [Header("Directional Light (Sun)")]
        public Color sunColor = Color.white;
        [Range(0f, 8f)] public float sunIntensity = 1f;
        public Vector3 sunRotationEuler = new Vector3(50f, -30f, 0f);

        [Header("Fog")]
        public bool fogEnabled = false;
        public Color fogColor = Color.gray;
        [Range(0f, 1f)] public float fogDensity = 0.01f;
    }

    [Header("Environment Presets")]
    [SerializeField] public EnvironmentPreset[] presets;

    [Header("References")]
    [Tooltip("씬의 태양 역할을 하는 Directional Light")]
    [SerializeField] private Light sunLight;

    [Header("Transition")]
    [SerializeField] private float transitionDuration = 1.5f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public Environment CurrentEnvironment { get; private set; }
    public event Action<Environment> OnEnvironmentChanged;

    private Coroutine transitionRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 시작 시 첫 번째 프리셋을 즉시 적용
        if (presets != null && presets.Length > 0)
        {
            SetEnvironment(presets[0].id, immediate: true);
        }
    }

    /// <summary>
    /// 런타임 중 스카이박스와 라이팅을 지정한 환경으로 전환합니다.
    /// </summary>
    /// <param name="env">전환할 환경</param>
    /// <param name="immediate">true면 트랜지션 없이 즉시 적용</param>
    public void SetEnvironment(Environment env, bool immediate = true)
    {
        EnvironmentPreset preset = FindPreset(env);
        if (preset == null)
        {
            Debug.LogWarning($"[EnvironmentManager] '{env}'에 해당하는 프리셋이 없습니다.");
            return;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (immediate || transitionDuration <= 0f || !Application.isPlaying || !gameObject.activeInHierarchy)
        {
            ApplyPresetImmediate(preset);
        }
        else
        {
            transitionRoutine = StartCoroutine(TransitionToPreset(preset));
        }

        CurrentEnvironment = env;
        OnEnvironmentChanged?.Invoke(env);
    }

    private EnvironmentPreset FindPreset(Environment env)
    {
        if (presets == null) return null;
        foreach (var p in presets)
        {
            if (p.id == env) return p;
        }
        return null;
    }

    private void ApplyPresetImmediate(EnvironmentPreset preset)
    {
        if (preset.skyboxMaterial != null)
            RenderSettings.skybox = preset.skyboxMaterial;

        RenderSettings.ambientLight = preset.ambientColor;
        RenderSettings.ambientIntensity = preset.ambientIntensity;

        RenderSettings.fog = preset.fogEnabled;
        RenderSettings.fogColor = preset.fogColor;
        RenderSettings.fogDensity = preset.fogDensity;

        if (sunLight != null)
        {
            sunLight.color = preset.sunColor;
            sunLight.intensity = preset.sunIntensity;
            sunLight.transform.rotation = Quaternion.Euler(preset.sunRotationEuler);
        }

        DynamicGI.UpdateEnvironment();
    }

    private IEnumerator TransitionToPreset(EnvironmentPreset target)
    {
        // 전환 시작 시점의 값들을 캡처
        Color startAmbient = RenderSettings.ambientLight;
        float startAmbientIntensity = RenderSettings.ambientIntensity;
        Color startFogColor = RenderSettings.fogColor;
        float startFogDensity = RenderSettings.fogDensity;

        Color startSunColor = sunLight != null ? sunLight.color : Color.white;
        float startSunIntensity = sunLight != null ? sunLight.intensity : 1f;
        Quaternion startSunRot = sunLight != null ? sunLight.transform.rotation : Quaternion.identity;
        Quaternion targetSunRot = Quaternion.Euler(target.sunRotationEuler);

        // 스카이박스 재질 자체는 블렌드 셰이더 없이는 즉시 전환만 가능하므로 바로 교체하고,
        // 그 주변 라이팅 값들을 서서히 보간하여 전환이 부드럽게 느껴지도록 합니다.
        if (target.skyboxMaterial != null)
            RenderSettings.skybox = target.skyboxMaterial;

        if (target.fogEnabled) RenderSettings.fog = true;

        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float lerp = transitionCurve.Evaluate(Mathf.Clamp01(t / transitionDuration));

            RenderSettings.ambientLight = Color.Lerp(startAmbient, target.ambientColor, lerp);
            RenderSettings.ambientIntensity = Mathf.Lerp(startAmbientIntensity, target.ambientIntensity, lerp);

            RenderSettings.fogColor = Color.Lerp(startFogColor, target.fogColor, lerp);
            RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, target.fogDensity, lerp);

            if (sunLight != null)
            {
                sunLight.color = Color.Lerp(startSunColor, target.sunColor, lerp);
                sunLight.intensity = Mathf.Lerp(startSunIntensity, target.sunIntensity, lerp);
                sunLight.transform.rotation = Quaternion.Slerp(startSunRot, targetSunRot, lerp);
            }

            DynamicGI.UpdateEnvironment();
            yield return null;
        }

        // 마지막에 목표 값으로 정확히 스냅
        ApplyPresetImmediate(target);
        transitionRoutine = null;
    }
}
