﻿namespace App
{
    /// <summary>
    /// Event identifications sent between view models via the AppController.
    /// </summary>
    public enum AppEvent
    {
        /// <summary>
        /// The id of an event, regarding the broadcast of all relevant values.
        /// The data type of this event is redundant, can be null.
        /// </summary>
        Broadcast,
        
        /// <summary>
        /// The id of an event, regarding an update of the noise map size.
        /// The data type of this event type is a ValueTuple of two floats, i.e., (width, height).
        /// </summary>
        UpdateNoiseMapSize
    }

}