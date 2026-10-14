using System;
using System.Collections.Generic;
using Shelter.Core;
using UnityEngine;

namespace Shelter.Player
{
    /// <summary>
    /// 플레이어 체력. 기획상 플레이어 스탯은 체력과 공격력(<see cref="PlayerAttackStats"/>)뿐이라 방어력은 없다 —
    /// 좀비 공격 피해가 그대로 체력을 깎는다. 사망 후 흐름(부활 등)은 player-state 스킬 작업에서 이 컴포넌트를 확장한다.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        /// <summary>Update 루프에서 FindObjectOfType 없이 근처 플레이어를 찾기 위한 활성 인스턴스 registry.</summary>
        internal static readonly List<PlayerHealth> ActiveInstances = new List<PlayerHealth>();

        [SerializeField, Min(1f)] private float _maxHealth = 1000f;

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

            _currentHealth = Mathf.Max(0f, _currentHealth - damageInfo.Amount);
            Damaged?.Invoke(damageInfo);

            if (IsDead) Died?.Invoke();
        }
    }
}
