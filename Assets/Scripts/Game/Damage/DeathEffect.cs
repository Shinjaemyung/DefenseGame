using Core.Utilities;
using UnityEngine;

namespace Core.Health
{
    public class DeathEffect : MonoBehaviour
    {
        [Header("Dissolve")]
        [Tooltip("연출 시간(초)")]
        [SerializeField] private float dissolveDuration = 1.2f;

        [Tooltip("디졸브 셰이더의 진행도 프로퍼티 이름 (1=완전히 보임, 0=완전히 사라짐 기준)")]
        [SerializeField] private string dissolveProgressPropertyName = "_DissolveProgress";

        [Tooltip("연출에 쓰일 풀링 프리팹")]
        [SerializeField] private GameObject pooledDeathEffectPrefab;

        private Renderer[] _sourceRenderers;
        private int _dissolveProgressId;

        DamageableBehaviour damageableBehaviour;
        Damageable _damageable;

        private void Awake()
        {
            damageableBehaviour = GetComponent<DamageableBehaviour>();

            if (damageableBehaviour != null)
            {
                _damageable = damageableBehaviour.configuration;
            }

            _dissolveProgressId = Shader.PropertyToID(dissolveProgressPropertyName);
            _sourceRenderers = GetComponentsInChildren<Renderer>(true);
        }

        private void OnEnable()
        {
            if (_damageable != null) 
                _damageable.Died += OnDied;
        }

        private void OnDisable()
        {
            if (_damageable != null)
                _damageable.Died -= OnDied;
        }

        private void OnDied(HealthChangeInfo healthChangeInfo)
        {
            if (pooledDeathEffectPrefab != null)
            {
                SpawnDissolveCorpse();
            }
        }

        /// <summary>
        /// 원본 오브젝트의 렌더러(파츠)마다 풀에서 PooledDissolveMesh를 하나씩 가져와
        /// 현재 위치/자세 그대로 표시하고 디졸브 연출을 재생시킨다.
        /// 원본 오브젝트(this.gameObject)는 건드리지 않으므로, 원본이 즉시 사라져도(풀 반환 등) 무관하다.
        /// </summary>
        private void SpawnDissolveCorpse()
        {
            foreach (var sourceRenderer in _sourceRenderers)
            {
                if (sourceRenderer == null || !sourceRenderer.enabled)
                {
                    continue;
                }

                PlayDissolvePart(sourceRenderer);
            }
        }

        /// <summary>
        /// 원본 렌더러 하나를 정적 메시(현재 포즈 기준)로 추출해서
        /// 풀에서 가져온 PooledDissolveMesh 하나에 재생시킨다.
        /// </summary>
        private void PlayDissolvePart(Renderer sourceRenderer)
        {
            Mesh mesh;

            if (sourceRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                mesh = new Mesh();
                skinnedMeshRenderer.BakeMesh(mesh);
            }
            else
            {
                var meshFilter = sourceRenderer.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    return;
                }

                mesh = meshFilter.sharedMesh;
            }

            Material[] sourceMaterials = sourceRenderer.sharedMaterials;

            var pooledObject = PoolManager.Instance.GetObject(pooledDeathEffectPrefab);
            var pooledDissolveMesh = pooledObject.GetComponent<PooledSlimeDeathEffect>();
            pooledDissolveMesh.Init(pooledDeathEffectPrefab);

            Transform sourceTransform = sourceRenderer.transform;
            pooledDissolveMesh.Play(
                mesh,
                sourceMaterials,
                _dissolveProgressId,
                dissolveDuration,
                sourceTransform.position,
                sourceTransform.rotation,
                sourceTransform.lossyScale);
        }
    }
}
