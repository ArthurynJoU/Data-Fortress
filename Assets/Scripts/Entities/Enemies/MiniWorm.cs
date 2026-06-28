/// <summary>
/// A secondary enemy spawned from a destroyed Worm. It continues towards the objective but cannot split any further.
/// </summary>
public class MiniWorm : Worm
{
    public override void Initialize()
    {
        base.Initialize();
        CanSplit = false;
    }
}