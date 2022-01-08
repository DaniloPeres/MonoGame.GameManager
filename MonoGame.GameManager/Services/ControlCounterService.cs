namespace MonoGame.GameManager.Services
{
    public class ControlCounterService
    {
        private int idCounter;
        public int GenerateControlId() => ++idCounter;
    }
}
