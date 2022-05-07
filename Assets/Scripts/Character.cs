using UnityEngine;
using System.Linq;
using System.Text;

public class Character : CharExtension
{
    [SerializeField] private string _name;
    [SerializeField] private string _atk;
    [SerializeField] private string _maxHealth;

    public override string GetName => (string) GetVar("name");
    public override bool IsAlive => GetBoolVar("alive", true);
    public override int Health => GetIntVar("healthPoints", 100);

    private void Awake()
    {
        CreateVar("name", _name);
        CreateVar("alive", true);
        CreateVar("atk", _atk);
        CreateVar("healthPoints", _maxHealth);
    }

    public void takeDamage(object[] dmg){
        if (int.TryParse((string) dmg[0], out int damage))
        {
            CreateVar("healthPoints", Mathf.Max(Health - damage, 0));

            Debug.Log($"{_name} Health now is: {GetVar("healthPoints")}");
            if (GetIntVar("healthPoints") == 0)
            {
                Debug.Log($"{_name} is dead, combat is over!");
                CreateVar("alive", false);
            }
        }
    }

    StringBuilder sb = new StringBuilder();

    public void talk(object[] text){
        sb.Append($"{_name}: ");
        systemMessage(text);
    }

    public void systemMessage(object[] texts){
        foreach (var text in texts)
        {
            if (text.GetType().IsArray)
            {
                ((object[]) text)
                .Select(t => this.VarParser((string) t))
                .ToList().ForEach(t => sb.Append(t.ToString()));
            }
            else sb.Append(this.VarParser((string) text));
        };
        Debug.Log(sb);
        sb.Clear();
    }

    [ContextMenu("Die")]
    public void Die() => CreateVar("alive", false);
}
