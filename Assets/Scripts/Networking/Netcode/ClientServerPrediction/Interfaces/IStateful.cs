namespace ClientServerPrediction
{
    public interface IStateful
    {
        public State GetState();
        public void SetState(State state);   
    }
}