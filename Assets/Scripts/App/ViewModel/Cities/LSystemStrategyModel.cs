using System;
using Cities.Roads;
using Interfaces;

namespace App.ViewModel.Cities
{
    [Serializable]
    public class LSystemStrategyModel : IViewAdapter<IGenerator<RoadNetwork>>
    {
        private LSystemStrategy _strategy;

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