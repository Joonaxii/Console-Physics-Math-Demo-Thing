using System.Collections.Generic;

namespace Joonaxii.Engine.Core
{
    public class GameObjectManager
    {
        public static GameObjectManager Instance { get; private set; }
        private List<GameObject> _objects = new List<GameObject>(4096);

        public GameObjectManager()
        {
            Instance = this;
        }

        public void RegisterGameObject(GameObject go)
        {
            _objects.Add(go);
        }

        public void Update(float delta)
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                var go = _objects[i];
                if (go.IsActiveInHierarchy)
                {
                    go.TriggerUpdate(delta);
                }
            }
        }
    }
}