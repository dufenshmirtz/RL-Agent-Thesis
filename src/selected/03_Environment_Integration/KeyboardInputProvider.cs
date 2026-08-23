using UnityEngine;

public class KeyboardInputProvider : IInputProvider
{
    public float GetAxis(string name) => Input.GetAxis(name);
    public bool GetButtonDown(string name) => Input.GetButtonDown(name);
    public bool GetButtonUp(string name) => Input.GetButtonUp(name);
    public bool GetKeyDown(KeyCode key) => Input.GetKeyDown(key);
    public bool GetKeyUp(KeyCode key) => Input.GetKeyUp(key);
    public bool GetKey(KeyCode key) => Input.GetKey(key); // NEW
}

