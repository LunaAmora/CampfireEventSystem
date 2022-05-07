using System.Collections.Generic;
using System;

public class Trigger : IComparable<Trigger>
{
    private List<List<string>> _effects = new List<List<string>>();
    private List<string> _tags = new List<string>();
    private int _priority = 0;

    public List<List<string>> Effects => _effects;
    public List<string> Tags => _tags;
    public string Condition {get; private set;}

    public bool hasTag(string str) => _tags.Contains(TriggerHelper.TAG_SYMBOL + str);
    public bool isUnique => hasTag("unique");

    public Trigger(string stringList)
    {
        if (stringList.Equals(string.Empty)) return;
        List<List<string>> listedTriggers = TriggerHelper.SplitInTrigger(stringList);

        // listedTriggers.ForEach(a => a.ForEach(s => Debug.Log($"------ {s}")));

        listedTriggers[0].ForEach((Action<string>)((string str) => {
            char atZero = str[0];
            if (TriggerHelper.PRIORITY_SYMBOL.Equals(atZero))
            {
                int p = 0;
                int.TryParse(str[1].ToString(), out p);
                _priority = p;
            } 
            else if (!TriggerHelper.TAG_SYMBOL.Equals(atZero))
            {
                this.Condition = str;
            } 
            else
            {
                _tags.Add(str);
            }
        }));

        _effects.Add(listedTriggers[1]);
    }

    public Trigger(string condition, List<List<string>> effects, List<string> tags)
    {
        Condition = condition;
        _effects.AddRange(effects);
        _tags.AddRange(tags);
    }

    public Trigger(string condition, List<List<string>> effects)
    {
        Condition = condition;
        _effects.AddRange(effects);
    }

    // Effect adders
    public void AddEffect(string stringList)
    {
        List<List<string>> listedTriggers = TriggerHelper.SplitInTrigger(stringList);
        if (listedTriggers[0][0].Equals(Condition))
        {
            _effects.Add(listedTriggers[1]);
        }
    }

    public void AddEffect(List<string> effect) => _effects.Add(effect);
    public void AddEffects(List<List<string>> effectList) => _effects.AddRange(effectList);

    public int CompareTo(Trigger other)
    {
        if (other != null) {
            return _priority.CompareTo(other._priority);
        }
        throw new Exception("Cannot compare to null");
    }
}