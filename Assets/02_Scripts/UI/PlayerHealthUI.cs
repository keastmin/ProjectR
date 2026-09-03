using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private PlayerCore _player;
    [SerializeField] private float _lerpSpeed = 10f;

    private float _targetValue = 0f;

    private void Awake()
    {
        _healthSlider.value = 1f;
        _targetValue = 1f;
        _player.OnHealthChange += SetValue;
    }

    private void Update()
    {
        _healthSlider.value = Mathf.Lerp(_healthSlider.value, _targetValue, Time.deltaTime * _lerpSpeed);
    }

    private void SetValue(float maxHP, float currHP)
    {
        _targetValue = Mathf.Clamp(currHP / maxHP, 0f, 1f);
    }
}