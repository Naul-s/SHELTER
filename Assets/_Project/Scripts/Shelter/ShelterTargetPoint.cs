using System;
using System.Collections.Generic;
using Shelter.Core;
using UnityEngine;

namespace Shelter.ShelterDefense
{
    /// <summary>
    /// 좀비가 "쉘터"를 목적지/공격 대상으로 삼을 때 참조하는 최소 placeholder.
    /// 실제 쉘터 코어/벽 세그먼트 체력 시스템(ShelterCore, ShelterWallSegment)은 shelter-defense 스킬 작업에서
    /// 이 역할을 대체하거나 이 컴포넌트를 부착해 재사용한다.
    /// </summary>
    public class ShelterTargetPoint : MonoBehaviour, IDamageable
    {
        internal static readonly List<ShelterTargetPoint> ActiveInstances = new List<ShelterTargetPoint>();

        [SerializeField, Min(0f)] private float _maxHealth = 300f;

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

            _currentHealth = Mathf.Max(0f, _currentHealth - Mathf.Max(0f, damageInfo.Amount));
            Damaged?.Invoke(damageInfo);

            if (IsDead) Died?.Invoke();
        }
    }
}
