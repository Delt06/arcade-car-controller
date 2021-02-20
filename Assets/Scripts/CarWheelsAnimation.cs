using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarWheelsAnimation : MonoBehaviour
{
	[SerializeField, Min(0f)] private float _radius = 1f;
	[SerializeField, Min(0f)] private float _maxSteeringAngle = 45f;
	[SerializeField, Min(0f)] private float _steeringDamping = 1f;
	[SerializeField, Min(0f)] private float _maxSteeringChangeSpeed = 2;

	private void Update()
	{
		var deltaTime = Time.deltaTime;
		UpdateSteeringDamping(deltaTime);
		UpdateRotation(deltaTime);
	}

	private void UpdateSteeringDamping(float deltaTime)
	{
		_steering = Mathf.SmoothDamp(_steering, _carController.Steering, ref _steeringChangeSpeed, _steeringDamping,
			_maxSteeringChangeSpeed, deltaTime
		);
	}

	private void UpdateRotation(float deltaTime)
	{
		for (var index = 0; index < _carController.Wheels.Count; index++)
		{
			var wheel = _carController.Wheels[index];
			var angularSpeed = GetAngularSpeed(wheel);
			var deltaThrottleAngle = angularSpeed * deltaTime;

			_throttleAngles[index] += deltaThrottleAngle;
			var steeringAngle = wheel.Steering ? _maxSteeringAngle * _steering : 0f;

			var carTransform = _carController.Rigidbody.transform;
			var carUp = carTransform.up;
			var carRight = carTransform.right;
			var initialWorldRotation = carTransform.rotation * _initialRelativeRotation[index];
			var throttleRotationComponent = Quaternion.AngleAxis(_throttleAngles[index], carRight);
			var steeringRotationComponent = Quaternion.AngleAxis(steeringAngle, carUp);
			var combinedRotation = steeringRotationComponent * throttleRotationComponent * initialWorldRotation;
			wheel.Visuals.rotation = combinedRotation;
		}
	}

	private float GetAngularSpeed(in Wheel wheel)
	{
		if (_carController.Break) return 0f;

		float linearSpeed;

		if (!wheel.IsGrounded)
			linearSpeed = _carController.MaxAcceleration * _carController.Throttle;
		else
			linearSpeed = ForwardSpeed;

		var wheelCircleLength = 2f * Mathf.PI * _radius;
		var spinsPerSecond = linearSpeed / wheelCircleLength;
		var angularSpeed = spinsPerSecond * 360f;
		return angularSpeed;
	}

	private float ForwardSpeed
	{
		get
		{
			var localVelocity =
				_carController.Rigidbody.transform.InverseTransformVector(_carController.Rigidbody.velocity);
			return localVelocity.z;
		}
	}

	private void Awake()
	{
		_carController = GetComponent<CarController>();
		var wheels = _carController.Wheels;
		_throttleAngles = new float[wheels.Count];
		_initialRelativeRotation = new Quaternion[wheels.Count];

		var worldToCarLocalRotation = Quaternion.Inverse(_carController.Rigidbody.transform.rotation);

		for (var index = 0; index < _initialRelativeRotation.Length; index++)
		{
			_initialRelativeRotation[index] = worldToCarLocalRotation * wheels[index].Visuals.rotation;
		}
	}

	private float _steering;
	private float _steeringChangeSpeed;
	private CarController _carController;
	private float[] _throttleAngles;
	private Quaternion[] _initialRelativeRotation;

	private void OnDrawGizmos()
	{
		var carController = Application.isPlaying ? _carController : GetComponent<CarController>();
		if (carController == null) return;
		if (carController.Wheels == null) return;

		Gizmos.color = Color.red;

		foreach (var wheel in carController.Wheels)
		{
			if (!wheel.Visuals) continue;

			var center = wheel.Visuals.position;
			Gizmos.DrawWireSphere(center, _radius);
		}
	}
}