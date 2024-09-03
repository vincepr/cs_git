using CS_Git.Lib.GitObjectLogic.ObjTypes;

namespace CS_Git.Lib.GitObjectLogic;

public class User2
{
    private ISomething _thing;

    public void Run1()
    {
        _thing.DoSomething();
        _thing.DoAB();
    }
    
    public void Run2()
    {
        _thing.DoSomething();
        _thing.DoSomething();
    }
    
    public void Run3()
    {
        _thing.DoAB();
        _thing.DoAB();
    }
}