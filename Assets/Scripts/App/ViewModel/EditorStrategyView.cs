using Interfaces;

namespace App.ViewModel
{
    public abstract class EditorStrategyView<TI, TO> : Strategy<TI, TO>, IDisplayable, IInitializable
    {
        protected EditorStrategyView() : base(null)
        {
        }

        public abstract void Display();

        public abstract void Initialize();
    }
}