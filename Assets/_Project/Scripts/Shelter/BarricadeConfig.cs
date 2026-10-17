using UnityEngine;

namespace Shelter.ShelterDefense
{
    /// <summary>바리케이드 밸런싱·연출 수치. 판자 수는 씬에 배치된 판자 오브젝트 수로 정해진다.</summary>
    [CreateAssetMenu(menuName = "Shelter/Shelter/Barricade Config", fileName = "BarricadeConfig")]
    public class BarricadeConfig : ScriptableObject
    {
        [Tooltip("판자 한 장이 떨어지기까지 필요한 좀비 공격 횟수. Nacht der Untoten 기준 1.")]
        [SerializeField, Min(1)] private int _hitsPerBoard = 1;

        [Header("판자 파괴 연출")]
        [Tooltip("떨어진 판자가 날아가다 사라지기까지의 시간(초).")]
        [SerializeField, Min(0.05f)] private float _boardFlyTime = 0.8f;
        [Tooltip("떨어진 판자가 바깥(좀비 쪽)으로 튕겨 나가는 초기 속도(m/s).")]
        [SerializeField, Min(0f)] private float _boardFlySpeed = 2.5f;
        [Tooltip("판자 파괴/타격 효과음 볼륨.")]
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;

        public int HitsPerBoard => _hitsPerBoard;
        public float BoardFlyTime => _boardFlyTime;
        public float BoardFlySpeed => _boardFlySpeed;
        public float SfxVolume => _sfxVolume;
    }
}
