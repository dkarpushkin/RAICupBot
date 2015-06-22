using System;

namespace Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk
{
    public struct Rect2D
    {
        private Vector2D _lu;  //  левый верхний угол
        private Vector2D _rd;  //  правый нижний

        public Vector2D LeftUpperCorner
        {
            get { return _lu; }
            private set { _lu = value; }
        }

        public Vector2D RightLowerCorner
        {
            get { return _rd; }
            private set { _lu = value; }
        }
        public Vector2D Center
        {
            get
            {
                return new Vector2D(_lu.X + Math.Abs(_rd.X - _lu.X) / 2, _lu.Y + Math.Abs(_rd.Y - _lu.Y) / 2);
            }
            private set { _lu = value; }
        }

        public Rect2D(Vector2D leftUp, Vector2D rightDown)
        {
            _lu = leftUp; _rd = rightDown;
        }

        public Rect2D(double leftUpX, double leftUpY, double rightDownX, double rightDownY)
        {
            _lu = new Vector2D(leftUpX, leftUpY);
            _rd = new Vector2D(rightDownX, rightDownY);
        }

        public bool IsInside(Vector2D coord)
        {
            return coord.X >= _lu.X && coord.X <= _rd.X && coord.Y >= _lu.Y && coord.Y <= _rd.Y;
        }

        public static bool IsInside(Rect2D rect, Vector2D coord)
        {
            return rect.IsInside(coord);
        }

        public bool IsOutside(Vector2D coord)
        {
            return !IsInside(coord);
        }

        public static bool IsOutside(Rect2D rect, Vector2D coord)
        {
            return !rect.IsInside(coord);
        }

        //  тест на пересечение с другим прямоугльником
        //public bool IsIntersects(Rect rect)
        //{
        //    //  dummy!
        //    return IsInside(rect.lu) || IsInside(rect.rd)
        //}
    }
}