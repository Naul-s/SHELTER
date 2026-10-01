using UnityEngine;

namespace Shelter.Core
{
    /// <summary>단일 피해 이벤트를 표현하는 값. 새 피해 유형(원거리/DOT 등) 추가 시 필드만 확장하면 되도록 구조체로 분리.</summary>
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly GameObject Instigator;
        public readonly Vector3 HitPoint;

        public DamageInfo(float amount, GameObject instigator, Vector3 hitPoint = default)
        {
            Amount = amount;
            Instigator = instigator;
            HitPoint = hitPoint;
        }
    }
}
