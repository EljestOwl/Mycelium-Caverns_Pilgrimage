public abstract class State
{
	private StateMachine _stateMachine;

	public abstract void OnStateEnter();
	public abstract void StateUpdate();
	public abstract void StateFixedUpdate();
	public abstract void OnStateExit();
}
