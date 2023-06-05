using UnityEngine;
using System.Collections;

namespace Completed
{
    public class SoundManager : MonoBehaviour
    {
        public AudioSource efxSource;                   // 사운드 효과를 재생할 오디오 소스에 대한 참조를 드래그합니다.
        public AudioSource musicSource;                 // 음악을 재생할 오디오 소스에 대한 참조를 드래그합니다.
        public static SoundManager instance = null;     // 다른 스크립트에서 SoundManager의 함수를 호출할 수 있도록 합니다.
        public float lowPitchRange = .95f;              // 사운드 효과가 재생될 때 무작위로 변경될 수 있는 최소 피치입니다.
        public float highPitchRange = 1.05f;            // 사운드 효과가 재생될 때 무작위로 변경될 수 있는 최대 피치입니다.


        void Awake()
        {
            // SoundManager의 인스턴스가 이미 존재하는지 확인합니다.
            if (instance == null)
                // 만약 존재하지 않으면, 이 인스턴스를 설정합니다.
                instance = this;
            // 이미 인스턴스가 존재한다면:
            else if (instance != this)
                // 이 스크립트를 제거합니다. 이렇게 하면 싱글톤 패턴이 지켜져서 SoundManager의 인스턴스가 하나만 존재합니다.
                Destroy(gameObject);

            // 장면을 다시 로드할 때 SoundManager가 파괴되지 않도록 DontDestroyOnLoad를 설정합니다.
            DontDestroyOnLoad(gameObject);
        }


        // 단일 사운드 클립을 재생합니다.
        public void PlaySingle(AudioClip clip)
        {
            // efxSource 오디오 소스의 클립을 전달받은 클립으로 설정합니다.
            efxSource.clip = clip;

            // 클립을 재생합니다.
            efxSource.Play();
        }


        // RandomizeSfx는 여러 오디오 클립 중에서 임의로 선택하고 약간의 피치를 변경합니다.
        public void RandomizeSfx(params AudioClip[] clips)
        {
            // 전달받은 클립 배열의 길이 사이에서 무작위로 숫자를 생성합니다.
            int randomIndex = Random.Range(0, clips.Length);

            // 재생할 클립의 피치를 최소 피치와 최대 피치 범위 사이에서 무작위로 선택합니다.
            float randomPitch = Random.Range(lowPitchRange, highPitchRange);

            // 오디오 소스의 피치를 무작위로 선택된 피치로 설정합니다.
            efxSource.pitch = randomPitch;

            // 클립을 무작위로 선택된 인덱스의 클립으로 설정합니다.
            efxSource.clip = clips[randomIndex];

            // 클립을 재생합니다.
            efxSource.Play();
        }
    }
}
