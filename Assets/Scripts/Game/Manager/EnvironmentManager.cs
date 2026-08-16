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
        public Color skyColor;
        public Color equatorColor;
        public Color groundColor;
        [Range(0f, 8f)] public float ambientIntensity;


        [Header("Directional Light (Sun)")]
        public Color sunColor = Color.white;
        [Range(0f, 8f)] public float sunIntensity = 1f;
        public Vector3 sunRotationEuler;

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
            SetEnvironment(presets[0].id);
        }
    }

    /// <summary>
    /// 런타임 중 스카이박스와 라이팅을 지정한 환경으로 전환합니다.
    /// </summary>
    /// <param name="env">전환할 환경</param>
    /// <param name="immediate">true면 트랜지션 없이 즉시 적용</param>
    public void SetEnvironment(Environment env)
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

        ApplyPresetImmediate(preset);

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

        RenderSettings.ambientSkyColor = preset.skyColor;
        RenderSettings.ambientEquatorColor = preset.equatorColor;
        RenderSettings.ambientGroundColor = preset.groundColor;
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
}
