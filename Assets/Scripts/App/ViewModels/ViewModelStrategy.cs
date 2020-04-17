using Interfaces;

namespace App.ViewModels
{
    /// <summary>
    /// A view model for a strategy to generate something based on an input.
    /// </summary>
    /// <typeparam name="TI">The input type of the generation.</typeparam>
    /// <typeparam name="TO">The output type of the generation.</typeparam>
    public abstract class ViewModelStrategy<TI, TO> : Strategy<TI, TO>, IDisplayable, IInitializable
    {
        protected ViewModelStrategy() : base(null)
        {
        }

        /// <summary>
        /// Displays the view.
        /// </summary>
        public virtual void Display()
        {
        }

        /// <summary>
        /// Initializes the view model.
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