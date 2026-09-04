using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Slider _slider;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Vector2 _offset;
    [SerializeField] private float _slideLerpSpeed = 10f;

    private RectTransform _canvasRectTransform;
    private Camera _cam;

    private EnemyCore _enemy;

    private float _targetValue = 0f;
    private float _targetAlpha = 1f;
    private float _fadeDuration;

    public float Alpha => _canvasGroup != null ? _canvasGroup.alpha : 1f;

    private void Awake()
    {
        if (_canvasGroup == null && !TryGetComponent(out _canvasGroup))
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (_enemy == null)
            return;

        _slider.value = Mathf.Lerp(_slider.value, _targetValue, Time.deltaTime * _slideLerpSpeed);
        UpdateAlpha();

        Vector3 screenPos = _cam.WorldToScreenPoint(_enemy.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform,
            screenPos,
            null,
            out Vector2 canvasPos);

        _rectTransform.anchoredPosition = canvasPos + _offset;
    }

    private void OnDestroy()
    {
        if (_enemy == null)
            return;
        _enemy.OnHealthChange -= SetValue;
    }

    public void InitializeHealthBar(Camera cam, RectTransform canvasRectTransform, EnemyCore enemy, Vector2 offset)
    {
        _cam = cam;
        _canvasRectTransform = canvasRectTransform;
        _enemy = enemy;
        _offset = offset;
        _enemy.OnHealthChange += SetValue;
        _slider.value = 1f;
        _targetValue = 1f;
    }

    public void SetAlpha(float alpha, float fadeDuration)
    {
        _targetAlpha = Mathf.Clamp01(alpha);
        _fadeDuration = Mathf.Max(0f, fadeDuration);

        if (_canvasGroup != null && _fadeDuration <= 0f)
            _canvasGroup.alpha = _targetAlpha;
    }

    private void UpdateAlpha()
    {
        if (_canvasGroup == null)
            return;

        if (_fadeDuration <= 0f)
        {
            _canvasGroup.alpha = _targetAlpha;
            return;
        }

        _canvasGroup.alpha = Mathf.MoveTowards(
            _canvasGroup.alpha,
            _targetAlpha,
            Time.unscaledDeltaTime / _fadeDuration);
    }

    private void SetValue(float maxHP, float currentHP)
    {
        _targetValue = maxHP > 0f
            ? Mathf.Clamp01(currentHP / maxHP)
            : 0f;
    }
}
