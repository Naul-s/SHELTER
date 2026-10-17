using UnityEngine;

namespace Shelter.Enemies.Movement
{
    /// <summary>이동 방식을 추상화 — 현재는 NavMesh 기반이지만, 비행형 등 다른 이동을 추가해도 EnemyBase는 바뀌지 않는다.</summary>
    public interface IEnemyMovement
    {
        float CurrentSpeed { get; }

        void Initialize(float moveSpeed, float stoppingDistance);
        void SetStoppingDistance(float stoppingDistance);
        void SetDestination(Vector3 worldPosition);
        void Stop();
        void Resume();
        void Warp(Vector3 worldPosition);

        /// <summary>창문 넘기 등 스크립트가 직접 위치를 옮기는 구간의 시작 — 경로 이동이 위치를 덮어쓰지 않게 한다.</summary>
        void BeginTraversal();
        /// <summary>스크립트 이동 구간의 끝 — endPosition에서 경로 이동을 재개한다.</summary>
        void EndTraversal(Vector3 endPosition);
    }
}
