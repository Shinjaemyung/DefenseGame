using System.Collections;
using UnityEngine;

/// <summary>
/// PoolManager로 재사용되는, 메시 하나를 디졸브시키며 사라지는 오브젝트.
/// 죽은 오브젝트의 메시(파츠 단위)를 복제해 그 자리에 그대로 보여주고,
/// 디졸브 연출이 끝나면 자동으로 풀에 반환된다.
///
/// 새 머티리얼을 씌우지 않고, 원본 렌더러가 쓰던 머티리얼을 그대로(런타임 인스턴스로 복제해서)
/// 사용한다. 즉 원본 머티리얼의 셰이더에 디졸브 진행도 프로퍼티가 이미 있어야 한다.
///
/// 사용법: Play()로 재생을 시작하면, 지정한 시간 동안 디졸브 진행도를
/// 1(완전히 보임) -> 0(완전히 사라짐)으로 애니메이션한 뒤 스스로 풀에 반환된다.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PooledSlimeDeathEffect : Poolable
{
    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer;

    Material[] _runtimeMaterials;

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// 지정한 메시를 지정한 위치/자세로 표시하고, 원본 머티리얼을 복제해 디졸브 연출을 재생한다.
    /// </summary>
    /// <param name="mesh">표시할 메시(원본 렌더러에서 복제/베이크된 메시)</param>
    /// <param name="sourceMaterials">원본 렌더러가 쓰던 머티리얼 배열. 셰이더에 디졸브 진행도 프로퍼티가 있어야 함</param>
    /// <param name="progressPropertyId">디졸브 진행도 셰이더 프로퍼티 ID (Shader.PropertyToID로 미리 계산)</param>
    /// <param name="duration">디졸브 연출 시간(초)</param>
    /// <param name="position">월드 위치</param>
    /// <param name="rotation">월드 회전</param>
    /// <param name="scale">월드(lossy) 스케일</param>
    public void Play(Mesh mesh, Material[] sourceMaterials, int progressPropertyId,
        float duration, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = scale;

        _meshFilter.sharedMesh = mesh;

        ReleaseRuntimeMaterials();

        _runtimeMaterials = new Material[sourceMaterials.Length];
        for (int i = 0; i < sourceMaterials.Length; i++)
        {
            _runtimeMaterials[i] = new Material(sourceMaterials[i]);
        }
        _meshRenderer.sharedMaterials = _runtimeMaterials;

        SetProgress(progressPropertyId, 0f);

        StopAllCoroutines();
        StartCoroutine(DissolveRoutine(progressPropertyId, duration));
    }

    IEnumerator DissolveRoutine(int progressPropertyId, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Lerp(0f, 1f, Mathf.Clamp01(elapsed / duration));
            SetProgress(progressPropertyId, progress);
            yield return null;
        }

        SetProgress(progressPropertyId, 1f);
        ReturnToPool();
    }

    void SetProgress(int progressPropertyId, float value)
    {
        if (_runtimeMaterials == null)
        {
            return;
        }

        for (int i = 0; i < _runtimeMaterials.Length; i++)
        {
            _runtimeMaterials[i].SetFloat(progressPropertyId, value);
        }
    }

    void ReleaseRuntimeMaterials()
    {
        if (_runtimeMaterials == null)
        {
            return;
        }

        for (int i = 0; i < _runtimeMaterials.Length; i++)
        {
            Destroy(_runtimeMaterials[i]);
        }
        _runtimeMaterials = null;
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        _meshFilter.sharedMesh = null;
        ReleaseRuntimeMaterials();
    }
}
