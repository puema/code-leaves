using UnityEngine;

namespace Utilities
{
    public static class GameObjectUtils
    {
        public static float GetYSize(GameObject obj)
        {
            return obj.GetComponent<MeshRenderer>().bounds.size.y;
        }

        public static float GetXSize(GameObject obj)
        {
            return obj.GetComponent<MeshRenderer>().bounds.size.x;
        }

        public static float GetZSize(GameObject obj)
        {
            return obj.GetComponent<MeshRenderer>().bounds.size.z;
        }
    }
}