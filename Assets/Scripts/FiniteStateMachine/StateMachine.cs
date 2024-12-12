using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
	private State _currentState;
	private List<State> _listOfStates;

	private void Awake()
	{

	}

	private void Update()
	{
		_currentState.StateUpdate();
	}

	private void FixedUpdate()
	{
		_currentState.StateFixedUpdate();
	}

	private void TransitionState(State newState)
	{
		if (newState == _currentState) return;

		_currentState.OnStateExit();
		_currentState = newState;
		_currentState.OnStateEnter();
	}
}
