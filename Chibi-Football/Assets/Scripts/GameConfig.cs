public static class GameConfig
{
#if TRAINING_BUILD
    public static readonly bool IsTraining = true;
    public static readonly float BallReappearDelay = 0f;
    public static readonly float GoalPauseSeconds = 0f;
    public static readonly string SelfPlayBehaviorName = "SoccerTwos";
#elif EVALUATION_BUILD
    public static readonly bool IsTraining = true;
    public static readonly float BallReappearDelay = 0f;
    public static readonly float GoalPauseSeconds = 0f;
    public static readonly string SelfPlayBehaviorName = null;
#else
    public static readonly bool IsTraining = false;
    public static readonly float BallReappearDelay = 1f;
    public static readonly float GoalPauseSeconds = 0f;
    public static readonly string SelfPlayBehaviorName = null;
#endif
    public static readonly float InferenceExitDelaySeconds = 3f;
}
