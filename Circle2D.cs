namespace Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk
{
    struct Circle2D
    {
        public Vector2D Position;
        public double Radius;

        public Circle2D(Vector2D pos, double radius)
        {
            Position = pos;
            Radius = radius;
        }

        public Circle2D(double posX, double posY, double radius)
        {
            Position = new Vector2D(posX, posY);
            Radius = radius;
        }

        public bool IsInside(Vector2D coord)
        {
            return (coord - Position).Magnitude < Radius;
        }

        public bool IsOutside(Vector2D coord)
        {
            return !IsInside(coord);
        }

        //  тест на пересечение с другим кругом
        public bool IsIntersects(Circle2D circle)
        {
            return (circle.Position - Position).Magnitude <= circle.Radius + Radius;
        }

        //  тест на пересечение с лучом
        //public bool IsRayIntersects(Vector2d origin, Vector2d direction, out Vector2d p0, out Vector2d p1)
        //{
        //    if (IsInside(origin))
        //        return true;


        //}
    }
}