using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;

namespace SoundEngine
{    
    public interface IGameComponent
    {
        void Initialize();
    }

    public static class ServiceManager
    {
        /*
        private static readonly List<object> services = new List<object>();
        public static bool IsFull;
        public static bool FirstLoadDone;

        public static Game Game { get; set; }

        static ServiceManager()
        {
        }

        public static void Clear()
        {
            foreach (object obj in services)
            {
                foreach (Type type in obj.GetType().GetInterfaces())
                    Game.Services.RemoveService(type);

                if (obj is IDisposable)
                    (obj as IDisposable).Dispose();
            }

            services.Clear();
        }

        public static void AddComponent(IGameComponent component)
        {
            AddComponent(component, false);
        }

        public static void AddComponent(IGameComponent component, bool addServices)
        {
            if (!addServices)
                InjectServices((object)component);

            ((Collection<IGameComponent>)Game.Components).Add(component);

            if (addServices)
                AddService((object)component);

            Console.WriteLine( component.GetType().Name + " loaded");
        }

        public static T Get<T>() where T : class
        {
            return Game.Services.GetService(typeof(T)) as T;
        }

        public static object Get(Type type)
        {
            return Game.Services.GetService(type);
        }

        public static void AddService(object service)
        {
            foreach (Type type in service.GetType().GetInterfaces())
            {
                if (type != typeof(IDisposable) && type != typeof(IUpdateable) && (type != typeof(IDrawable) && type != typeof(IGameComponent)) && (!type.Name.StartsWith("IComparable") && type.GetCustomAttributes(typeof(DisabledServiceAttribute), false).Length == 0))
                    Game.Services.AddService(type, service);
            }
            services.Add(service);
        }

        public static void InitializeServices()
        {
            foreach (object componentOrService in services)
                InjectServices(componentOrService);
        }

        public static void InjectServices(object componentOrService)
        {
            Type type = componentOrService.GetType();
            do
            {
                foreach (PropertyInfo propInfo in ReflectionHelper.GetSettableProperties(type))
                {
                    ServiceDependencyAttribute firstAttribute = ReflectionHelper.GetFirstAttribute<ServiceDependencyAttribute>(propInfo);
                    if (firstAttribute != null)
                    {
                        Type propertyType = propInfo.PropertyType;
                        object obj = Game == null ? (object)null : Game.Services.GetService(propertyType);
                        if (obj == null)
                        {
                            //if (!firstAttribute.Optional)
                            //    throw new MissingServiceException(type, propertyType);
                        }
                        else
                            propInfo.GetSetMethod(true).Invoke(componentOrService, new object[1]
              {
                obj
              });
                    }
                }
                type = type.BaseType;
            }
            while (type != typeof(object));
        }

        public static void RemoveComponent<T>(T component) where T : IGameComponent
        {
            lock (Game)
            {
                if ((object)component is IDisposable)
                    ((object)component as IDisposable).Dispose();
                ((Collection<IGameComponent>)Game.Components).Remove((IGameComponent)component);
            }
        }
         * */
    }
}
