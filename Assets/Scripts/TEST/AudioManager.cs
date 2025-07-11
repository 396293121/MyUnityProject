using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class AudioManagerTest : MonoBehaviour
{
    public static AudioManagerTest Instance;

    public AudioClip jumpSound;
    public AudioClip damageSound;

    private AudioSource audioSource;
    public AudioClip swordSwingSound;
    public AudioClip swordHitSound;
    public AudioClip enemyHurtSound;
    public AudioClip enemyDeathSound;
    public AudioClip enemyAttackSound;
    public AudioClip enemyChargeSound;
    public AudioClip bgm;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    private void Start()
    {
        PlayBGM(bgm);
    }
    private void PlayBGM(AudioClip bgm)
    {
        audioSource.clip = bgm;
        audioSource.loop = true;
        audioSource.Play();
    }
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}