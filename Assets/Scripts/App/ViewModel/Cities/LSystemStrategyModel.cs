using Cities.Roads;
using Interfaces;

namespace App.ViewModel.Cities
{
    public class LSystemModel : IViewAdapter<IGenerator<RoadNetwork>>
    {
        private LSystemStrategy _strategy;

        public IGenerator<RoadNetwork> Model
        {
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
            throw new System.NotImplementedException();
        }
    }
}