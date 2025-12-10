using UnityEngine;
using UnityEngine.AI;

public class WaypointPatrol : MonoBehaviour
{
	public Transform[] waypoints;

	private NavMeshAgent _navMeshAgent;
	private int _currentWaypointIndex;

	private const float rotSpeed = 20f;
	void Start()
	{
		_navMeshAgent = GetComponent<NavMeshAgent>();
		_navMeshAgent.SetDestination(waypoints[0].position);
		_navMeshAgent.updateRotation = false;
		//_navMeshAgent.updateRotation = ((_navMeshAgent.velocity).magnitude > 0.2);
	}
	void Update()
	{
		if (_navMeshAgent.remainingDistance < _navMeshAgent.stoppingDistance)
		{
			_currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Length;
			_navMeshAgent.SetDestination(waypoints[_currentWaypointIndex].position);
		}
	}

	void FixedUpdate()
	{
		//_navMeshAgent.updateRotation = ((_navMeshAgent.velocity).magnitude > 0.2);
		InstantlyTurn(_navMeshAgent.destination);
	}


	private void InstantlyTurn(Vector3 destination)
	{
		//When on target -> dont rotate!
		if ((destination - transform.position).magnitude < 0.1f) return;

		Vector3 direction = (destination - transform.position).normalized;
		Quaternion qDir = Quaternion.LookRotation(direction);
		transform.rotation = Quaternion.Slerp(transform.rotation, qDir, Time.deltaTime * rotSpeed);
	}
}
