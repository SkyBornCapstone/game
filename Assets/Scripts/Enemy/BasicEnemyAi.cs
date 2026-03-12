using player;
using PurrNet;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Enemy
{
    public class BasicEnemyAi : NetworkBehaviour
    {
        public NavMeshAgent agent;
        public LayerMask groundMask, playerMask;

        public Vector3 walkPoint;
        private bool _walkPointSet;
        public float walkPointRange;

        public float attackDamage;
        public float timeBetweenAttacks;
        private float _nextAttack = 0;

        public float sightRange, attackRange;
        private bool _playerInSightRange, _playerInAttackRange;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            _playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerMask);
            _playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerMask);

            PlayerHealth health = null;
            if (_playerInSightRange)
            {
                int maxColliders = 4;
                Collider[] hitColliders = new Collider[maxColliders];
                var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, hitColliders, playerMask);
                for (int i = 0; i < size; i++)
                {
                    hitColliders[i].gameObject.TryGetComponent(out health);
                }
            }

            if (!_playerInSightRange && !_playerInAttackRange) Patrolling();
            if (_playerInSightRange && !_playerInAttackRange) ChasePlayer(health);
            if (_playerInSightRange && _playerInAttackRange) AttackPlayer(health);
        }

        private void Patrolling()
        {
            if (!_walkPointSet) SearchWalkPoint();

            if (_walkPointSet) agent.SetDestination(walkPoint);

            Vector3 dist = transform.position - walkPoint;

            if (dist.magnitude < 1f) _walkPointSet = false;
        }

        private void SearchWalkPoint()
        {
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y,
                transform.position.z + randomZ);

            if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask))
                _walkPointSet = true;
        }

        private void ChasePlayer(PlayerHealth health)
        {
            agent.SetDestination(health.transform.position);
        }

        private void AttackPlayer(PlayerHealth health)
        {
            agent.SetDestination(transform.position);
            transform.LookAt(health.transform);
            if (Time.time > _nextAttack)
            {
                _nextAttack = Time.time + timeBetweenAttacks;
                health.TakeDamage(attackDamage);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }
    }
}