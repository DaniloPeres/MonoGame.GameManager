using MonoGame.GameManager.Screens.Transitions;
using System.Collections.Generic;

namespace MonoGame.GameManager.Screens
{
    /// <summary>
    /// Opens and closes screens. Every request is applied at the end of the current frame, one after the other,
    /// so it is safe to navigate from an event handler or during a transition.
    /// </summary>
    public interface IScreenNavigator
    {
        /// <summary>The screen on top of the stack (the one that receives the input), or null.</summary>
        Screen CurrentScreen { get; }

        /// <summary>The open screens, from the bottom to the top.</summary>
        IReadOnlyList<Screen> Screens { get; }

        /// <summary>True while a navigation (with its transition) is running.</summary>
        bool IsTransitioning { get; }

        /// <summary>Closes every open screen and opens <paramref name="screen"/> with the default transition.</summary>
        void ChangeScreen(Screen screen);

        /// <summary>Closes every open screen and opens <paramref name="screen"/> with the given transition (null = none).</summary>
        void ChangeScreen(Screen screen, ITransition transition);

        void ChangeScreenWithNoTransition(Screen screen);

        /// <summary>Opens <paramref name="screen"/> on top of the current one (eg: a pause menu or a dialog).</summary>
        void PushScreen(Screen screen, ITransition transition = null);

        /// <summary>Closes the screen on top of the stack (the last screen is never closed).</summary>
        void PopScreen(ITransition transition = null);
    }
}
