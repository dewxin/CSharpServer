using System;
using Autofac;
using System.Collections.Generic;
using System.Linq;

namespace ProjectCommon.Unit
{
    public class AfContainer
    {
        protected SortedDictionary<string, AfComponent> componentDict { get; private set; }

        public IContainer AutoContainer { get; private set; }

        public AfContainer(IContainer container)
        {
            AutoContainer = container;
            componentDict = new();
        }

        public bool CreateComponent<TComponent>() where TComponent : AfComponent
        {
            return AddComponent(AutoContainer.Resolve<TComponent>());
        }

        public bool AddComponent<TComponent>(TComponent component) where TComponent : AfComponent
        {
            var typeName = component.GetType().Name;
            if (!componentDict.ContainsKey(typeName))
            {
                componentDict.Add(typeName, component);
                return true;
            }
            return false;
        }

        public T GetComponent<T>() where T : AfComponent
        {
            return GetComponent(typeof(T)) as T;
        }

        public AfComponent GetComponent(Type type)
        {
            //todo 可能性能有优化的空间
            var name = type.Name;
            if (componentDict.ContainsKey(name))
                return componentDict[name];

            return null;
        }


        //在serverThread中执行
        public void Tick(double elapsed)
        {
            foreach (var it in componentDict)
            {
                var timer = CheckTimeHelper.StartTime();

                it.Value.Tick(elapsed);

                if (timer.IntervalGreaterThan())
                    timer.Error("Heartbeat-" + it.Value.GetType().Name);
            }
        }
    }
}