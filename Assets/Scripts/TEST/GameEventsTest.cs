using UnityEngine.Events;

    public class GameEventsTest
    {
            public static UnityEvent<float> OnPlayerTakeDamage = new UnityEvent<float>();
            public static UnityEvent onSkill = new UnityEvent();
            public static UnityEvent onSkillEnd = new UnityEvent();
            public static UnityEvent onPlayerDie = new UnityEvent();

    }