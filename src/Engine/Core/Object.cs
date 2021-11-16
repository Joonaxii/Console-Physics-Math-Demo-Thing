using System;
using System.Runtime.InteropServices;

namespace Joonaxii.Engine.Core
{
    public class Object
    {
        public string name;
   
        public Object() : this("") { }
        public Object(string name)
        {
            this.name = name;
        }

        public override string ToString() => name;


        protected virtual void OnInstantiate<T>(T source) where T : Object
        {
            source.name = name;
        }

        protected virtual void OnInstantiate() { }

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
            name = null;
        }

        public static void Destroy(ref Object obj) 
        {
            obj.OnDestroy();
            obj = null;
        }
    }
}