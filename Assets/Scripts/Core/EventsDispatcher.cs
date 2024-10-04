using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public interface IDispatcherEvent {}
public interface IEventsDispatcherClient {}
    
public class EventsDispatcher
{
    public void Dispatch(IDispatcherEvent e)
    {
        Type type = e.GetType();

        if (!_callbacks.ContainsKey(type))
        {
            return;
        }
            
        foreach (Action<IDispatcherEvent> callback in _callbacks[type].Values)
        {
            callback(e);
        }
    }

    public void Register<T>(IEventsDispatcherClient client, Action<T> callback)
    {
        Type type = typeof(T);

        if (!_callbacks.ContainsKey(type))
        {
            _callbacks.Add(type,  new Dictionary<IEventsDispatcherClient, Action<IDispatcherEvent>>());
        }
            
        Assert.IsFalse(_callbacks[type].ContainsKey(client), "Client already has EventsDispatcher registration");
            
        _callbacks[type].Add(client, e => callback((T)e));
    }
        
    public void Unregister<T>(IEventsDispatcherClient client)
    {
        Type type = typeof(T);

        if (_callbacks.ContainsKey(type) && _callbacks[type].ContainsKey(client))
        {
            _callbacks[type].Remove(client);
        }
    }

    private readonly Dictionary<Type, Dictionary<IEventsDispatcherClient, Action<IDispatcherEvent>>> _callbacks = new ();
}