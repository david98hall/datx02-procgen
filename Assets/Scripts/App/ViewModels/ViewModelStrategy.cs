using System;
using Interfaces;
using Services;

namespace App.ViewModels
{
    /// <summary>
    /// A view model for a strategy to generate something based on an input.
    /// </summary>
    /// <typeparam name="TI">The input type of the generation.</typeparam>
    /// <typeparam name="TO">The output type of the generation.</typeparam>
    [Serializable]
    public abstract class ViewModelStrategy<TI, TO> 
        : Strategy<TI, TO>, IDisplayable, ISubscriber<AppEvent>, IInitializable
    {

        /// <summary>
        /// The event bus that this view model can create events on and
        /// where it can listen for events from other view models.
        /// 
        /// When set, this view model is automatically unsubscribed
        /// from the previous one and subscribed to the new one.
        /// </summary>
        public virtual EventBus<AppEvent> EventBus
        {
            get => _eventBus;
            set
            {
                _eventBus?.Unsubscribe(this);
                _eventBus = value;
                _eventBus.Subscribe(this);
            }
        }
        
        private EventBus<AppEvent> _eventBus;
        
        /// <summary>
        /// Required constructor for initializing the underlying injector.
        /// </summary>
        protected ViewModelStrategy() : base(null)
        {
        }

        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public virtual void Initialize()
        {
        }
        
        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public virtual void Display()
        {
        }

        /// <summary>
        /// Generates an instance of the output type based
        /// on the values in the UI and on the input.
        /// </summary>
        /// <returns>The output based on the UI</returns>
        public abstract override TO Generate();

        public virtual void OnEvent(AppEvent eventId, object eventData)
        {
        }
    }
}