using System;

namespace wmsMLC.General
{
    public class WeakEventHandler
    {
        private readonly WeakReference _weakReference;

        public WeakEventHandler(EventHandler handler, ref object weakEventStrongReferenceHolder,
            Action<EventHandler> unsubscribe)
        {
            var eventStrongReferenceHolder = new WeakEventStrongReferenceHolder(handler, this, unsubscribe);
            _weakReference = new WeakReference(eventStrongReferenceHolder);
            weakEventStrongReferenceHolder = eventStrongReferenceHolder;
        }

        public void Handle(object sender, EventArgs eventArgs)
        {
            var weakEventStrongReference = (WeakEventStrongReferenceHolder)this._weakReference.Target;

            if (weakEventStrongReference != null)
                weakEventStrongReference.EventHandler(sender, eventArgs);
        }
    }

    public class WeakEventStrongReferenceHolder
    {
        public WeakEventStrongReferenceHolder(
            EventHandler eventHandler,
            WeakEventHandler weakEventHandler,
            Action<EventHandler> unsubscribe)
        {
            EventHandler = eventHandler;
            WeakEventHandler = weakEventHandler;
            Unsubscribe = unsubscribe;
        }

        public EventHandler EventHandler { get; private set; }
        public WeakEventHandler WeakEventHandler { get; private set; }
        public Action<EventHandler> Unsubscribe { get; private set; }

        ~WeakEventStrongReferenceHolder()
        {
            Unsubscribe(WeakEventHandler.Handle);
        }
    }
}
