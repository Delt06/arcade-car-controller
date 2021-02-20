using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarStabilizer : MonoBehaviour
{
	[SerializeField] private CarController _carController;
	[Min(0f)] public float Stability = 0.3f;
	[Min(0f)] public float Speed = 2.0f;

	private void FixedUpdate()
	{
		var angularVelocity = Rigidbody.angularVelocity;
		var predictedUp = Quaternion.AngleAxis(
			angularVelocity.magnitude * Mathf.Rad2Deg * Stability / Speed,
			angularVelocity
		) * transform.up;
		var torqueVector = Vector3.Cross(predictedUp, Vector3.up);
		torqueVector *= Speed * Speed;
		torqueVector *= _carController.TractionCoefficient;
		Rigidbody.AddTorque(torqueVector);
	}

	private Rigidbody Rigidbody => _carController.Rigidbody;
}