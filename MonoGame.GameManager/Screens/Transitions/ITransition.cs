using System;

namespace MonoGame.GameManager.Screens.Transitions
{
    /// <summary>
    /// An animated transition between screens. The screen manager calls <see cref="TransitionOut"/>, replaces the
    /// screens when it completes, and then calls <see cref="TransitionIn"/>.
    /// </summary>
    public interface ITransition
    {
        /// <summary>Hides the current screen (eg: fades to black) and invokes <paramref name="onComplete"/>.</summary>
        void TransitionOut(Action onComplete);

        /// <summary>Reveals the new screen and invokes <paramref name="onComplete"/>.</summary>
        void TransitionIn(Action onComplete);
    }
}
