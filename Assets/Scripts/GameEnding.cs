using UnityEngine;
using UnityEngine.SceneManagement;
public class GameEnding : MonoBehaviour
{
    public float fadeDuration=1f;
    public GameObject player;
    public float displayImageDuration;
    public CanvasGroup exitBackgroundImageCanvasGroup;
    public CanvasGroup caughtBackGroundImageCanvasGroup;
    public AudioSource caughtAudio;
    public AudioSource escapeAudio;
    bool m_IsPlayerCaught;
    bool m_IsPlayerAtExit;
    float m_Timer;
    bool  m_HasAudioPlay;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_IsPlayerAtExit)
        {
            EndLevel(exitBackgroundImageCanvasGroup,false,escapeAudio);
        }
        else if(m_IsPlayerCaught)
        {
            EndLevel(caughtBackGroundImageCanvasGroup,true,caughtAudio);
        }
    }
    public void CaughtPlayer()
    {
        m_IsPlayerCaught = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            m_IsPlayerAtExit = true;
        }
    }
    void EndLevel(CanvasGroup canvasGroup,bool dorestart,AudioSource audioSource)
    {
        if(!m_HasAudioPlay)
        {
            audioSource.Play();
            m_HasAudioPlay = true;
        }
        m_Timer += Time.deltaTime;
        canvasGroup.alpha = m_Timer / fadeDuration;
        if(m_Timer>fadeDuration+displayImageDuration)
        {
            if (dorestart)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
            Application.Quit();

            }
        }
    }
}
