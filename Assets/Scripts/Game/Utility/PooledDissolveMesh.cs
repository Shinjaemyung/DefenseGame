using System.Collections;
using UnityEngine;

/// <summary>
/// PoolManager로 재사용되는, 메시 하나를 디졸브시키며 사라지는 오브젝트.
/// 죽은 오브젝트의 메시(파츠 단위)를 복제해 그 자리에 그대로 보여주고,
/// 디졸브 연출이 끝나면 자동으로 풀에 반환된다.
///
/// 사용법: Play()로 재생을 시작하면, 지정한 시간 동안 디졸브 진행도를
/// 1(완전히 보임) -> 0(완전히 사라짐)으로 애니메이션한 뒤 스스로 풀에 반환된다.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PooledDissolveMesh : Poolable
{
    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer;

    Material _dissolveInstance;

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// 지정한 메시를 지정한 위치/자세로 표시하고 디졸브 연출을 재생한다.
    /// </summary>
    /// <param name="mesh">표시할 메시(원본 렌더러에서 복제/베이크된 메시)</param>
    /// <param name="materialCount">머티리얼 슬롯 개수 (원본 렌더러의 서브메시 개수와 동일해야 함)</param>
    /// <param name="dissolveMaterialTemplate">디졸브 셰이더를 쓰는 머티리얼 템플릿</param>
    /// <param name="progressPropertyId">디졸브 진행도 셰이더 프로퍼티 ID (Shader.PropertyToID로 미리 계산)</param>
    /// <param name="duration">디졸브 연출 시간(초)</param>
    /// <param name="position">월드 위치</param>
    /// <param name="rotation">월드 회전</param>
    /// <param name="scale">월드(lossy) 스케일</param>
    public void Play(Mesh mesh, int materialCount, Material dissolveMaterialTemplate, int progressPropertyId,
        float duration, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = scale;

        _meshFilter.sharedMesh = mesh;

        if (_dissolveInstance == null)
        {
            _dissolveInstance = new Material(dissolveMaterialTemplate);
        }
        else if (_dissolveInstance.shader != dissolveMaterialTemplate.shader)
        {
            _dissolveInstance.shader = dissolveMaterialTemplate.shader;
        }

        var materials = new Material[materialCount];
        for (int i = 0; i < materialCount; i++)
        {
            materials[i] = _dissolveInstance;
        }
        _meshRenderer.sharedMaterials = materials;

        _dissolveInstance.SetFloat(progressPropertyId, 1f);

        StopAllCoroutines();
        StartCoroutine(DissolveRoutine(progressPropertyId, duration));
    }

    IEnumerator DissolveRoutine(int progressPropertyId, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Lerp(1f, 0f, Mathf.Clamp01(elapsed / duration));
            _dissolveInstance.SetFloat(progressPropertyId, progress);
            yield return null;
        }

        _dissolveInstance.SetFloat(progressPropertyId, 0f);
        ReturnToPool();
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        _meshFilter.sharedMesh = null;
    }
}
