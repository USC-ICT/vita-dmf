using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AGGuiCircularAreaGraph : MonoBehaviour
{
    [Range(0, 4)] public float m_closingScore = 0f;
    [Range(0, 4)] public float m_activeListeningScore = 0f;
    [Range(0, 4)] public float m_selfPromotingScore = 0f;
    [Range(0, 4)] public float m_interviewResponseScore = 0f;
    [Range(0, 4)] public float m_firstImpressionScore = 0f;
    public float m_overallScore = 0f;

    public RectTransform m_closingArea;
    public RectTransform m_activeListeningArea;
    public RectTransform m_selfPromotingArea;
    public RectTransform m_interviewResponseArea;
    public RectTransform m_firstImpressionsArea;
    public Text m_closingScoreText;
    public Text m_activeListeningScoreText;
    public Text m_selfPromotingText;
    public Text m_interviewResponseText;
    public Text m_firstImpressionText;
    public Text m_overallScoreText;

    void Update()
    {
        //SetScores();
    }

    public void SetScores()
    {
        float m_closingPercentage = (m_closingScore + 1) / 5;
        float m_activeListeningPercentage = (m_activeListeningScore + 1) / 5;
        float m_selfPromotingPercentage = (m_selfPromotingScore + 1) / 5;
        float m_interviewResponsePercentage = (m_interviewResponseScore + 1) / 5;
        float m_firstImpressionPercentage = (m_firstImpressionScore + 1) / 5;
        m_overallScore = (m_closingScore + m_activeListeningScore + m_selfPromotingScore + m_interviewResponseScore + m_firstImpressionScore) / 5;

        m_closingArea.localScale = new Vector3(m_closingPercentage, m_closingPercentage, m_closingArea.localScale.z);
        m_activeListeningArea.localScale = new Vector3(m_activeListeningPercentage, m_activeListeningPercentage, m_activeListeningArea.localScale.z);
        m_selfPromotingArea.localScale = new Vector3(m_selfPromotingPercentage, m_selfPromotingPercentage, m_selfPromotingArea.localScale.z);
        m_interviewResponseArea.localScale = new Vector3(m_interviewResponsePercentage, m_interviewResponsePercentage, m_interviewResponseArea.localScale.z);
        m_firstImpressionsArea.localScale = new Vector3(m_firstImpressionPercentage, m_firstImpressionPercentage, m_firstImpressionsArea.localScale.z);

        m_closingScoreText.text         = m_closingScore == -1              ? "--" : m_closingScore.ToString();
        m_activeListeningScoreText.text = m_activeListeningScore == -1      ? "--" : m_activeListeningScore.ToString();
        m_selfPromotingText.text        = m_selfPromotingScore == -1        ? "--" : m_selfPromotingScore.ToString();
        m_interviewResponseText.text    = m_interviewResponseScore == -1    ? "--" : m_interviewResponseScore.ToString();
        m_firstImpressionText.text      = m_firstImpressionScore == -1      ? "--" : m_firstImpressionScore.ToString();
        m_overallScoreText.text         = m_overallScore == -1              ? "--" : m_overallScore.ToString();
    }

    public void SetScores(float closingScore, float activeListeningScore, float selfPromotingScore, float interviewResponseScore, float firstImpressionScore)
    {
        m_closingScore = closingScore;
        m_activeListeningScore = activeListeningScore;
        m_selfPromotingScore = selfPromotingScore;
        m_interviewResponseScore = interviewResponseScore;
        m_firstImpressionScore = firstImpressionScore;

        SetScores();
    }
}
