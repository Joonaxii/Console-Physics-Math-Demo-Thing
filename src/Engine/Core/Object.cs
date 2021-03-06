using System;
using System.Runtime.InteropServices;

namespace Joonaxii.Engine.Core
{
    public class Object
    {
        public virtual string Name { get; set; }
   
        public Object() : this("") { }
        public Object(string name)
        {
            this.Name = name;
        }

        public override string ToString() => Name;

        protected virtual void OnInstantiate<T>(T source) where T : Object
        {
            source.Name = Name;
        }

        protected virtual void OnInstantiate() { }

        public static Object Instantiate(Type type)       
        {
            if (!type.IsSubclassOf(typeof(Object))) { return null; }
            Object temp = Activator.CreateInstance(type) as Object;
            temp.OnInstantiate();
            return temp;
        }

        public static T Instantiate<T>() where T : Object
        {
            T temp = Activator.CreateInstance<T>();
            temp.OnInstantiate();
            return temp;
        }

        public static T Instantiate<T>(T reference) where T : Object
        {
            T temp = Instantiate<T>();
            reference.OnInstantiate(temp);
            return temp;
        }

        protected virtual void OnDestroy()
        {
            Name = null;
        }

        public static void Destroy<T>(ref T obj) where T : Object
        {
            if (obj == null) { return; }

            obj.OnDestroy();
            obj = null;
        }

        public static void Destroy(ref Object obj) 
        {
            if(obj == null) { return; }

            obj.OnDestroy();
            obj = null;
        }
    }
}