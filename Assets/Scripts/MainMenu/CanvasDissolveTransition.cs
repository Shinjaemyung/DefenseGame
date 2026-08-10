using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    /// <summary>
    /// Canvas 전체를 스크린샷으로 캡처한 뒤, 픽셀 디졸브 셰이더로 화면을 사라지게 하고
    /// 애니메이션이 끝나면 지정한 씬을 로드한다.
    ///
    /// 사용법:
    /// 1) Main Menu Canvas와 같은 계층(혹은 그 위)에 이 컴포넌트를 붙인 오브젝트를 둔다.
    /// 2) 화면 전체를 덮는 RawImage(별도의, sortingOrder가 가장 높은 Canvas 위에 두는 걸 추천)를
    ///    만들어 dissolveOverlay에 연결한다.
    /// 3) Assets/Shaders/PixelDissolve.shader를 쓰는 머티리얼을 만들어 dissolveMaterialTemplate에 연결한다.
    ///    머티리얼의 _NoiseTex에는 흑백 노이즈 텍스처를 지정해야 한다.
    /// 4) 씬 전환이 필요한 곳(예: UI_GameStartButton)에서
    ///    CanvasDissolveTransition.PlayDissolveThenLoadScene("GamePlayScene")을 호출한다.
    /// </summary>
    [DisallowMultipleComponent]
    public class CanvasDissolveTransition : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("디졸브 대상이 되는 Canvas. 캡처 직후 자동으로 비활성화된다.")]
        [SerializeField] Canvas targetCanvas;

        [Tooltip("화면 전체를 덮는 RawImage. 캡처된 스크린샷 + 디졸브 셰이더가 여기에 표시된다.")]
        [SerializeField] RawImage dissolveOverlay;

        [Tooltip("Assets/Shaders/PixelDissolve.shader를 사용하는 머티리얼 템플릿.")]
        [SerializeField] Material dissolveMaterialTemplate;

        [Header("Settings")]
        [SerializeField] float duration = 1.2f;
        [SerializeField] AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        Material _runtimeMaterial;
        RenderTexture _captureTexture;
        bool _isPlaying;

        void Awake()
        {
            if (dissolveMaterialTemplate != null)
            {
                _runtimeMaterial = new Material(dissolveMaterialTemplate);

                if (dissolveOverlay != null)
                {
                    dissolveOverlay.material = _runtimeMaterial;
                }
            }

            if (dissolveOverlay != null)
            {
                dissolveOverlay.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 대상 Canvas를 캡처해 픽셀 디졸브로 사라지게 한 뒤 지정한 씬을 로드한다.
        /// </summary>
        public void PlayDissolveThenLoadScene(string sceneName)
        {
            if (_isPlaying)
            {
                return;
            }

            StartCoroutine(DissolveRoutine(sceneName));
        }

        IEnumerator DissolveRoutine(string sceneName)
        {
            _isPlaying = true;

            yield return CaptureScreenToOverlay();

            if (targetCanvas != null)
            {
                targetCanvas.enabled = false;
            }

            if (_runtimeMaterial != null)
            {
                _runtimeMaterial.SetFloat("_DissolveAmount", 1f);
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float linear = Mathf.Clamp01(elapsed / duration);
                float eased = easeCurve.Evaluate(linear);
                float dissolveValue = 1f - eased;

                if (_runtimeMaterial != null)
                {
                    _runtimeMaterial.SetFloat("_DissolveAmount", dissolveValue);
                }

                yield return null;
            }

            if (_runtimeMaterial != null)
            {
                _runtimeMaterial.SetFloat("_DissolveAmount", 0f);
            }

            SceneManager.LoadScene(sceneName);
        }

        IEnumerator CaptureScreenToOverlay()
        {
            // Canvas의 렌더링 결과가 실제로 화면에 반영된 시점(프레임 끝)까지 대기
            yield return new WaitForEndOfFrame();

            if (_captureTexture != null)
            {
                _captureTexture.Release();
            }

            _captureTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(_captureTexture);

            if (dissolveOverlay != null)
            {
                dissolveOverlay.texture = _captureTexture;
                dissolveOverlay.uvRect = new Rect(0f, 1f, 1f, -1f);
                dissolveOverlay.gameObject.SetActive(true);
            }
        }

        void OnDestroy()
        {
            if (_runtimeMaterial != null)
            {
                Destroy(_runtimeMaterial);
            }

            if (_captureTexture != null)
            {
                _captureTexture.Release();
                Destroy(_captureTexture);
            }
        }
    }
}
