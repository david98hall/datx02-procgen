using System;
using Interfaces;
using Packages.Rider.Editor.UnitTesting;
using UnityEngine;

namespace Terrain
{
    public class Whittaker : IGenerator<Texture2D>
    {
        #region Properties
        
        private readonly float[,] _noiseMap;
        
        #endregion
        
        public Whittaker(float[,] noiseMap)
        {
            _noiseMap = noiseMap;
        }
        public Texture2D Generate()
        {
            Texture2D texture = new Texture2D(_noiseMap.GetLength(0), 
                _noiseMap.GetLength(1));

            for (var x = 0; x < _noiseMap.GetLength(0); x++)
            {
                for (var y = 0; y < _noiseMap.GetLength(1); y++)
                {
                    texture.SetPixel(x, y, Color.red);
                }
            }
            texture.Apply(false);
            return texture;
        }

        #region Private methods
        
        #endregion
    }
}
