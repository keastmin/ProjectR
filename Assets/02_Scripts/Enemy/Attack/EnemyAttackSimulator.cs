using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackSimulator : MonoBehaviour
{
    [SerializeField] private EnemyAttackSO[] _attackSOs;

    private Dictionary<EnemyAttackID, EnemyAttackSO> _hitboxInfos;

    private void Awake()
    {
        _hitboxInfos = new Dictionary<EnemyAttackID, EnemyAttackSO>();
        foreach(var so in _attackSOs)
        {
            _hitboxInfos.Add(so.AttackID, so);
        }
    }

    public void AttackRangeSimulation(EnemyAttackID id, Quaternion rotation)
    {
        if (!_hitboxInfos.ContainsKey(id))
            return;

        EnemyAttackSO so = _hitboxInfos[id];
        Quaternion rot = rotation;
    }
}