namespace _GameAssets.Scripts.Extensions
{
    public static class SpringMathExtensions
    {
        public static float CalculateForceDamped(float currentLength, float lengthVelocity, float restLength,
            float strength, float damper)
        {
            float lengthOffset = restLength - currentLength;
            return (lengthOffset * strength) - (lengthVelocity * damper);
        }
    }
}