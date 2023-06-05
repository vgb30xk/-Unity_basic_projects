using UnityEngine;
using System.Collections;

namespace Completed
{
    public class Loader : MonoBehaviour
    {
        public GameObject gameManager;          // 인스턴스화할 GameManager 프리팹.
        public GameObject soundManager;         // 인스턴스화할 SoundManager 프리팹.


        void Awake()
        {
            // 이미 정적 변수 GameManager.instance에 GameManager가 할당되었는지 또는 아직 null인지 확인합니다.
            if (GameManager.instance == null)

                // GameManager 프리팹을 인스턴스화합니다.
                Instantiate(gameManager);

            // 이미 정적 변수 SoundManager.instance에 SoundManager가 할당되었는지 또는 아직 null인지 확인합니다.
            if (SoundManager.instance == null)

                // SoundManager 프리팹을 인스턴스화합니다.
                Instantiate(soundManager);
        }
    }
}
