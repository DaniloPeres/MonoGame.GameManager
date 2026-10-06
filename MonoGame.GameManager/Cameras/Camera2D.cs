using Microsoft.Xna.Framework;
using MonoGame.GameManager.Core;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Cameras
{
    /// <summary>
    /// A 2D camera: position, zoom and rotation, following a target, world bounds and screen shake.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The camera maps world coordinates to viewport coordinates (from 0 to <see cref="ViewportSize"/>).
    /// <see cref="Position"/> is the world point shown at the top-left of the viewport when the zoom is 1 and the
    /// rotation is 0; zoom and rotation are applied around <see cref="Origin"/> (the center of the viewport by
    /// default). Use <see cref="LookAt"/> to center a world point.
    /// </para>
    /// <para>
    /// Use it with a <see cref="Controls.CameraPanel"/>, or pass <see cref="GetViewMatrix"/> to
    /// <c>SpriteBatch.Begin</c> when drawing a world yourself.
    /// </para>
    /// </remarks>
    public class Camera2D : IUpdatable
    {
        private readonly IRandom random;
        private Vector2? origin;
        private float zoom = 1f;
        private float minZoom = 0.1f;
        private float maxZoom = 10f;
        private Func<Vector2> followTarget;
        private float followLerpSpeed;
        private float shakeAmplitude;
        private float shakeDuration;
        private float shakeTimeLeft;
        private float shakeInterval;
        private float shakeTimer;
        private Vector2 shakeOffset;

        /// <param name="viewportSize">The size of the area where the world is shown.</param>
        /// <param name="random">The random generator used by the shake (optional).</param>
        public Camera2D(Vector2 viewportSize, IRandom random = null)
        {
            ViewportSize = viewportSize;
            this.random = random ?? RandomGenerator.Default;
        }

        /// <summary>The world point at the top-left of the viewport (when the zoom is 1 and the rotation is 0).</summary>
        public Vector2 Position { get; set; }

        /// <summary>The world point at the center of the viewport (ignoring the shake).</summary>
        public Vector2 Center
        {
            get => Position + Origin;
            set => Position = value - Origin;
        }

        /// <summary>The size of the viewport.</summary>
        public Vector2 ViewportSize { get; set; }

        /// <summary>The point of the viewport used as pivot for the zoom and the rotation (default: its center).</summary>
        public Vector2 Origin
        {
            get => origin ?? ViewportSize / 2f;
            set => origin = value;
        }

        /// <summary>The zoom (1 = no zoom, 2 = everything twice as big), clamped between MinZoom and MaxZoom.</summary>
        public float Zoom
        {
            get => zoom;
            set => zoom = MathHelper.Clamp(value, minZoom, maxZoom);
        }

        public float MinZoom
        {
            get => minZoom;
            set
            {
                minZoom = Math.Max(0.001f, value);
                Zoom = zoom;
            }
        }

        public float MaxZoom
        {
            get => maxZoom;
            set
            {
                maxZoom = Math.Max(minZoom, value);
                Zoom = zoom;
            }
        }

        /// <summary>The rotation of the view in radians (the world appears rotated clockwise for positive values).</summary>
        public float Rotation { get; set; }

        /// <summary>The area of the world the camera cannot leave (null = no limits).</summary>
        public RectangleF? Bounds { get; set; }

        /// <summary>True while the camera follows a target.</summary>
        public bool IsFollowing => followTarget != null;

        /// <summary>True while the camera shakes.</summary>
        public bool IsShaking => shakeTimeLeft > 0f;

        /// <summary>The current offset of the shake, in viewport pixels.</summary>
        public Vector2 ShakeOffset => shakeOffset;

        /// <summary>Centers the camera on a world point.</summary>
        public void LookAt(Vector2 worldPosition)
        {
            Center = worldPosition;
            ClampToBounds();
        }

        /// <summary>Moves the camera by a distance in world units.</summary>
        public void Move(Vector2 worldDistance)
        {
            Position += worldDistance;
            ClampToBounds();
        }

        /// <summary>
        /// Follows a target. The camera moves towards it smoothly (higher <paramref name="lerpSpeed"/> is faster;
        /// 0 or less snaps immediately).
        /// </summary>
        public void Follow(Func<Vector2> target, float lerpSpeed = 5f)
        {
            followTarget = target ?? throw new ArgumentNullException(nameof(target));
            followLerpSpeed = lerpSpeed;
        }

        public void StopFollowing() => followTarget = null;

        /// <summary>Shakes the camera; the intensity decreases until the end of the duration.</summary>
        /// <param name="amplitude">The maximum offset, in viewport pixels.</param>
        /// <param name="duration">The duration, in seconds.</param>
        /// <param name="frequency">How many times per second the offset changes.</param>
        public void Shake(float amplitude, float duration, float frequency = 30f)
        {
            shakeAmplitude = Math.Max(0f, amplitude);
            shakeDuration = Math.Max(0.0001f, duration);
            shakeTimeLeft = shakeDuration;
            shakeInterval = 1f / Math.Max(1f, frequency);
            shakeTimer = 0f;
        }

        public void StopShake()
        {
            shakeTimeLeft = 0f;
            shakeOffset = Vector2.Zero;
        }

        /// <summary>Updates the follow, the bounds and the shake. A <see cref="Controls.CameraPanel"/> calls it every frame.</summary>
        public void Update(float deltaSeconds)
        {
            if (followTarget != null)
            {
                var desiredPosition = followTarget() - Origin;
                Position = followLerpSpeed <= 0f
                    ? desiredPosition
                    : Vector2.Lerp(Position, desiredPosition, 1f - (float)Math.Exp(-followLerpSpeed * deltaSeconds));
            }

            ClampToBounds();
            UpdateShake(deltaSeconds);
        }

        /// <summary>The matrix that transforms world coordinates into viewport coordinates.</summary>
        public Matrix GetViewMatrix()
        {
            var pivot = Origin;
            return Matrix.CreateTranslation(-Position.X - pivot.X, -Position.Y - pivot.Y, 0f)
                * Matrix.CreateRotationZ(Rotation)
                * Matrix.CreateScale(zoom, zoom, 1f)
                * Matrix.CreateTranslation(pivot.X + shakeOffset.X, pivot.Y + shakeOffset.Y, 0f);
        }

        /// <summary>The matrix that transforms viewport coordinates into world coordinates.</summary>
        public Matrix GetInverseViewMatrix() => Matrix.Invert(GetViewMatrix());

        /// <summary>Converts a viewport position (eg: the mouse inside the viewport) to the world.</summary>
        public Vector2 ScreenToWorld(Vector2 viewportPosition) => Vector2.Transform(viewportPosition, GetInverseViewMatrix());

        /// <summary>Converts a world position to the viewport.</summary>
        public Vector2 WorldToScreen(Vector2 worldPosition) => Vector2.Transform(worldPosition, GetViewMatrix());

        /// <summary>The area of the world visible in the viewport (its bounding box when the camera is rotated).</summary>
        public RectangleF GetVisibleArea()
        {
            var inverse = GetInverseViewMatrix();
            var topLeft = Vector2.Transform(Vector2.Zero, inverse);
            var topRight = Vector2.Transform(new Vector2(ViewportSize.X, 0f), inverse);
            var bottomLeft = Vector2.Transform(new Vector2(0f, ViewportSize.Y), inverse);
            var bottomRight = Vector2.Transform(ViewportSize, inverse);

            var min = Vector2.Min(Vector2.Min(topLeft, topRight), Vector2.Min(bottomLeft, bottomRight));
            var max = Vector2.Max(Vector2.Max(topLeft, topRight), Vector2.Max(bottomLeft, bottomRight));
            return new RectangleF(min, max - min);
        }

        private void ClampToBounds()
        {
            if (!Bounds.HasValue)
                return;

            var bounds = Bounds.Value;
            var shake = shakeOffset;
            shakeOffset = Vector2.Zero; // the bounds apply to the camera, not to the shake
            var visible = GetVisibleArea();
            shakeOffset = shake;

            var correction = Vector2.Zero;
            if (visible.Width >= bounds.Width)
                correction.X = bounds.Center.X - visible.Center.X;
            else if (visible.Left < bounds.Left)
                correction.X = bounds.Left - visible.Left;
            else if (visible.Right > bounds.Right)
                correction.X = bounds.Right - visible.Right;

            if (visible.Height >= bounds.Height)
                correction.Y = bounds.Center.Y - visible.Center.Y;
            else if (visible.Top < bounds.Top)
                correction.Y = bounds.Top - visible.Top;
            else if (visible.Bottom > bounds.Bottom)
                correction.Y = bounds.Bottom - visible.Bottom;

            Position += correction;
        }

        private void UpdateShake(float deltaSeconds)
        {
            if (shakeTimeLeft <= 0f)
            {
                shakeOffset = Vector2.Zero;
                return;
            }

            shakeTimeLeft -= deltaSeconds;
            if (shakeTimeLeft <= 0f)
            {
                StopShake();
                return;
            }

            shakeTimer -= deltaSeconds;
            if (shakeTimer > 0f)
                return;

            shakeTimer = shakeInterval;
            var intensity = shakeAmplitude * (shakeTimeLeft / shakeDuration);
            shakeOffset = random.NextUnitVector() * intensity * random.NextFloat();
        }
    }
}
