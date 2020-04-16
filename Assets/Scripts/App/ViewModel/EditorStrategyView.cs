using Interfaces;

namespace App.ViewModel
{
    /// <summary>
    /// A view for a strategy to generate something based on an input.
    /// </summary>
    /// <typeparam name="TI">The input type of the generation.</typeparam>
    /// <typeparam name="TO">The output type of the generation.</typeparam>
    public abstract class EditorStrategyView<TI, TO> : Strategy<TI, TO>, IDisplayable, IInitializable
    {
        protected EditorStrategyView() : base(null)
        {
        }

        /// <summary>
        /// Displays the view.
        /// </summary>
        public virtual void Display()
        {
        }

        /// <summary>
        /// Initializes the view.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Generates an instance of the output type based
        /// on the values in the UI and on the input.
        /// </summary>
        /// <returns>The output based on the UI</returns>
        public abstract override TO Generate();
    }
}