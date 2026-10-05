using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Timers;
using System;

namespace MonoGame.GameManager.Physics
{
    /// <summary>
    /// A <see cref="Mover"/> that moves a control (its <see cref="ILayoutElement.PositionAnchor"/>).
    /// It stops when the control is disposed.
    /// </summary>
    /// <example>
    /// <code>
    /// var ballMover = new ControlMover(ball) { Velocity = new Vector2(300, 200) }.Start();
    /// </code>
    /// </example>
    public class ControlMover : Mover
    {
        private IScheduler registeredScheduler;

        public ControlMover(IControl control)
        {
            Control = control ?? throw new ArgumentNullException(nameof(control));
            Position = control.PositionAnchor;
            control.Disposed += OnControlDisposed;
        }

        public IControl Control { get; }

        /// <summary>True while the mover is registered in a scheduler.</summary>
        public bool IsRunning => registeredScheduler != null;

        /// <summary>
        /// Starts moving the control every frame, with the given scheduler or the scheduler of the current screen.
        /// </summary>
        public ControlMover Start(IScheduler scheduler = null)
        {
            Stop();
            registeredScheduler = scheduler ?? ServiceProvider.Scheduler;
            registeredScheduler.Add(this);
            return this;
        }

        public ControlMover Stop()
        {
            registeredScheduler?.Remove(this);
            registeredScheduler = null;
            return this;
        }

        /// <inheritdoc />
        protected override void BeforeMove() => Position = Control.PositionAnchor; // the control can be moved by other code

        /// <inheritdoc />
        protected override void AfterMove() => Control.SetPosition(Position);

        private void OnControlDisposed(IControl control) => Stop();
    }
}
