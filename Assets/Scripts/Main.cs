using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class  Main : MonoBehaviour
{
    [SerializeField] private List<Character> _characters = new List<Character>();
    private SystemManager _triggerSystem = SystemManager.Instance();

    public void Start()
    {
        _characters.ForEach(actor => _triggerSystem.AddNewActor(actor));

        new Dictionary<string, string[]>()
        {
            {"All", new []{
                "basicAttack*;$2/oth_takeDamage;#atk",
                "basicAttack*;$1/systemMessage;#name; is attacking opponent with ;#atk; damage.",
                "on_takeDamage*;@retaliation/basicAttack*",
            }},
            {_characters[0].GetName, new []{
                "initialize*/storeVar;taunt;Get ready for the combat!",
                "combatBegin*/basicAttack*",
            }},
            {_characters[1].GetName, new []{
                "initialize*/storeVar;motherSpeech;I am ;#name; mama!",
                "res_talk*/talk;#motherSpeech",
            }}
        }.ToList().ForEach(pair => _triggerSystem.AddTriggers(pair.Key, pair.Value));

        _triggerSystem.InitializeActors();
        _triggerSystem.EvokeAction(_characters[0].GetName, "talk/#taunt", _characters[1].GetName);
        _triggerSystem.EvokeAction(_characters[0].GetName, "combatBegin*", _characters[1].GetName);
    }
}
