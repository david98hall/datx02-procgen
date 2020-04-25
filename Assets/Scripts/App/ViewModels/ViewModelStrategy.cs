using System;
using System.Threading;
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
    public abstract class ViewModelStrategy<TI, TO> : IGenerator<TO>, IDisplayable, ISubscriber<AppEvent>
    {
        
        public virtual CancellationToken CancelToken { get; set; }
        
        internal virtual IInjector<TI> Injector { get; set; }
        
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
                // Unsubscribe from the current event bus (if there is one)
                _eventBus?.Unsubscribe(this);
                
                // Subscribe to the new event bus
                _eventBus = value;
                _eventBus.Subscribe(this);
            }
        }
        
        private EventBus<AppEvent> _eventBus;

        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public abstract void Display();

        /// <summary>
        /// Generates an instance of the output type based
        /// on the values in the UI and on the input.
        /// </summary>
        /// <returns>The output based on the UI</returns>
        public abstract TO Generate();

        
        public virtual void OnEvent(AppEvent eventId, object eventData, object creator)
        {
            // No default event action
        }
    }
}