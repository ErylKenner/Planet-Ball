namespace ClientServerPrediction
{
    public interface IStateful
    {
        public State GetState();
        public void SetState(State state);

        public void PredictState(State state);
    }
}