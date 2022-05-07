using System.Collections.Generic;
using static TriggerHelper;
using UnityEngine;
using System.Linq;
using System;

public abstract class CharExtension : MonoBehaviour 
{
    private Dictionary<string, List<Trigger>> _triggerList = new Dictionary<string, List<Trigger>>();
    private Dictionary<string, object> _vars = new Dictionary<string, object>();
    private static System.Random _rand = new System.Random();
    private int _actionStackId;

    public CharExtension LastInteractedWith {get; set;}
    public SystemManager Manager {get; set;}
    public EventAction Action {get; set;}

    public Dictionary<string, List<Trigger>> TriggerList => _triggerList;

    public abstract string GetName { get; }
    public abstract bool IsAlive { get; }
    public abstract int Health { get; }

    public int ActionStackId
    {
        get
        {
            if (_actionStackId != 0)
            {
                return _actionStackId;
            }
            return (_rand.Next(100000));
        }
        set => _actionStackId = value;
    }

    public void AddTrigger(Trigger trigger)
    {
        if (trigger == null) return;
        string triggerName = trigger.Condition;
        List<List<string>> effects = trigger.Effects;

        if (_triggerList.ContainsKey(triggerName))
        {
            foreach (Trigger triggerInList in _triggerList[triggerName])
            {
                if (triggerInList.Equals(trigger))
                {
                    if (!trigger.isUnique)
                    {
                        triggerInList.AddEffects(effects);
                    }
                    return;
                }
            }
            _triggerList[triggerName].Add(trigger);
            return;
        }
        _triggerList[triggerName] = new List<Trigger>(){{trigger}};
    }

    public void AddTrigger(string str) => AddTrigger(new Trigger(str));

    public void AddTriggers(string[] str)
    {
        foreach (string s in str)
        {
            AddTrigger(new Trigger(s));
        }
    }

    // Dictionary variables manipulation and int parser
    public void storeVar(object[] data)
    {
        try 
        {
            if (data.Length > 2)
            {
                object[] newVarArray = new object[data.Length-1];
                Array.Copy(data, 1, newVarArray, 0, data.Length - 1);
                CreateVar((string) data[0], newVarArray);
            }
            else
            {
                CreateVar((string) data[0], data[1]);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void CreateVar(string var, object obj) => _vars[var] = obj;
    public bool ContainsVar(string var) => _vars.ContainsKey(var);

    public int GetIntVar(string var) => GetIntVar(var, 0);
    
    public int GetIntVar(string var, int defaultValue)
    {
        var v = GetVar(var, defaultValue);

        if(v != null)
        {
            int.TryParse(v.ToString(), out defaultValue);
        }

        return defaultValue;
    }

    public bool GetBoolVar(string var, bool defaultValue)
    {
        var v = GetVar(var, defaultValue);

        if(v != null)
        {
            bool.TryParse(v.ToString(), out defaultValue);
        }

        return defaultValue;
    }

    public object GetVar(string var) => GetVar(var, null);
    public object GetVar(string var, object defaultValue)
    {
        if(ContainsVar(var))
        {
            // Debug.Log($"Found var value: {vars[var]}");
            return _vars[var];
        }
        // Debug.Log($"Actor does not have var {var} set");
        return defaultValue;
    }
    
    public void EvokeAction(string tEvent) => EvokeAction(tEvent, null);

    public void EvokeAction(string tEvent, CharExtension target)
    {
        if (!IsAlive) return;
        string[] splitEventName = tEvent.Split(COND_SEPARATOR);
        bool searchClassMethods = !splitEventName[0].Contains(EVENT_SYMBOL);

        Manager.AddToQueue(target == null ?
            NewAction(tEvent, searchClassMethods) :
            NewInteraction(tEvent, target, searchClassMethods));
    }

    public CharExtension GetInteractingActor()
    {
        if (Action == null || Action.OppositeActor(this) == null)
        {
            return LastInteractedWith;
        }
        return Action.OppositeActor(this);
    }

    public void ActionCompleted(int stackId) => Manager.ActionCompleted(stackId);

    public void SortTriggersByPriority()
    {
        _triggerList.ToList().ForEach(entry => 
        {
            List<Trigger> sortedList = new List<Trigger>(entry.Value);
            sortedList.Sort();
            _triggerList[entry.Key] = sortedList;
        });
    }

    private EventAction NewAction(string tEvent, bool searchClassMethods)
    {
        return new EventAction(this, tEvent, searchClassMethods, ActionStackId);
    }

    private EventAction NewInteraction(string tEvent, CharExtension target, bool searchClassMethods)
    {
        return new Interaction(this, target, tEvent, searchClassMethods, ActionStackId);
    }
}