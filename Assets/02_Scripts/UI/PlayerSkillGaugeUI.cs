using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillGaugeUI : MonoBehaviour
{
    [SerializeField] private PlayerCore _player;
    [SerializeField] private Slider _slider;
    [SerializeField] private float _sliderLerpSpeed = 10f;

    private float _targetValue = 0f;

    private void Awake()
    {
        _player.OnSkillGaugeChange += SetSkillGauge;
        _targetValue = 0f;
        _slider.value = _targetValue;
    }

    private void Update()
    {
        if (_player == null)
            return;

        _slider.value = Mathf.Lerp(_slider.value, _targetValue, Time.deltaTime * _sliderLerpSpeed);
    }

    private void SetSkillGauge(float maxGauge, float currGauge)
    {
        _targetValue = Mathf.Clamp(currGauge / maxGauge, 0f, 1f);
    }
}