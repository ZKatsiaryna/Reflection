using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;

namespace MyIoC
{
    public class Container
    {
        private readonly IDictionary<Type, Type> types;

        public Container()
        {
            types = new Dictionary<Type, Type>();
        }

        public void AddAssembly(Assembly assembly)
        {
            var assemblyTypes = assembly.ExportedTypes;

            foreach (var assemblyType in assemblyTypes)
            {
                var exportAttribute = assemblyType.GetCustomAttribute<ExportAttribute>();
                if (exportAttribute != null || ContainsImportProperty(assemblyType))
                {
                    types.Add(exportAttribute?.Contract ?? assemblyType, assemblyType);
                }

                var constructorAttribute = assemblyType.GetCustomAttribute<ImportConstructorAttribute>();
                if (constructorAttribute != null)
                {
                    types.Add(assemblyType, assemblyType);
                }
            }
        }

        private bool ContainsImportProperty(Type type)
        {
            var importProperties = type.GetProperties();
            var result = importProperties.Where(p => p.GetCustomAttributes<ImportAttribute>() != null);
            return result.Any();
        }

        public void AddType(Type type)
        {
            types.Add(type, type);
        }

        public void AddType(Type type, Type baseType)
        {
            types.Add(baseType, type);
        }

        public object CreateInstance(Type type)
        {
            Type implementation = types[type];

            var instance = CreateConstructarInstance(implementation);

            return CreatePropertyIns(implementation, instance);
        }

        private object CreateConstructarInstance(Type implementation)
        {
            ConstructorInfo constructor = implementation.GetConstructors()[0];
            ParameterInfo[] conParameters = constructor.GetParameters();

            if (conParameters.Length == 0)
            {
                return Activator.CreateInstance(implementation);
            }

            List<object> parametrs = new List<object>(conParameters.Length);
            foreach (ParameterInfo param in conParameters)
            {
                parametrs.Add(CreateInstance(param.ParameterType));
            }

            return constructor.Invoke(parametrs.ToArray());
        }

        public object CreatePropertyIns(Type implementation, Object instance)
        {
            var properties = implementation.GetProperties();
            var allPproperties = properties.Where(p => p.GetCustomAttributes<ImportAttribute>() != null);

            foreach (var property in allPproperties)
            {
                var propertyInstance = CreateInstance(property.PropertyType);
                property.SetValue(instance, propertyInstance);
            }

            return instance;
        }

        public T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }

        public void Sample()
        {
            var container = new Container();
            container.AddAssembly(Assembly.GetExecutingAssembly());

            var customerBLL = (CustomerBLL)container.CreateInstance(typeof(CustomerBLL));
            var customerBLL2 = container.CreateInstance<CustomerBLL>();

            container.AddType(typeof(CustomerBLL));
            container.AddType(typeof(Logger));
            container.AddType(typeof(CustomerDAL), typeof(ICustomerDAL));
        }
    }
}
