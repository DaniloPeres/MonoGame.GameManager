namespace MonoGame.GameManager.StateMachines
{
    /// <summary>
    /// A state implemented as a class (State pattern), for states with their own data and logic.
    /// </summary>
    public interface IState
    {
        void Enter();

        void Update(float deltaSeconds);

        void Exit();
    }
}
