using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;

namespace TMP.NET.Modules
{
    public class PrivateConstructorContractResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            // Hanya proses kelas dengan konstruktor internal dan ada parameter
            if (HasInternalParameterizedConstructor(objectType))
            {
                contract.OverrideCreator = CreateObjectConstructor;
            }

            return contract;
        }

        private bool HasInternalParameterizedConstructor(Type objectType)
        {
            var constructor = objectType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                                        .FirstOrDefault(ctor => !ctor.IsStatic && ctor.GetParameters().Length > 0);

            return constructor != null;
        }

        private object CreateObjectConstructor(params object[] args)
        {
            var objectType = args[0] as Type;

            var constructor = objectType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                                        .FirstOrDefault(ctor => !ctor.IsStatic && ctor.GetParameters().Length > 0);

            if (constructor == null)
            {
                throw new InvalidOperationException($"No suitable constructor found for type {objectType}.");
            }

            var parameters = constructor.GetParameters();
            var parameterValues = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (args.Length > i + 1)
                {
                    // Menggunakan nilai dari args sesuai posisi parameter
                    parameterValues[i] = args[i + 1];
                }
                else
                {
                    // Jika tidak ada nilai yang sesuai di args, Anda dapat memberikan nilai default atau menimbulkan exception
                    throw new InvalidOperationException($"Missing argument for parameter {parameters[i].Name} in constructor.");
                }
            }

            return constructor.Invoke(parameterValues);
        }
    }
}
