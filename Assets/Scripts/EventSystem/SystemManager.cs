using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;

public class SystemManager
{
    private static SystemManager _systemManagerInstance = new SystemManager();

    private Dictionary<string, CharExtension> _actorsList = new Dictionary<string, CharExtension>();
    private Queue<EventAction> _actionQueue = new Queue<EventAction>();
    private ExecutorService _queueProcess;
    private int _currentStackId =  0;

    public static SystemManager Instance() => _systemManagerInstance;
    public List<CharExtension> GetActorsList() => _actorsList.Values.ToList();
    public CharExtension GetActor(string name) => _actorsList[name];

    public void AddNewActor(CharExtension actor)
    {
        _actorsList[actor.GetName] = actor;
        actor.Manager = this;
    }

    public void AddToQueue(EventAction action)
    {
        if (_currentStackId != 0 && _currentStackId != action.StackId)
        {
            // Debug.Log($"Adding action with id: {action.StackId} to the queue");
            _actionQueue.Enqueue(action);
            return;
        }

        // Debug.Log($"Adding action with id: {action.StackId} to the ExecutorService");
        _currentStackId = action.StackId;
        ExecuteActionThread(action);
    }

    public void ActionCompleted(int id)
    {
        _queueProcess.Enqueue(new Thread(() =>
        {
            if (_queueProcess.Count == 0 && id == _currentStackId)
            {
                if (_actionQueue.Count > 0)
                {
                    // Debug.Log("Moving to next queue");
                    MoveQueue();
                    return;
                }
                
                // Debug.Log("No more actions to execute");
                _currentStackId = 0;
                _queueProcess.Shutdown();
                _queueProcess = null;
            }
            // Debug.Log($"Queue is not done, or ids are diferent {id} != {currentStackId}");
        }));
    }

    public SystemManager EvokeAction(string actorName, string tEvent)
    {
        EvokeAction(actorName, tEvent, null);
        return this;
    }

    public SystemManager EvokeAction(string actorName, string tEvent, string targetName)
    {
        CharExtension actor = _actorsList[actorName];
        CharExtension target;

        if (targetName == null) target = null;
        else 
        {
            target = _actorsList[targetName];
            target.ActionStackId = 0;
        }

        actor.ActionStackId = 0;
        actor.EvokeAction(tEvent, target);
        return this;
    }

    public SystemManager InitializeActors()
    {
        foreach (CharExtension actor in _actorsList.Values)
        {
            EvokeAction(actor.GetName, "initialize*");
            actor.SortTriggersByPriority();
        }
        return this;
    }

    public SystemManager AddTriggers(string[] triggers, Predicate<CharExtension> predicate)
    {
        _actorsList.Values.ToList().ForEach(actor =>
        {
            if (predicate(actor))
            {
                actor.AddTriggers(triggers);
            }
        });
        return this;
    }

    public SystemManager AddTriggers(string actorName, string[] triggers)
    {
        if (actorName.Equals("All")) return AddTriggers(triggers);
        _actorsList[actorName].AddTriggers(triggers);
        return this;
    }

    public SystemManager AddTriggers(string[] triggers)
    {
        foreach(var entry in _actorsList)
        {
            entry.Value.AddTriggers(triggers);
        }
        return this;
    }

    public SystemManager AddTrigger(string trigger)
    {
        foreach(var entry in _actorsList)
        {
            entry.Value.AddTrigger(trigger);
        }
        return this;
    }

    public SystemManager AddTrigger(string name, string trigger)
    {
        _actorsList[name].AddTrigger(trigger);
        return this;
    }

    private void ExecuteActionThread(EventAction actionToExecute)
    {
        if (_queueProcess == null)
        {
            _queueProcess = new ExecutorService();
        }
        
        _queueProcess.Enqueue(new Thread(new ThreadStart(actionToExecute.run)));
    }

    private void MoveQueue()
    {
        EventAction action = _actionQueue.Dequeue();
        _currentStackId = action.StackId;
        ExecuteActionThread(action);
        // Debug.Log($"Moving to Action queue with id: {currentStackId}");
    }

    private class ExecutorService : Queue<Thread>
    {
        private Thread _current;
        private bool _shutdown;
        public void Shutdown() => _shutdown = true;

        public ExecutorService()
        {
            new Thread(() =>
            {
                while(!_shutdown)
                {
                    if (Count > 0 && (_current == null || !_current.IsAlive))
                    {
                        // Debug.Log($"Dequeueing ExecutorService");
                        _current = Dequeue();
                        _current.Start();
                    }
                }
            }).Start();
        }
    }
}