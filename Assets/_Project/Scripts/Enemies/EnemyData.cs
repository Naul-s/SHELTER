using UnityEngine;

namespace Shelter.Enemies
{
    /// <summary>
    /// 좀비 한 종류의 밸런싱 수치를 전부 담는 ScriptableObject. 코드에 수치를 하드코딩하지 않기 위한
    /// 유일한 값 출처 — 같은 EnemyBase/전략 컴포넌트 조합이라도 이 SO만 바꾸면 다른 개체가 된다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shelter/Enemy/Enemy Data", fileName = "EnemyData_")]
    public class EnemyData : ScriptableObject
    {
        [Header("정체성")]
        [SerializeField] private string _enemyId;
        [SerializeField] private string _displayName;

        [Header("체력/방어")]
        [SerializeField, Min(0f)] private float _maxHealth = 100f;
        [SerializeField, Min(0f)] private float _defense = 5f;

        [Header("이동")]
        [SerializeField, Min(0f)] private float _moveSpeed = 2f;

        [Header("공격")]
        [SerializeField, Min(0f)] private float _attackPower = 20f;
        [SerializeField, Min(0.01f)] private float _attackRange = 1.2f;
        [SerializeField, Min(0.01f)] private float _attackCooldown = 1.8f;
        [SerializeField, Min(0f)] private float _attackWindupTime = 0.4f;

        [Header("타겟팅 — 쉘터 vs 플레이어 가중치")]
        [Tooltip("이 반경 안에 플레이어가 있어야 플레이어를 타겟 후보로 고려한다.")]
        [SerializeField, Min(0f)] private float _aggroRadius = 8f;
        [Tooltip("플레이어가 사거리 안에 있을 때 쉘터 대신 플레이어를 고를 확률(0=무조건 쉘터, 1=무조건 플레이어).")]
        [SerializeField, Range(0f, 1f)] private float _playerPreferenceWeight = 0.5f;
        [Tooltip("플레이어를 쫓는 중 이 반경을 벗어나면 시야를 잃은 것으로 간주해 시간 카운트를 시작한다.")]
        [SerializeField, Min(0f)] private float _loseTargetRadius = 12f;
        [Tooltip("플레이어 시야를 잃은 채 이 시간이 지나면 타겟을 쉘터로 복귀시킨다.")]
        [SerializeField, Min(0f)] private float _loseTargetTime = 5f;
        [Tooltip("피격 시 공격한 대상으로 타겟을 강제 전환할지 여부 (러너/스피터형에 사용, 워커/비스트는 false).")]
        [SerializeField] private bool _switchTargetOnHit;
        [SerializeField, Min(0.05f)] private float _targetReevaluateInterval = 0.5f;

        [Header("사망")]
        [Tooltip("사망 후 아이템 드랍/시체가 사라지기까지의 시간.")]
        [SerializeField, Min(0f)] private float _corpseLifetime = 8f;

        public string EnemyId => _enemyId;
        public string DisplayName => _displayName;

        public float MaxHealth => _maxHealth;
        public float Defense => _defense;

        public float MoveSpeed => _moveSpeed;

        public float AttackPower => _attackPower;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public float AttackWindupTime => _attackWindupTime;

        public float AggroRadius => _aggroRadius;
        public float PlayerPreferenceWeight => _playerPreferenceWeight;
        public float LoseTargetRadius => _loseTargetRadius;
        public float LoseTargetTime => _loseTargetTime;
        public bool SwitchTargetOnHit => _switchTargetOnHit;
        public float TargetReevaluateInterval => _targetReevaluateInterval;

        public float CorpseLifetime => _corpseLifetime;
    }
}
