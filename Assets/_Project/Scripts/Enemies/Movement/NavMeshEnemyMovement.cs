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

        public void SetStoppingDistance(float stoppingDistance)
        {
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
            _agent.updatePosition = true;
            _agent.updateRotation = true;
            _agent.Warp(worldPosition);
            if (_agent.isOnNavMesh) _agent.isStopped = false;
        }

        public void BeginTraversal()
        {
            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }

            _agent.updatePosition = false;
            _agent.updateRotation = false;
        }

        public void EndTraversal(Vector3 endPosition)
        {
            Warp(endPosition);
        }
    }
}
