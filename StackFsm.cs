using System.Collections.Generic;

namespace Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk
{
    delegate void State();

    class StackFsm
    {
        private readonly Stack<State> _stack;

        public StackFsm()
        {
            _stack = new Stack<State>(5);
        }

        public StackFsm(State initial)
        {
            _stack = new Stack<State>(5);
            _stack.Push(initial);
        }

        public void Update()
        {
            State state = GetCurrentState();
            if (state != null)
                state();
        }

        public State GetCurrentState()
        {
            if (_stack.Count > 0)
                return _stack.Peek();
            return null;
        }

        public void PushState(State state)
        {
            if (GetCurrentState() != state)
                _stack.Push(state);
        }

        public State PopState()
        {
            return _stack.Count > 0 ? _stack.Pop() : null;
        }

        public void SetCurrent(State state)
        {
            if (_stack.Count > 0)
                _stack.Pop();
            _stack.Push(state);
        }

        public void ClearStack()
        {
            _stack.Clear();
        }
    }
}