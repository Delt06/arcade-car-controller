using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class CenterOfMassOverride : MonoBehaviour
{
	[SerializeField] private Transform _centerOfMass = default;

	private void Awake()
	{
		var body = GetComponent<Rigidbody>();
		body.centerOfMass = body.transform.InverseTransformPoint(_centerOfMass.position);
	}
}