using Microsoft.Xna.Framework;
using MonoGame.GameManager.Core;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Physics
{
    /// <summary>
    /// Simple kinematics: velocity, acceleration, gravity, drag and maximum speed, integrated every update.
    /// </summary>
    public class Mover : IUpdatable
    {
        public Vector2 Position { get; set; }

        /// <summary>Velocity in units per second.</summary>
        public Vector2 Velocity { get; set; }

        /// <summary>Acceleration in units per second squared.</summary>
        public Vector2 Acceleration { get; set; }

        /// <summary>A constant acceleration, eg: (0, 980) for a platformer.</summary>
        public Vector2 Gravity { get; set; }

        /// <summary>The fraction of the velocity lost per second, from 0 (none) to 1 (stops at once).</summary>
        public float Drag { get; set; }

        /// <summary>The maximum speed (0 = unlimited).</summary>
        public float MaxSpeed { get; set; }

        /// <summary>When false, the mover does not move.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Adds an instant change of velocity (eg: a jump or an explosion).</summary>
        public void AddImpulse(Vector2 impulse) => Velocity += impulse;

        /// <summary>Integrates the movement of one frame.</summary>
        public virtual void Update(float deltaSeconds)
        {
            if (!IsEnabled || deltaSeconds <= 0f)
                return;

            BeforeMove();

            var velocity = Velocity + (Acceleration + Gravity) * deltaSeconds;
            if (Drag > 0f)
                velocity *= (float)Math.Pow(MathUtils.Clamp01(1f - Drag), deltaSeconds);
            if (MaxSpeed > 0f)
                velocity = velocity.Truncate(MaxSpeed);

            Velocity = velocity;
            Position += velocity * deltaSeconds;

            AfterMove();
        }

        /// <summary>Called before the movement is integrated.</summary>
        protected virtual void BeforeMove() { }

        /// <summary>Called after the position was updated.</summary>
        protected virtual void AfterMove() { }
    }
}
