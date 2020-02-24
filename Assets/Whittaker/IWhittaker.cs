using UnityEngine;

namespace Whittaker
{
    public interface IWhittaker
    {
        Color GetColor(Vector3 vertex);

        Color[] GetColors(Vector3[] vertices);
    }
}