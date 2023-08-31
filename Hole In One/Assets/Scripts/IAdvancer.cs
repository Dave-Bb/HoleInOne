namespace Assets.Scripts
{
    public interface IAdvancer
    {
        void OnAdvance(float advanceValueOne);

        float CurrentAdvanceValue();
    }
}