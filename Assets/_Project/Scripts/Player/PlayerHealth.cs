using System;
using System.Collections.Generic;
using Shelter.Core;
using UnityEngine;

namespace Shelter.Player
{
    /// <summary>
    /// 플레이어 체력의 최소 구현. 좀비 공격이 실제로 받아칠 대상이 필요해 우선 만든 것으로,
    /// 사망/30초 후 쉘터 부활/자원 드랍 등 전체 생사 흐름은 player-state 스킬 작업에서 이 컴포넌트를 확장한다.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        /// <summary>Update 루프에서 FindObjectOfType 없이 근처 플레이어를 찾기 위한 활성 인스턴스 registry.</summary>
        internal static readonly List<PlayerHealth> ActiveInstances = new List<PlayerHealth>();

        [SerializeField, Min(0f)] private float _maxHealth = 1000f;
        [SerializeField, Min(0f)] private float _defense = 50f;

        private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsDead => _currentHealth <= 0f;

        public event Action<DamageInfo> Damaged;
        public event Action Died;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        private void OnEnable()
        {
            ActiveInstances.Add(this);
        }

        private void OnDisable()
        {
            ActiveInstances.Remove(this);
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (IsDead) return;

            float mitigated = CombatMath.ApplyDefense(damageInfo.Amount, _defense);
            _currentHealth = Mathf.Max(0f, _currentHealth - mitigated);
            Damaged?.Invoke(damageInfo);

            if (IsDead) Died?.Invoke();
        }
    }
}
