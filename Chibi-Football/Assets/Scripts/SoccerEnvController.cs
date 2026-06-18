using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class SoccerEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public AgentSoccer Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    public GameObject ball;
    [HideInInspector]
    public Rigidbody ballRb;
    Vector3 m_BallStartingPos;

    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    private SoccerSettings m_SoccerSettings;
    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;

    private int m_ResetTimer;
    private bool m_BallGenerationStopped;
    private bool m_GoalPending;

    void Start()
    {
        m_SoccerSettings = FindAnyObjectByType<SoccerSettings>();
        m_BlueAgentGroup = new SimpleMultiAgentGroup();
        m_PurpleAgentGroup = new SimpleMultiAgentGroup();
        ballRb = ball.GetComponent<Rigidbody>();
        m_BallStartingPos = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);
        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            if (item.Agent.team == Team.Blue)
                m_BlueAgentGroup.RegisterAgent(item.Agent);
            else
                m_PurpleAgentGroup.RegisterAgent(item.Agent);
        }
        ResetScene();
    }

    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_BlueAgentGroup.GroupEpisodeInterrupted();
            m_PurpleAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

    public void GoalTouched(Team scoredTeam)
    {
        if (m_GoalPending) return;
        m_GoalPending = true;

        if (scoredTeam == Team.Blue)
        {
            ScoreManager.instance.AddBluePoint();
            m_BlueAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_PurpleAgentGroup.AddGroupReward(-1);
        }
        else
        {
            ScoreManager.instance.AddPurplePoint();
            m_PurpleAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_BlueAgentGroup.AddGroupReward(-1);
        }
        m_PurpleAgentGroup.EndGroupEpisode();
        m_BlueAgentGroup.EndGroupEpisode();

        if (GameConfig.GoalPauseSeconds > 0f)
            StartCoroutine(DelayedGoalReset());
        else
            ResetScene();
    }

    public void ResetBall()
    {
        if (m_BallGenerationStopped) return;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        if (GameConfig.BallReappearDelay == 0f)
        {
            ball.transform.position = m_BallStartingPos + new Vector3(
                Random.Range(-2.5f, 2.5f), 0f, Random.Range(-2.5f, 2.5f));
        }
        else
        {
            var newPosition = m_BallStartingPos + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            StartCoroutine(DisappearAndReappearCoroutine(newPosition));
        }
    }

    public void ResetScene()
    {
        m_ResetTimer = 0;
        m_GoalPending = false;

        foreach (var item in AgentsList)
        {
            var newStartPos = item.Agent.initialPos;
            if (GameConfig.IsTraining)
                newStartPos += new Vector3(Random.Range(-5f, 5f), 0f, 0f);
            var newRot = Quaternion.Euler(0, item.Agent.rotSign * Random.Range(80.0f, 100.0f), 0);
            item.Agent.transform.SetPositionAndRotation(newStartPos, newRot);
            item.Rb.linearVelocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        ResetBall();
    }

    public void StopBallGeneration()
    {
        if (!GameConfig.IsTraining)
        {
            m_BallGenerationStopped = true;
            ball.SetActive(false);
        }
    }

    private IEnumerator<object> DelayedGoalReset()
    {
        yield return new WaitForSeconds(GameConfig.GoalPauseSeconds);
        if (!m_BallGenerationStopped)
            ResetScene();
    }

    private IEnumerator<object> DisappearAndReappearCoroutine(Vector3 newPosition)
    {
        ball.SetActive(false);
        yield return new WaitForSeconds(GameConfig.BallReappearDelay);
        if (m_BallGenerationStopped) yield break;
        ball.transform.position = newPosition;
        ball.SetActive(true);
    }
}
