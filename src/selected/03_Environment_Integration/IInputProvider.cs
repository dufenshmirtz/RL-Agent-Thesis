public interface IInputProvider
{
    float GetAxis(string name);
    bool GetButtonDown(string name);
    bool GetButtonUp(string name);
    bool GetKeyDown(UnityEngine.KeyCode key);
    bool GetKeyUp(UnityEngine.KeyCode key);
    bool GetKey(UnityEngine.KeyCode key);   // NEW: support "is key held?"
}
