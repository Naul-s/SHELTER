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

        [Header("공격 (판자 뜯기와 플레이어 공격 공용)")]
        [Tooltip("플레이어에게 주는 피해. 판자에는 공격 1회 = 타격 1회로 적용되고 이 값은 쓰지 않는다.")]
        [SerializeField, Min(0f)] private float _attackPower = 20f;
        [Tooltip("플레이어 공격 사거리(수평 거리).")]
        [SerializeField, Min(0.01f)] private float _attackRange = 1.2f;
        [Tooltip("팔이 닿는 순간부터 다음 공격을 시작할 수 있을 때까지(초).")]
        [SerializeField, Min(0.01f)] private float _attackCooldown = 1.3f;
        [Tooltip("공격 동작 시작부터 팔이 실제로 닿는 순간까지(초) — 공격 애니메이션의 내려치는 시점에 맞춘다. " +
                 "이 순간의 거리·방향으로 다시 판정하므로 그사이 물러나거나 비켜서면 빗나간다.")]
        [SerializeField, Min(0f)] private float _attackWindupTime = 0.9f;
        [Tooltip("내려친 뒤 자세를 회복하는 동안 제자리에 서 있는 시간(초).")]
        [SerializeField, Min(0f)] private float _attackRecoveryTime = 0.85f;

        [Header("피격 반응")]
        [Tooltip("움찔하는 피격 애니메이션을 다시 재생하기까지의 최소 간격(초). 연사에 맞아도 매 발마다 굳지 않게 한다.")]
        [SerializeField, Min(0f)] private float _hitReactionCooldown = 0.6f;
        [Tooltip("피격 반응 동안 멈춰 서는 시간(초). 내려치기 전에 맞으면 공격이 끊긴다. 0이면 경직 없음(중량급) — 공격 중엔 반응 동작도 생략.")]
        [SerializeField, Min(0f)] private float _hitStaggerDuration = 0.45f;

        [Header("쉘터 진입")]
        [Tooltip("판자가 다 뜯긴 창문을 뛰어넘는 데 걸리는 시간(초). 출입구는 이동 속도로 걸어서 통과한다.")]
        [SerializeField, Min(0.1f)] private float _windowVaultDuration = 1.3f;

        [Header("타겟팅")]
        [Tooltip("쉘터 안에서 가장 가까운 플레이어를 다시 고르는 주기(초).")]
        [SerializeField, Min(0.05f)] private float _targetReevaluateInterval = 0.5f;

        [Header("사망")]
        [Tooltip("사망 후 시체가 사라지기(풀 반환)까지의 시간.")]
        [SerializeField, Min(0f)] private float _corpseLifetime = 8f;

        [Header("보상")]
        [Tooltip("처치(막타)한 플레이어가 받는 돈. 플레이테스트 후 조정 대상.")]
        [SerializeField, Min(0)] private int _killReward = 60;

        public string EnemyId => _enemyId;
        public string DisplayName => _displayName;

        public float MaxHealth => _maxHealth;
        public float Defense => _defense;

        public float MoveSpeed => _moveSpeed;

        public float AttackPower => _attackPower;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public float AttackWindupTime => _attackWindupTime;
        public float AttackRecoveryTime => _attackRecoveryTime;

        public float HitReactionCooldown => _hitReactionCooldown;
        public float HitStaggerDuration => _hitStaggerDuration;

        public float WindowVaultDuration => _windowVaultDuration;

        public float TargetReevaluateInterval => _targetReevaluateInterval;

        public float CorpseLifetime => _corpseLifetime;

        public int KillReward => _killReward;
    }
}
