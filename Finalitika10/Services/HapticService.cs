namespace Finalitika10.Services.AppServices
{
    public sealed class HapticService : IHapticService
    {
        public void PerformClick()
        {
            try
            {
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            }
            catch
            {
                
            }
        }
    }
}