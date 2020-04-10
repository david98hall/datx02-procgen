using System;
using Cities.Roads;
using Interfaces;
using UnityEngine;

namespace App.ViewModel.Cities
{
    [Serializable]
    public class LSystemStrategyModel : IViewAdapter<IGenerator<RoadNetwork>>
    {
        private LSystemStrategy _strategy;

        internal IInjector<float[,]> HeightMapInjector { get; set; }
        
        public IGenerator<RoadNetwork> Model
        {
            // is used for generation
            get
            {
                return _strategy;
            }
            set
            {
                _strategy = value as LSystemStrategy;
            }
        }

        public void Display()
        {
            // Is used for displaying stuff
        }
    }
}