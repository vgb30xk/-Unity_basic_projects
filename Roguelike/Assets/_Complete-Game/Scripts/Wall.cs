using UnityEngine;
using System.Collections;

namespace Completed
{
    public class Wall : MonoBehaviour
    {
        public AudioClip chopSound1;                // 플레이어가 벽을 공격할 때 재생되는 1번째 오디오 클립.
        public AudioClip chopSound2;                // 플레이어가 벽을 공격할 때 재생되는 2번째 오디오 클립.
        public Sprite dmgSprite;                    // 벽이 플레이어의 공격을 받은 후 표시되는 대체 스프라이트.
        public int hp = 3;                          // 벽의 체력.


        private SpriteRenderer spriteRenderer;      // 연결된 SpriteRenderer 컴포넌트에 대한 참조를 저장합니다.


        void Awake()
        {
            // SpriteRenderer에 대한 컴포넌트 참조를 가져옵니다.
            spriteRenderer = GetComponent<SpriteRenderer>();
        }


        // DamageWall은 플레이어가 벽을 공격할 때 호출됩니다.
        public void DamageWall(int loss)
        {
            // SoundManager의 RandomizeSfx 함수를 호출하여 두 개의 벽 부수는 소리 중 하나를 재생합니다.
            SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);

            // spriteRenderer를 손상된 벽 스프라이트로 설정합니다.
            spriteRenderer.sprite = dmgSprite;

            // 체력에서 손실을 뺍니다.
            hp -= loss;

            // 체력이 0 이하인 경우:
            if (hp <= 0)
                // gameObject를 비활성화합니다.
                gameObject.SetActive(false);
        }
    }
}
