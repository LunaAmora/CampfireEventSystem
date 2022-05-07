using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

public static class TriggerHelper
{
    public static char ARG_SEPARATOR = ';';
    public static char COND_SEPARATOR = '/';
    public static char PRIORITY_SYMBOL = '$';
    public static char TAG_SYMBOL = '@';
    public static char VAR_SYMBOL = '#';
    public static char EVENT_SYMBOL = '*';
    
    public static List<List<string>> SplitInTrigger(string rawTrigger)
    {
        string[] splitTrigger = rawTrigger.Split(COND_SEPARATOR);

        return splitTrigger
            .Select((string str) => str.Split(ARG_SEPARATOR).ToList())
            .ToList();
    }

    public static object InvokeVar(this CharExtension actor, string varName)
    {
        try
        {
            FieldInfo z = actor.GetType().GetField(varName);
            object p = (object)actor;
            // Debug.Log("Invoked Value: " + z.GetValue(p));
            return z.GetValue(p);
        } 
        catch (Exception)
        {
            // Debug.Log($"Trying to find var: {varName}");
            if (actor.ContainsVar(varName))
            {
                return actor.GetVar(varName);
            } else return null;
        }
    }

    public static object VarParser(this CharExtension actor, string var)
    {
        if (VAR_SYMBOL.Equals(var[0]))
        {
            object invoked = actor.InvokeVar(var.Substring(1));
            if (invoked != null) return invoked;
        }
        return var;
    }

    public static object[] VarListParser(this CharExtension actor, List<string> vars)
    {
        return vars.Select(var => actor.VarParser(var)).ToArray();
    }

    public static void CallMethod(this EventAction eventAction)
    {
        CharExtension actor = eventAction.Caller;
        string tEvent = eventAction.TriggerEvent;
        object[] args = null;

        if (tEvent.Contains(COND_SEPARATOR))
        {
            List<List<string>> splitEvent = SplitInTrigger(tEvent);

            eventAction.SetEvent(splitEvent[0].First(str => !TAG_SYMBOL.Equals(str[0])));

            if (splitEvent[1] != null)
            {
                args = actor.VarListParser(splitEvent[1]);
            }
        } 
        else if (tEvent.Contains(ARG_SEPARATOR))
        {
            List<string> split = tEvent.Split(ARG_SEPARATOR).ToList();
            eventAction.SetEvent(split[0]);
            split.RemoveAt(0);
            args = actor.VarListParser(split);
        } 
        else if (eventAction.CallIfContainsEventSymbol())
        {
            return;
        }
        eventAction.CallMethodArgs(args);
    }

    public static bool CallIfContainsEventSymbol(this EventAction action)
    {
        CharExtension actor = action.Caller;
        string eventName = action.TriggerEvent;

        if (eventName.Contains(EVENT_SYMBOL))
        {
            if (actor.TriggerList.ContainsKey(eventName))
            {
                if (actor.GetInteractingActor() != null)
                {
                    actor.EvokeAction(eventName, actor.GetInteractingActor());
                }
                else actor.EvokeAction(eventName);
            }
            return true;
        }
        return false;
    }

    public static bool AlternateActor(this EventAction action)
    {
        return AlternateActor(action, null);
    }

    static bool AlternateActor(this EventAction action, object[] args)
    {
        if (action.TriggerEvent.Contains("oth_"))
        {
            if (action.Caller.GetInteractingActor() != null)
            {
                string str = action.TriggerEvent.Substring(4);
                if (args != null)
                {
                    str += string.Concat(args.ToList().Select(arg => $"{ARG_SEPARATOR}{arg}"));
                }
                action.Caller.GetInteractingActor().EvokeAction(str, action.Caller);
            }
            return true;
        }
        return false;
    }

    public static void CallMethodArgs(this EventAction action, object[] args)
    {
        if (AlternateActor(action, args)) return;

        CharExtension actor = action.Caller;
        string methodName = action.TriggerEvent;

        try
        {
            if (args != null)
            {
                Type[] types = new Type[]{ typeof(object[]) };
                MethodInfo methodInfo = actor.GetType().GetMethod(methodName, types);
                // Debug.Log($"{methodInfo.Name} found, will call with args: {string.Join(", ", args)}");
                methodInfo.Invoke(actor, new object[]{args});
            } else {
                MethodInfo methodInfo = actor.GetType().GetMethod(methodName, Type.EmptyTypes);
                // Debug.Log($"{methodInfo.Name} found");
                methodInfo.Invoke(actor, new object[]{});
            }
            // Debug.Log($"{methodName} Was invoked sucessfully");

            string actionToEvoke = $"on_{methodName}*";
            if (actor.TriggerList.ContainsKey(actionToEvoke))
            {
                actor.EvokeAction(actionToEvoke);
            }

            CharExtension other = actor.GetInteractingActor();
            if (other != null)
            {
                actionToEvoke = $"res_{methodName}*";
                if (other.TriggerList.ContainsKey(actionToEvoke))
                    other.EvokeAction(actionToEvoke);
            }
        }
        catch (Exception)
        {
            // Debug.Log($"Method {methodName} not found");
            action.SetEvent(methodName + EVENT_SYMBOL);
            action.CallIfContainsEventSymbol();
        }
    }

    public static void InvokeStringMethod(this CharExtension target, string methodName, object[] args, Type[] types)
    {
        // Debug.Log("Invoking Method: " + methodName);
        MethodInfo methodInfo = target.GetType().GetMethod(methodName, types);
        methodInfo.Invoke(target, args);
    }

    // Execute every event that has the given event name in the target actor
    public static void CallEvent(EventAction action)
    {
        if (AlternateActor(action)) return;

        List<Trigger> listOfTriggers = action.GetCallerEffectList();
        if (listOfTriggers == null) return;

        listOfTriggers
            .SelectMany(trigger => trigger.Effects)
            .ToList()
            .ForEach(effect =>
            {
                var effectName = effect[0];

                if (effect.Count() == 1)
                {
                    action.Execute(effectName, null);
                    return;
                }

                var effects = new List<string>(effect);
                effects.RemoveAt(0);
                action.Execute(effectName, effects);
            });
    }

    // Calls for a method search with the effect name and a list of string arguments
    public static void Execute(this EventAction action, string effectName, List<string> args)
    {
        action.SetEvent(effectName);
        object[] newArgs = null;

        if (args != null) newArgs = VarListParser(action.Caller, args);
        else if (action.CallIfContainsEventSymbol()) return;

        action.CallMethodArgs(newArgs);
    }
}
