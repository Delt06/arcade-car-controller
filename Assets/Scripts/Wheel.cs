using System;
using UnityEngine;

[Serializable]
public struct Wheel
{
	public Transform Visuals;
	public Transform Physics;
	[NonSerialized] public bool IsGrounded;

	public bool Steering;
}