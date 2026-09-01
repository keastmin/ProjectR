using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerPerfectDodgeDetector : MonoBehaviour
{
    [SerializeField] private PlayerCore _player;
    [SerializeField] private EnemyCore[] _enemies;

    private HashSet<EnemyCore> _enemyHash;

    private void Awake()
    {
        _enemyHash = new HashSet<EnemyCore>();
        AddEnemy(_enemies);
        EnemyCore[] sceneEnemies = FindObjectsByType<EnemyCore>();
        AddEnemy(sceneEnemies);
    }

    private void OnEnable()
    {
        if (_player != null)
            _player.OnPerfectDodgeCheck += IsPlayerPerfectDodge;
    }

    private void OnDisable()
    {
        if (_player != null)
            _player.OnPerfectDodgeCheck -= IsPlayerPerfectDodge;
    }

    private EnemyCore IsPlayerPerfectDodge()
    {
        EnemyCore dodgeSource = null;

        List<EnemyCore> sortedEnemies = GetEnemiesByDistance(_player.transform, _enemyHash);
        foreach (var enemy in sortedEnemies)
        {
            if (enemy != null && enemy.isActiveAndEnabled && enemy.CurrentHP > 0f && enemy.IsPlayerInEnemyAttackRange())
            {
                return enemy;
            }
        }
        return dodgeSource;
    }

    private List<EnemyCore> GetEnemiesByDistance(Transform player, HashSet<EnemyCore> enemies)
    {
        return enemies.Where(enemy => enemy != null).OrderBy(enemy => (enemy.transform.position - player.position).sqrMagnitude).ToList();
    }

    private void AddEnemy(EnemyCore[] enemies)
    {
        foreach(var enemy in enemies)
        {
            if (enemy != null)
                _enemyHash.Add(enemy);
        }
    }
}
