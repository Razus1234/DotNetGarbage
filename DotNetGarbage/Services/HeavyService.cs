namespace DotNetGarbage.Services
{
    public class HeavyService : IHeavyService
    {
        private byte[] _memory;

        public void Allocate()
        {
            _memory = new byte[500 * 1024 * 1024]; // 500MB
        }
    }
}
