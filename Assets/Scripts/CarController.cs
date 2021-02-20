using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
	[SerializeField] private Rigidbody _rigidbody = default;
	[SerializeField] private Wheel[] _wheels = default;
	[SerializeField] private float _suspensionDistance = 0.1f;
	[SerializeField] private LayerMask _groundLayer = default;
	[SerializeField, Min(0f)] private float _maxAcceleration = 10f;
	[SerializeField, Range(0f, 1f)] private float _reverseAccelerationCoefficient = 0.5f;
	[SerializeField, Min(0f)] private float _maxTorque = 10f;
	[SerializeField] private float _sidewaysFriction = 1f;
	[SerializeField] private AnimationCurve _forwardSpeedToSteeringCoefficient = AnimationCurve.Linear(0f, 0f, 5f, 1f);
	[SerializeField, Min(0f)] private float _breaksFriction = 10f;

	public float MaxAcceleration => _maxAcceleration;

	public IReadOnlyList<Wheel> Wheels => _wheels;

	public Rigidbody Rigidbody => _rigidbody;
	public float TractionCoefficient { get; private set; }

	public float Throttle
	{
		get => _throttle;
		set => _throttle = Mathf.Clamp(value, -1f, 1f);
	}

	public float Steering
	{
		get => _steering;
		set => _steering = Mathf.Clamp(value, -1f, 1f);
	}

	public bool Break { get; set; }

	private void FixedUpdate()
	{
		RecalculateTraction();
		ApplyThrottle();
		ApplySteering();
		ApplyBreaks();
		ApplySidewaysFriction();
	}

	private void RecalculateTraction()
	{
		var numberOfGroundedWheels = 0;

		for (var index = 0; index < _wheels.Length; index++)
		{
			var wheel = _wheels[index];
			var ray = new Ray(wheel.Physics.position, SuspensionCheckDirection);
			wheel.IsGrounded = Physics.Raycast(ray, _suspensionDistance, _groundLayer, QueryTriggerInteraction.Ignore);
			if (wheel.IsGrounded)
				numberOfGroundedWheels++;

			_wheels[index] = wheel;
		}

		TractionCoefficient = (float) numberOfGroundedWheels / _wheels.Length;
	}

	private Vector3 SuspensionCheckDirection => -_rigidbody.transform.up;

	private void ApplyThrottle()
	{
		if (Break) return;

		var acceleration = Throttle * TractionCoefficient * _maxAcceleration;

		if (Throttle < 0f)
			acceleration *= _reverseAccelerationCoefficient;

		var accelerationVector = Forward * acceleration;
		_rigidbody.AddForce(accelerationVector, ForceMode.Acceleration);
	}

	private Vector3 Forward => _rigidbodyTransform.forward;

	private void ApplySteering()
	{
		var localForwardVelocity = _rigidbodyTransform.InverseTransformVector(_rigidbody.velocity);
		var forwardSpeed = localForwardVelocity.z;
		var coefficient = _forwardSpeedToSteeringCoefficient.Evaluate(Mathf.Abs(forwardSpeed));
		var steeringTorque = coefficient * Steering * TractionCoefficient * _maxTorque;

		if (forwardSpeed < 0f)
			steeringTorque *= -1f;

		var steeringTorqueVector = Up * steeringTorque;
		_rigidbody.AddTorque(steeringTorqueVector, ForceMode.Acceleration);
	}

	private Vector3 Up => _rigidbodyTransform.up;

	private void ApplyBreaks()
	{
		if (!Break) return;

		var localForward = _rigidbodyTransform.InverseTransformVector(_rigidbody.velocity);
		localForward.x = localForward.y = 0f;
		var againstForwardDirection = _rigidbodyTransform.TransformDirection(-localForward.normalized);
		var forwardSpeed = localForward.magnitude;
		var breakAcceleration = forwardSpeed * _breaksFriction * TractionCoefficient * againstForwardDirection;
		_rigidbody.AddForce(breakAcceleration, ForceMode.Acceleration);
	}

	private void ApplySidewaysFriction()
	{
		var localSideVelocity = _rigidbodyTransform.InverseTransformVector(_rigidbody.velocity);
		localSideVelocity.y = localSideVelocity.z = 0f;

		var worldSideVelocity = _rigidbodyTransform.TransformVector(localSideVelocity);
		var sideSqrSpeed = worldSideVelocity.sqrMagnitude;
		var againstSideVelocity = -worldSideVelocity.normalized;
		var friction = TractionCoefficient * _sidewaysFriction * sideSqrSpeed * againstSideVelocity;
		_rigidbody.AddForce(friction, ForceMode.Acceleration);
	}

	private void Awake()
	{
		_rigidbodyTransform = _rigidbody.transform;
	}

	private float _throttle;
	private float _steering;
	private Transform _rigidbodyTransform;

	private void OnDrawGizmos()
	{
		if (_wheels == null) return;
		if (!_rigidbody) return;

		Gizmos.color = Color.red;

		foreach (var wheel in _wheels)
		{
			if (!wheel.Physics) continue;

			var center = wheel.Physics.position;
			Gizmos.DrawSphere(center, 0.35f);
			Gizmos.DrawRay(center, SuspensionCheckDirection * _suspensionDistance);
		}
	}

	private void OnValidate()
	{
		if (!_rigidbody)
			_rigidbody = GetComponent<Rigidbody>();
	}
}