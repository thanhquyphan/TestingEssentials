using Moq;
using Newtonsoft.Json;
using System.Linq;

namespace TQP.TestingEssentials.Core
{
    public static class MockExtensions
    {
        public static Mock<T> RegisterForJsonSerialization<T>(this Mock<T> mock) where T : class
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new[] { new JsonMockConverter() }
            };

            mock.SetupAllProperties();
            JsonMockConverter.RegisterMock(
                mock,
                () => typeof(T).GetProperties().ToDictionary(p => p.Name, p => p.GetValue(mock.Object))
            );
            return mock;
        }
    }
}
