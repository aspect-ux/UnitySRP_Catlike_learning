

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MusicMgr : BaseManager<MusicMgr>
{
    private AudioSource bkMusic = null;
    private float bkValue = 1;
    private float soundValue = 1;
    private GameObject soundObj = null;
    private List<AudioSource> soundList = new List<AudioSource>();

    public MusicMgr()
    {
        MonoMgr.GetInstance().AddUpdateListener(update);
    }

    private void update()
    {
        for (int i = soundList.Count - 1; i >= 0; i--)
        {
            if (!soundList[i].isPlaying)
            {
                GameObject.Destroy(soundList[i]);
                soundList.RemoveAt(i);
            }
        }
    }

    public void PlayBKMusic(string musicName)
    {
        if (bkMusic == null)
        {
             GameObject obj = new GameObject("BKMusic");
             bkMusic = obj.AddComponent<AudioSource>();
        }

        ResMgr.GetInstance().LoadAsync<AudioClip>("Music/bk" + musicName, (clip) =>
        {
            bkMusic.clip = clip;
            bkMusic.loop = true;
            bkMusic.volume = bkValue;
            bkMusic.Play();
        });
    }

    public void ChangeBKValue(float v)
    {
        bkValue = v;
        if(bkMusic==null)
            return;
        bkMusic.volume = bkValue;
    }

    public void PauseBKMusic()
    {
        if(bkMusic==null)
            return;
        bkMusic.Pause();
    }

    public void StopBkMusic()
    {
        if(bkMusic==null)
            return;
        bkMusic.Stop();
    }



    public void PlaySound(string musicName, bool isLoop, UnityAction<AudioSource> callback = null)
    {
        if (soundObj == null)
        {
            soundObj = new GameObject();
            soundObj.name = "Sounds";
        }

        AudioSource source = soundObj.AddComponent<AudioSource>();
        ResMgr.GetInstance().LoadAsync<AudioClip>("Music/Sounds/"+musicName, (clip) =>
        {
            source.clip = clip;
            source.loop = isLoop;
            source.volume = soundValue;
            source.Play();
            if (callback != null)
                callback(source);
        });
    }

    public void ChangeSoundValue(float value)
    {
        soundValue = value;
        for (int i = 0; i < soundList.Count; i++)
            soundList[i].volume = value;
    }

    public void StopSound(AudioSource source)
    {
        if (soundList.Contains(source))
        {
            soundList.Remove(source);
            source.Stop();
            GameObject.Destroy(source);
        }
    } 
}
