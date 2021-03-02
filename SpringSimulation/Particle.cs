using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Windows;
using System.Drawing;

namespace SpringSimulation
{
    class Particle
    {
        public Vector2 Acceleration, Velocity, Position;
        public float Mass;
        public bool Locked;

        public Particle(float x, float y, bool locked = false)
        {
            Locked = locked;
            Acceleration = Velocity = Vector2.Zero;
            Position = new Vector2(x, y);
            Mass = 1;
        }

        public void ApplyForce(Vector2 force)
        {
            Vector2 f = Vector2.Divide(force, Mass);
            Acceleration = Vector2.Add(Acceleration, f);
        }

        public void Update()
        {
            if (!Locked)
            {
                Velocity = Vector2.Multiply(Velocity, .99f);
                UpdatePosition();
                Velocity = Vector2.Add(Velocity, Acceleration);
                Acceleration = Vector2.Multiply(Acceleration, 0);
            }
        }

        private void UpdatePosition()
        {
            Position = Vector2.Add(Position, Velocity);

        }

        public void ResetMomentum() => Acceleration = Velocity = Vector2.Zero;
        public void MoveToCoordinates(float x, float y) => MoveToVector(new Vector2(x, y));
        public void MoveToPoint(Point point) => MoveToCoordinates(point.X, point.Y);
        public void MoveToPoint(PointF point) => MoveToCoordinates(point.X, point.Y);

        public void MoveToVector(Vector2 position)
        {
            ResetMomentum();
            Position = position;
        }
    }
}
