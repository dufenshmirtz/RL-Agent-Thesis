using UnityEngine;

public class AIInputProvider : IInputProvider
{
    public struct Command
    {
        public int moveX;          // -1,0,1
        public bool jump;          // hold
        public bool drop;          // hold
        public bool light;         // press (edge)
        public bool heavy;         // press (edge)
        public bool blockHold;     // hold
        public bool special;       // press (edge)
        public bool chargeHold;    // hold
        public bool chargeRelease; // one-frame release
        public bool parry;         // press (edge)
    }

    private Command current, last;
    private readonly string playerSuffix;

    // Movement keys (from CharacterSetup)
    private KeyCode leftKey, rightKey, upKey, downKey;
    // Action keys (from CharacterSetup)
    private KeyCode lightKey, heavyKey, blockKey, abilityKey, chargeKey, parryKey;

    public AIInputProvider(string playerSuffix)
    {
        this.playerSuffix = playerSuffix;
    }

    // Reset input state after binding a character.
    public void SetKeys(
        KeyCode left, KeyCode right, KeyCode up, KeyCode down,
        KeyCode light, KeyCode heavy, KeyCode block, KeyCode ability, KeyCode charge, KeyCode parry)
    {
        leftKey = left; rightKey = right; upKey = up; downKey = down;
        lightKey = light; heavyKey = heavy; blockKey = block; abilityKey = ability; chargeKey = charge; parryKey = parry;
    }

    // Agent calls this every decision
    public void Apply(Command cmd)
    {
        last = current;
        current = cmd;
    }

    // ---------- IInputProvider ----------
    public float GetAxis(string name)
    {
        if (name == "Horizontal" + playerSuffix)
            return Mathf.Clamp(current.moveX, -1, 1);
        if (name == "Vertical" + playerSuffix)
        {
            if (current.drop) return -1f;
            if (current.jump) return  1f;
            return 0f;
        }
        return 0f;
    }

    // Controller-axis access required by the shared input interface.
    public bool GetButtonDown(string name)
    {
        if (name == "QuickAttack" + playerSuffix)  return current.light  && !last.light;
        if (name == "HeavyAttack" + playerSuffix)  return current.heavy  && !last.heavy;
        if (name == "Block" + playerSuffix)        return current.blockHold && !last.blockHold;
        if (name == "Spell" + playerSuffix)        return current.special && !last.special;
        if (name == "ChargeAttack" + playerSuffix) return (current.chargeHold && !last.chargeHold); 
        return false;
    }

    public bool GetButtonUp(string name)
    {
        if (name == "Block" + playerSuffix)        return !current.blockHold && last.blockHold;

        // IMPORTANT: allow explicit release command OR key-up transition
        if (name == "ChargeAttack" + playerSuffix) 
            return current.chargeRelease || (!current.chargeHold && last.chargeHold);

        return false;
    }

    public bool GetKeyDown(KeyCode key)
    {
        // Movement edges
        if (key == upKey)   return current.jump && !last.jump;
        if (key == downKey) return current.drop && !last.drop;

        // Action edges
        if (key == lightKey)  return current.light  && !last.light;
        if (key == heavyKey)  return current.heavy  && !last.heavy;
        if (key == blockKey)  return current.blockHold && !last.blockHold;
        if (key == abilityKey)return current.special && !last.special;
        if (key == chargeKey) return current.chargeHold && !last.chargeHold;
        if (key == parryKey)  return current.parry && !last.parry;

        return false;
    }

    public bool GetKeyUp(KeyCode key)
    {
        if (key == upKey)    return !current.jump && last.jump;
        if (key == downKey)  return !current.drop && last.drop;

        if (key == blockKey) return !current.blockHold && last.blockHold;

        // IMPORTANT: allow explicit release command OR key-up transition
        if (key == chargeKey) 
            return current.chargeRelease || (!current.chargeHold && last.chargeHold);

        return false;
    }

    public bool GetKey(KeyCode key)
    {
        if (key == leftKey)   return current.moveX < 0;
        if (key == rightKey)  return current.moveX > 0;
        if (key == upKey)     return current.jump;
        if (key == downKey)   return current.drop;

        if (key == blockKey)  return current.blockHold;
        if (key == chargeKey) return current.chargeHold;

        return false;
    }
}
