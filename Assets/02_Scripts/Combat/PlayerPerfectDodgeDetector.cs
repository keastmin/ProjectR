using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerPerfectDodgeDetector : MonoBehaviour
{
    [SerializeField] private PlayerCore _player;
    [SerializeField] private EnemyCore[] _enemies;

    private void Awake()
    {
        _player.OnPerfectDodgeCheck += IsPlayerPerfectDodge;
    }

    private EnemyCore IsPlayerPerfectDodge()
    {
        EnemyCore dodgeSource = null;

        List<EnemyCore> sortedEnemies = GetEnemiesByDistance(_player.transform, _enemies);
        foreach (var enemy in sortedEnemies)
        {
            if (enemy != null && enemy.IsPlayerInEnemyAttackRange())
            {
                return enemy;
            }
        }
        return dodgeSource;
    }

    private List<EnemyCore> GetEnemiesByDistance(Transform player, EnemyCore[] enemies)
    {
        return enemies.Where(enemy => enemy != null).OrderBy(enemy => (enemy.transform.position - player.position).sqrMagnitude).ToList();
    }
}