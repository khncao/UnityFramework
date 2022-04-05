
namespace m4k.AI {
public interface IState {
    int priority { get; }
    StateProcessor processor { get; }
    void OnEnter(StateProcessor processor);
    bool OnUpdate();
    void OnExit();
}
}