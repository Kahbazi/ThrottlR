namespace ThrottlR
{
    public class DisableThrottle : IDisableThrottle
    {
        public static IDisableThrottle Instance { get; } = new DisableThrottle();

        private DisableThrottle()
        {

        }
    }
}
