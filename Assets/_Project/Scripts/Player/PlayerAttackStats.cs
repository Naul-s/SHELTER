using UnityEngine;

namespace Shelter.Player
{
    /// <summary>플레이어의 유일한 공격 스탯. 무기가 타격 시 자기 기본 데미지에 이 값을 더해 피해량을 정한다.</summary>
    public class PlayerAttackStats : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _attackPower = 10f;

        public float AttackPower => _attackPower;
    }
}
