using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SpringSimulation
{
    class Spring
    {
        public float K, RestLength;
        public Particle A, B;
        public Spring(float k, float restLength, Particle A, Particle B)
        {
            K = k;
            RestLength = restLength;
            this.A = A;
            this.B = B;
        }

        public void Update()
        {
            Vector2 force = Vector2.Subtract(B.Position, A.Position);//Vector.Subtract(A.Position, B.Position);
            float deltaX = force.Length() - RestLength;
            force = Vector2.Normalize(force);
            force = Vector2.Multiply(force, K * deltaX);
            A.ApplyForce(force);
            force = Vector2.Multiply(force, -1);
            B.ApplyForce(force);
        }
    }
}
