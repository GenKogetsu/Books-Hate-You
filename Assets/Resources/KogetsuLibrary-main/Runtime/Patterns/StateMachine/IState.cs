namespace Kogetsu.Library.DesignPatternCore;

public interface IState { }

public interface IEnterState 
{ 
    void OnEnter(); 
}

public interface IExitState 
{ 
    void OnExit(); 
}

public interface IUpdateState 
{
    void OnUpdate(); 
}

public interface IFixedUpdateState 
{
    void OnFixedUpdate();
}
