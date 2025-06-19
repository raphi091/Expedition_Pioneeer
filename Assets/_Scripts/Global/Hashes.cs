using UnityEngine;


public static class AnimatorHashSet
{
    public static int MOVESPEED = Animator.StringToHash("MOVESPEED");
    public static int CROUCH = Animator.StringToHash("CROUCH");
    public static int DODGE = Animator.StringToHash("DODGE");

    public static int WEAPON = Animator.StringToHash("WEAPON");

    public static int ATTACK = Animator.StringToHash("ATTACK");
    public static int ATTACK_COMBO = Animator.StringToHash("ATTACK_COMBO");
    public static int SECONDARYATTACK = Animator.StringToHash("SECONDARYATTACK");
    public static int SECONDARYATTACK_COMBO = Animator.StringToHash("SECONDARYATTACK_COMBO");

    public static int GUARD = Animator.StringToHash("GUARD");

    public static int CHARGE = Animator.StringToHash("CHARGE");
    public static int CHARGE_LEVEL = Animator.StringToHash("CHARGE_LEVEL");
    public static int CHARGED_ATTACK = Animator.StringToHash("CHARGE_ATTACK");

    public static int DEATH = Animator.StringToHash("DEATH");
}
