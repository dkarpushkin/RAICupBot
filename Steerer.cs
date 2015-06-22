using System.Collections.Generic;

namespace Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk
{
    class Steerer
    {
        private const double MaxVelocity = 10;
        private const double MaxAhead = 100;

        private const double SeekForceWeight = 1.0d;
        private const double AvoidenceForceWeight = 30.0d;

        public static Vector2D SeekAvoidingForce(Vector2D pos, Vector2D speed, Vector2D target, IEnumerable<Vector2D> threatsPos, IEnumerable<Vector2D> threatSpeeds, double threatRadius)
        {
            return SeekForceWeight * SeekForce(pos, speed, target, MaxVelocity) +
                   AvoidenceForceWeight * AvoidForce(pos, speed, threatsPos, threatSpeeds, threatRadius);
        }

        public static Vector2D AvoidForce(Vector2D pos, Vector2D speed, IEnumerable<Vector2D> threatsPos, IEnumerable<Vector2D> threatSpeeds, double threatRadius)
        {
            double dynamicLength = speed.Magnitude / MaxVelocity * MaxAhead;

            Vector2D ahead = pos + Vector2D.Scale(speed, dynamicLength);
            Vector2D ahead1 = pos + Vector2D.Scale(speed, dynamicLength / 2);

            var force = new Vector2D(0, 0);

            foreach (Vector2D threat in threatsPos)
            {
                if (threat.Distance(ahead) <= threatRadius)
                    force = force + (ahead - threat);//EvadeForce(pos, speed, threat, threatSpeeds[i], 10);
                else if (threat.Distance(ahead1) <= threatRadius)
                    force = force + (ahead1 - threat);//EvadeForce(pos, speed, threat, threatSpeeds[i], 10);
                else if (threat.Distance(pos) <= threatRadius)
                    force = force + (pos - threat);//EvadeForce(pos, speed, threat, threatSpeeds[i], 10);
            }

            if (!force.IsOrigin())
                force.Normalize();

            return force;// *MAX_AVOIDENCE_FORCE;
        }

        public static Vector2D EvadeForce(Vector2D pos, Vector2D speed, Vector2D threatPos, Vector2D threatSpd, int ticks)
        {
            Vector2D futureThreatPos = threatPos + threatSpd * ticks;

            return EscapeForce(pos, speed, futureThreatPos, MaxVelocity);
        }

        public static Vector2D SeekForce(Vector2D pos, Vector2D speed, Vector2D target, double maxSpeed)
        {
            Vector2D desiredVelocity = target - pos;
            //desiredVelocity.Magnitude = maxSpeed;

            if (desiredVelocity.IsOrigin())
                return desiredVelocity; //  нулевой вектор

            return Vector2D.Normalize(desiredVelocity - speed);
        }

        public static Vector2D EscapeForce(Vector2D pos, Vector2D speed, Vector2D threat, double maxSpeed)
        {
            Vector2D desiredVelocity = pos - threat;
            //desiredVelocity.Magnitude = maxSpeed;

            if (desiredVelocity.IsOrigin())
                return desiredVelocity;

            return Vector2D.Normalize(desiredVelocity - speed);
        }
    }
}