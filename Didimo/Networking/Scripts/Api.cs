
namespace Didimo.Networking
{
    public static class Api
    {
        private static IApi _instance;

        public static IApi Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DidimoApi();
                }

                return _instance;
            }
        }

        public static void SetImplementation(IApi api) { _instance = api; }
    }
}