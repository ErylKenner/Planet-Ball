using ClientServerPrediction;
using UnityEngine;

namespace MockModel
{
    public class MockPlayer : IStateful, IInputful
    {
        public Vector2 GetPosition()
        {
            return position;
        }

        private Vector2 position = Vector2.zero;
        public void ApplyInput(Inputs input)
        {
            position += input.movement;
        }

        public Inputs GetInput()
        {
            // Always move up
            Inputs inputs = new Inputs
            {
                movement = Vector2.up
            };
            return inputs;
        }

        public State GetState()
        {
            State state = new State
            {
                position = position
            };
            return state;
        }

        public void SetState(State state)
        {
            position = state.position;
        }

        public void PredictState(State state)
        {

        }

        public void SmoothState(State oldState, State newState, RunContext runContext, StateError stateError)
        {

        }
    }
}
