
namespace ClientServerPrediction {
    public interface IInputful
    {
        public Inputs GetInput();
        public void ApplyInput(Inputs input);
    }
}