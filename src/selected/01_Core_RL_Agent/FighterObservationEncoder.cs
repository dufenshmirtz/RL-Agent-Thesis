using System.Collections.Generic;
using UnityEngine;

public static class FighterObservationEncoder
{
    public static float[] Encode(
        Character self,
        Character opp,
        float relXScale,
        float relYScale,
        float velScale,
        int totalCharacterCount)
    {
        int obsSize = GetObservationSize(totalCharacterCount);
        List<float> obs = new List<float>(obsSize);

        if (self == null || opp == null)
        {
            for (int i = 0; i < obsSize; i++)
                obs.Add(0f);

            return obs.ToArray();
        }

        var rb = self.GetComponent<Rigidbody2D>();
        var orb = opp.GetComponent<Rigidbody2D>();

        Vector2 vel = rb ? rb.velocity : Vector2.zero;
        Vector2 ovel = orb ? orb.velocity : Vector2.zero;
        Vector2 rel = (Vector2)(opp.transform.position - self.transform.position);

        float nx = Mathf.Clamp(rel.x / relXScale, -1f, 1f);
        float ny = Mathf.Clamp(rel.y / relYScale, -1f, 1f);
        float absDxNorm = Mathf.Clamp(Mathf.Abs(rel.x) / relXScale, 0f, 1f);
        float absDyNorm = Mathf.Clamp(Mathf.Abs(rel.y) / relYScale, 0f, 1f);

        float facingSign = Mathf.Sign(self.transform.localScale.x);
        float oppDirSign = Mathf.Sign(rel.x);
        float facingCorrectly = (facingSign == oppDirSign) ? 1f : 0f;

        // relative position
        obs.Add(nx);
        obs.Add(ny);

        // absolute spacing helpers
        obs.Add(absDxNorm);
        obs.Add(absDyNorm);

        // velocities
        obs.Add(Mathf.Clamp(vel.x / velScale, -1f, 1f));
        obs.Add(Mathf.Clamp(vel.y / velScale, -1f, 1f));
        obs.Add(Mathf.Clamp(ovel.x / velScale, -1f, 1f));
        obs.Add(Mathf.Clamp(ovel.y / velScale, -1f, 1f));

        // health
        obs.Add(self.GetCurrentHealth() / 100f);
        obs.Add(opp.GetCurrentHealth() / 100f);

        // one-hot character ids
        AddOneHot(obs, self.characterID, totalCharacterCount);
        AddOneHot(obs, opp.characterID, totalCharacterCount);

        // self state
        obs.Add(B(self.IsGrounded));
        obs.Add(B(self.IsBlocking));
        obs.Add(B(self.IsCasting));
        obs.Add(B(self.IsStunned));
        obs.Add(B(self.IsKnocked));
        obs.Add(B(self.IsCharging));
        obs.Add(B(self.IsCharged));
        obs.Add(B(self.OnAbilityCD));
        obs.Add(self.AbilityCooldown01);
        obs.Add(B(self.CanCast));
        obs.Add(B(self.CanParry));
        obs.Add(B(self.LightAttacking));
        obs.Add(B(self.HeavyAttacking));
        obs.Add(B(self.Parrying));

        // self disabled flags
        obs.Add(B(self.QuickDisabled));
        obs.Add(B(self.HeavyDisabled));
        obs.Add(B(self.BlockDisabled));
        obs.Add(B(self.SpecialDisabled));
        obs.Add(B(self.ChargeDisabled));
        obs.Add(B(self.JumpDisabled));

        // opponent state
        obs.Add(B(opp.IsGrounded));
        obs.Add(B(opp.IsBlocking));
        obs.Add(B(opp.IsCasting));
        obs.Add(B(opp.IsStunned));
        obs.Add(B(opp.IsKnocked));
        obs.Add(B(opp.IsCharging));
        obs.Add(B(opp.IsCharged));
        obs.Add(B(opp.OnAbilityCD));
        obs.Add(opp.AbilityCooldown01);
        obs.Add(B(opp.CanCast));
        obs.Add(B(opp.CanParry));
        obs.Add(B(opp.LightAttacking));
        obs.Add(B(opp.HeavyAttacking));
        obs.Add(B(opp.Parrying));

        // facing hints
        obs.Add(oppDirSign);
        obs.Add(facingSign);
        obs.Add(facingCorrectly);

        return obs.ToArray();
    }

    public static int GetObservationSize(int totalCharacterCount)
    {
        // 2  relative position
        // 2  absolute spacing helpers
        // 4  velocities
        // 2  health
        // N  self one-hot
        // N  opp one-hot
        // 14 self state
        // 6  self disabled flags
        // 14 opp state
        // 3  facing hints
        return 2 + 2 + 4 + 2 + totalCharacterCount + totalCharacterCount + 14 + 6 + 14 + 3;
    }

    private static void AddOneHot(List<float> obs, int index, int count)
    {
        for (int i = 0; i < count; i++)
            obs.Add(i == index ? 1f : 0f);
    }

    private static float B(bool value) => value ? 1f : 0f;
}