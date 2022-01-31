using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip battleMusicIntro;
    [SerializeField] private AudioClip battleMusicLoop;
    [SerializeField] private AudioClip buildMusicIntro;
    [SerializeField] private AudioClip buildMusicLoop;

    private Coroutine _coroutine;

    public void PlayBattleMusic()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
        _coroutine = StartCoroutine(PlayBattleMusicRoutine());
    }
    
    public void PlayBuildMusic()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
        _coroutine = StartCoroutine(PlayBuildMusicRoutine());
    }

    private IEnumerator PlayBattleMusicRoutine()
    {
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = battleMusicIntro;
        audioSource.Play();
        yield return new WaitForSeconds(battleMusicIntro.length);
        audioSource.clip = battleMusicLoop;
        audioSource.loop = true;
        audioSource.Play();
    }
    
    private IEnumerator PlayBuildMusicRoutine()
    {
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = buildMusicIntro;
        audioSource.Play();
        yield return new WaitForSeconds(buildMusicIntro.length);
        audioSource.clip = buildMusicLoop;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PlayOneShot(AudioClip audioClip)
    {
        sfxSource.PlayOneShot(audioClip);
    }
}
