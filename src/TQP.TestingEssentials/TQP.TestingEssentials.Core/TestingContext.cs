using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TQP.TestingEssentials.Core
{
    [TestFixture]
    public abstract class TestingContext<T> where T : class
    {
        private Fixture _fixture;
        private Dictionary<Type, Mock> _injectedMocks;
        private Dictionary<Type, object> _injectedConcreteClasses;
        private Mock<T> _sut;

        [SetUp]
        public void BaseSetup()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            _injectedMocks = new Dictionary<Type, Mock>();
            _injectedConcreteClasses = new Dictionary<Type, object>();

            SetUp();

            // Inject the rest of constructor params that have not been injected during SetUp
            var type = typeof(T);
            var constructors = type.GetConstructors();
            var method = typeof(TestingContext<T>).GetMethod("GetMockFor");
            foreach (var contructor in constructors)
            {
                var parameters = contructor.GetParameters();
                foreach (var parameter in parameters)
                {
                    var parameterType = parameter.ParameterType;
                    if (!_injectedConcreteClasses.ContainsKey(parameterType) && (parameterType.IsInterface || parameterType.IsAbstract))
                    {
                        var genericMethod = method.MakeGenericMethod(parameterType);
                        genericMethod.Invoke(this, null);
                    }
                }
            }
        }

        public virtual void SetUp() { }

        /// <summary>
        /// Generates a mock for a class and injects it into the final fixture
        /// </summary>
        /// <typeparam name="TMockType"></typeparam>
        /// <returns></returns>
        public Mock<TMockType> GetMockFor<TMockType>() where TMockType : class
        {
            var mockType = typeof(TMockType);
            var existingMock = _injectedMocks.FirstOrDefault(x => x.Key == mockType);
            if (existingMock.Key == null)
            {
                var newMock = new Mock<TMockType>();
                _injectedMocks.Add(mockType, newMock);
                _fixture.Inject(newMock.Object);
                return newMock;
            }

            return existingMock.Value as Mock<TMockType>;
        }

        /// <summary>
        /// Injects a concrete class to be used when generating the fixture. 
        /// </summary>
        /// <typeparam name="TClassType"></typeparam>
        /// <returns></returns>
        public void InjectMock<TClassType>(TClassType injectedClass) where TClassType : class
        {
            var classType = typeof(TClassType);
            var existingClass = _injectedConcreteClasses.FirstOrDefault(x => x.Key == classType);
            if (existingClass.Key != null)
            {
                throw new ArgumentException($"{injectedClass.GetType().Name} has been injected more than once");
            }
            _injectedConcreteClasses.Add(classType, injectedClass);
            _fixture.Inject(injectedClass);
        }

        public TClassType GetInjectedMock<TClassType>() where TClassType : class
            => _injectedConcreteClasses[typeof(TClassType)] as TClassType;

        public T Sut => CreateSutMock().Object;

        public Mock<T> CreateSutMock() => _sut ??= _fixture.Create<Mock<T>>();
    }
}
