using System;
using Packages.Rider.Editor.UnitTesting;
using UnityEngine;

namespace Whittaker
{
    public class Whittaker : IWhittaker
    {
        public UnityEngine.Color GetColor(Vector3 vertex)
        {
            return UnityEngine.Color.red;
        }

        public UnityEngine.Color[] GetColors(Vector3[] vertices)
        {
            Color[] colors = new Color[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                colors[i] = GetColor(vertices[i]);
            }

            return colors;
        }
        

        private void Test()
        {
            Vector3[] vertices;
            float[,] heights;
            float[,] whittaker;
        }
        
    }
}
