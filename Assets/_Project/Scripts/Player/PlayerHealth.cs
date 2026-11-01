using System;
using System.Collections.Generic;
using Shelter.Core;
using UnityEngine;

namespace Shelter.Player
{
    /// <summary>
    /// 플레이어 체력. 기획상 플레이어 스탯은 체력과 공격력(<see cref="PlayerAttackStats"/>)뿐이라 방어력은 없다 —
    /// 좀비 공격 피해가 그대로 체력을 깎는다. 사망하면 <see cref="PlayerParty"/>가 전원 사망(패배) 여부를 판정한다.
    /// 사망 후 흐름(부활 등)은 미확정 — player-state 스킬 작업에서 이 컴포넌트를 확장한다.
    /// 체력은 공유 상태 — 바꾸는 곳은 <see cref="TakeDamage"/>와 <see cref="Heal"/> 두 곳뿐이고, 네트워킹 도입 시 이 둘이 서버 진입점이 된다.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        /// <summary>Update 루프에서 FindObjectOfType 없이 근처 플레이어를 찾기 위한 활성 인스턴스 registry.</summary>
        internal static readonly List<PlayerHealth> ActiveInstances = new List<PlayerHealth>();

        [SerializeField, Min(1f)] private float _maxHealth = 1000f;
        [Tooltip("좀비가 쫓아가고 공격하는 지점. VR 리그에서는 머리(HMD 카메라)를 넣어 룸스케일로 걸어도 실제 위치를 따라가게 한다. 비우면 이 오브젝트.")]
        [SerializeField] private Transform _targetPoint;

        private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsDead => _currentHealth <= 0f;
        public bool IsDamaged => !IsDead && _currentHealth < _maxHealth;
        public float Health01 => _currentHealth / _maxHealth;
        public Transform TargetPoint => _targetPoint != null ? _targetPoint : transform;

        public event Action<DamageInfo> Damaged;
        /// <summary>실제로 회복된 양(최대치에서 잘린 뒤).</summary>
        public event Action<float> Healed;
        public event Action Died;
        /// <summary>피격·회복 등 체력 값이 바뀔 때마다(표시용).</summary>
        public event Action HealthChanged;

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
            HealthChanged?.Invoke();

            if (!IsDead) return;

            Died?.Invoke();
            PlayerParty.NotifyPlayerDied(this);
        }

        /// <summary>최대 체력을 넘지 않게 회복한다. 사망한 플레이어는 회복하지 않는다(부활은 별도 규칙). 실제 회복량을 돌려준다.</summary>
        public float Heal(float amount)
        {
            if (IsDead || amount <= 0f) return 0f;

            float before = _currentHealth;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            float healed = _currentHealth - before;
            if (healed <= 0f) return 0f;

            Healed?.Invoke(healed);
            HealthChanged?.Invoke();
            return healed;
        }
    }
}
