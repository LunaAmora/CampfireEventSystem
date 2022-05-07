public class Interaction : EventAction
{
    private CharExtension _target;

    public Interaction(CharExtension caller, CharExtension target, string tEvent, bool eventOnly, int stackId)
        : base(caller, tEvent, eventOnly, stackId) => _target = target;

    public override CharExtension OppositeActor(CharExtension methodCaller)
    {
        if (Caller.Equals(methodCaller))
            return _target;
        return Caller;
    }

    public override void run()
    {
        if (_target != null)
        {
            Caller.LastInteractedWith = _target;
            _target.Action = this;
            _target.LastInteractedWith = Caller;
            _target.ActionStackId = StackId;
        }
        base.run();
    }
}