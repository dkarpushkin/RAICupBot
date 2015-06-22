using System;
using System.IO.IsolatedStorage;

namespace Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk
{
    public struct Vector2D
    {
        public double X;
        public double Y;

        public bool Equals(Vector2D other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vector2D && Equals((Vector2D)obj);
        }

        private static double _equalityTolerance = 0.0000001d;
        public static double EqualityTolerance
        {
            get
            {
                return _equalityTolerance;
            }
            set
            {
                _equalityTolerance = value;
            }
        }

        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector2D operator+(Vector2D a, Vector2D b)
        {
            return new Vector2D
            {
                X = a.X + b.X,
                Y = a.Y + b.Y
            };
        }

        public static Vector2D operator-(Vector2D a, Vector2D b)
        {
            return new Vector2D
            {
                X = a.X - b.X,
                Y = a.Y - b.Y
            };
        }

        public static Vector2D operator+(Vector2D a)
        {
            return new Vector2D
            {
                X = +a.X,
                Y = +a.Y
            };
        }

        public static Vector2D operator-(Vector2D a)
        {
            return new Vector2D
            {
                X = -a.X,
                Y = -a.Y
            };
        }

        public static bool operator==(Vector2D a, Vector2D b)
        {
            return  Math.Abs(a.X - b.X) < EqualityTolerance &&
                    Math.Abs(a.Y - b.Y) < EqualityTolerance;
        }

        public static bool operator!=(Vector2D a, Vector2D b)
        {
            return !(a == b);
        }

        public static Vector2D operator/(Vector2D a, double n)
        {
            return new Vector2D
            {
                X = a.X / n,
                Y = a.Y / n
            };
        }

        public static Vector2D operator*(Vector2D a, double n)
        {
            return new Vector2D
            {
                X = a.X * n,
                Y = a.Y * n
            };
        }

        public static Vector2D operator*(double n, Vector2D a)
        {
            return a * n;
        }

        public static double DotProduct(Vector2D a, Vector2D b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public double DotProduct(Vector2D a)
        {
            return DotProduct(this, a);
        }

        public double Magnitude
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y);
            }
            set
            {
                if (value < EqualityTolerance)
                    throw new ArgumentNullException("The magnitude must not be zero");

                if (IsOrigin())
                    throw new ArgumentException("Cannot set the magnitude of zero vector");

                this = this * (Math.Abs(value) / Magnitude);
            }
        }

        public static bool IsUnit(Vector2D v)
        {
            return Math.Abs(v.Magnitude - 1) < EqualityTolerance;
        }

        public bool IsUnit()
        {
            return Math.Abs(Magnitude - 1) < EqualityTolerance;
        }

        public static bool IsOrigin(Vector2D v)
        {
            return v.Magnitude < EqualityTolerance;
        }

        public bool IsOrigin()
        {
            return Magnitude < EqualityTolerance;
        }

        public static Vector2D Normalize(Vector2D v)
        {
            double mag = v.Magnitude;

            if (mag < EqualityTolerance)
                throw new DivideByZeroException("Trying to normalize zero vector");

            return v / mag;
        }

        public void Normalize()
        {
            this =  Normalize(this);
        }

        public static double Distance(Vector2D a, Vector2D b)
        {
            return (b - a).Magnitude;
        }

        public double Distance(Vector2D a)
        {
            return Distance(this, a);
        }

        public static double Angle(Vector2D a, Vector2D b)
        {
            double aAngle = Math.Atan2(a.Y, a.X);
            double bAngle = Math.Atan2(b.Y, b.X);
            double angle = bAngle - aAngle;

            while (angle > Math.PI)
            {
                angle -= 2.0D * Math.PI;
            }

            while (angle < -Math.PI)
            {
                angle += 2.0D * Math.PI;
            }

            return angle;
            //return Math.Acos(DotProduct(a, b) / (a.Magnitude * b.Magnitude));
        }

        public static Vector2D Scale(Vector2D a, double scale)
        {
            return Normalize(a) * scale;
        }

        public Vector2D Scale(double scale)
        {
            return Normalize(this) * scale;
        }
    }
}