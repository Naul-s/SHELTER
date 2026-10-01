using UnityEngine;
using UnityEngine.AI;

namespace Shelter.Enemies.Movement
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshEnemyMovement : MonoBehaviour, IEnemyMovement
    {
        private NavMeshAgent _agent;

        public float CurrentSpeed => _agent.velocity.magnitude;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void Initialize(float moveSpeed, float stoppingDistance)
        {
            _agent.speed = moveSpeed;
            _agent.stoppingDistance = stoppingDistance;
        }

        public void SetDestination(Vector3 worldPosition)
        {
            if (_agent.isOnNavMesh) _agent.SetDestination(worldPosition);
        }

        public void Stop()
        {
            if (_agent.isOnNavMesh) _agent.isStopped = true;
        }

        public void Resume()
        {
            if (_agent.isOnNavMesh) _agent.isStopped = false;
        }

        public void Warp(Vector3 worldPosition)
        {
            _agent.Warp(worldPosition);
            _agent.isStopped = false;
        }
    }
}
