using System.Collections.Generic;

public class EventAction
{
    public CharExtension Caller {get; private set;}
    public string TriggerEvent {get; set;}
    public int StackId {get; private set;}

    private bool _searchClassMethods;

    public void SetEvent(string triggerEvent) => TriggerEvent = triggerEvent;

    public EventAction(CharExtension caller, string triggerEvent, bool searchClassMethods, int stackId)
    {
        TriggerEvent = triggerEvent;
        StackId = stackId;
        Caller = caller;

        _searchClassMethods = searchClassMethods;
    }

    public virtual CharExtension OppositeActor(CharExtension methodCaller)
    {
        return null;
    }

    public List<Trigger> GetCallerEffectList() => Caller.TriggerList[TriggerEvent];

    public virtual void run()
    {
        if (Caller.IsAlive)
        {
            // Debug.Log(StackId + " -> Calling: " + TriggerEvent);
            Caller.ActionStackId = StackId;
            Caller.Action = this;
            if (_searchClassMethods)
            {
                TriggerHelper.CallMethod(this);
            }
            else
            {
                TriggerHelper.CallEvent(this);
            }
        }
        // Debug.Log(StackId + " -> Ending call: " + TriggerEvent);
        Caller.ActionCompleted(StackId);
    }
}