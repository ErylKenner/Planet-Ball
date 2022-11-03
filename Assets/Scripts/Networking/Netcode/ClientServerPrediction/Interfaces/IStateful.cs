namespace ClientServerPrediction
{
    public interface IStateful
    {
        public State GetState();
        public void SetState(State state);

        public void PredictState(State state);

        public void SmoothState(State oldState, State newState, RunContext runContext, StateError stateError);
    }
}