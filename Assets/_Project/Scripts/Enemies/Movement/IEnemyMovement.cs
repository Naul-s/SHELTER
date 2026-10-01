using UnityEngine;

namespace Shelter.Enemies.Movement
{
    /// <summary>이동 방식을 추상화 — 현재는 NavMesh 기반이지만, 비행형 등 다른 이동을 추가해도 EnemyBase는 바뀌지 않는다.</summary>
    public interface IEnemyMovement
    {
        float CurrentSpeed { get; }

        void Initialize(float moveSpeed, float stoppingDistance);
        void SetDestination(Vector3 worldPosition);
        void Stop();
        void Resume();
        void Warp(Vector3 worldPosition);
    }
}
