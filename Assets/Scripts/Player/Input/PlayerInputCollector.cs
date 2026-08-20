using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputCollector : MonoBehaviour
{
    private string _moveName = "Move";
    private string _attackName = "Attack";

    private PlayerInput _playerInput;

    private InputAction _moveAction;
    private InputAction _attackAction;

    private Vector2 _moveValue = Vector2.zero;
    private bool _attackValue = false;

    // 프로퍼티
    public Vector2 MoveValue => _moveValue;
    public bool IsInputMove => MoveValue.sqrMagnitude >= 0.001f;
    public bool IsInputAttack => _attackValue;

    private void Awake()
    {
        TryGetComponent(out _playerInput);
        _moveAction = _playerInput.actions[_moveName];
        _attackAction = _playerInput.actions[_attackName];
    }

    private void Update()
    {
        // 움직임 입력 감지
        _moveValue = _moveAction.ReadValue<Vector2>();

        // 공격 입력 감지
        _attackValue = _attackAction.WasPressedThisFrame();
    }
}