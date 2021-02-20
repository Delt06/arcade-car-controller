using System;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarInput : MonoBehaviour
{
	private void Update()
	{
		_carController.Steering = Input.GetAxisRaw("Horizontal");
		_carController.Throttle = Input.GetAxisRaw("Vertical");
		_carController.Break = Input.GetButton("Jump");
	}

	private void Awake()
	{
		_carController = GetComponent<CarController>();
	}

	private CarController _carController;
}