using System;

namespace MonoGame.GameManager.Screens.Transitions
{
    public interface ITransition
    {
        void CreateTransitionIn(Action onComplete);
        void CreateTransitionOut();
    }
}
