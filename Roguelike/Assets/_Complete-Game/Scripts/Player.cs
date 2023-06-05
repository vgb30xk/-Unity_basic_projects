using UnityEngine;
using System.Collections;
using UnityEngine.UI;    // UI 사용을 가능하게 함
using UnityEngine.SceneManagement;

namespace Completed
{
    // Player는 이동할 수 있는 객체를 위한 기본 클래스인 MovingObject로부터 상속받음. Enemy도 이를 상속받음.
    public class Player : MovingObject
    {
        public float restartLevelDelay = 1f;        // 레벨을 재시작할 때의 딜레이(초)
        public int pointsPerFood = 10;              // 음식 아이템을 줍기 위해 플레이어의 음식 점수에 추가되는 점수
        public int pointsPerSoda = 20;              // 소다 아이템을 줍기 위해 플레이어의 음식 점수에 추가되는 점수
        public int wallDamage = 1;                  // 벽을 공격할 때 플레이어가 입히는 데미지
        public Text foodText;                       // 현재 플레이어의 음식 점수를 표시하는 UI 텍스트
        public AudioClip moveSound1;                // 플레이어 이동 시 재생될 오디오 클립 1
        public AudioClip moveSound2;                // 플레이어 이동 시 재생될 오디오 클립 2
        public AudioClip eatSound1;                 // 음식 아이템 획득 시 재생될 오디오 클립 1
        public AudioClip eatSound2;                 // 음식 아이템 획득 시 재생될 오디오 클립 2
        public AudioClip drinkSound1;               // 소다 아이템 획득 시 재생될 오디오 클립 1
        public AudioClip drinkSound2;               // 소다 아이템 획득 시 재생될 오디오 클립 2
        public AudioClip gameOverSound;             // 플레이어 사망 시 재생될 오디오 클립

        private Animator animator;                   // 플레이어의 Animator 컴포넌트에 대한 참조
        private int food;                            // 레벨 동안 플레이어의 음식 점수를 저장하는 변수
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;  // 모바일 컨트롤용 스크린 터치 원점의 위치를 저장하는 변수
#endif

        // MovingObject의 Start 함수를 오버라이드함
        protected override void Start()
        {
            // 플레이어의 Animator 컴포넌트에 대한 참조를 가져옴
            animator = GetComponent<Animator>();

            // GameManager.instance에서 레벨 간에 저장된 현재 음식 점수를 가져옴
            food = GameManager.instance.playerFoodPoints;

            // 현재 플레이어의 음식 총량을 반영하여 foodText를 설정합니다.
            foodText.text = "음식: " + food;

            // MovingObject의 기본 클래스인 Start 함수를 호출합니다.
            base.Start();
        }


        // 이 함수는 동작이 비활성화될 때 호출됩니다.
        private void OnDisable()
        {
            // Player 객체가 비활성화되면 현재 로컬 음식 총량을 GameManager에 저장하여 다음 레벨에서 다시 로드할 수 있도록 합니다.
            GameManager.instance.playerFoodPoints = food;
        }


        private void Update()
        {
            // 플레이어 턴이 아닌 경우 함수를 종료합니다.
            if (!GameManager.instance.playersTurn) return;

            int horizontal = 0;     // 수평 이동 방향을 저장하는 변수입니다.
            int vertical = 0;       // 수직 이동 방향을 저장하는 변수입니다.

            // Unity 에디터 또는 독립 실행형 빌드에서 실행 중인지 확인합니다.
#if UNITY_STANDALONE || UNITY_WEBPLAYER

            // 입력 관리자에서 입력을 받아 정수로 반올림하여 horizontal에 저장하여 x 축 이동 방향을 설정합니다.
            horizontal = (int)(Input.GetAxisRaw("Horizontal"));

            // 입력 관리자에서 입력을 받아 정수로 반올림하여 vertical에 저장하여 y 축 이동 방향을 설정합니다.
            vertical = (int)(Input.GetAxisRaw("Vertical"));

            // 가로로 이동 중인지 확인하고, 그렇다면 vertical을 0으로 설정합니다.
            if (horizontal != 0)
            {
                vertical = 0;
            }
            // iOS, Android, Windows Phone 8 또는 Unity iPhone에서 실행 중인지 확인합니다.
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

// 입력이 0보다 큰 터치가 등록되었는지 확인합니다.
if (Input.touchCount > 0)
{
    // 첫 번째로 감지된 터치를 저장합니다.
    Touch myTouch = Input.touches[0];

    // 터치의 상태가 Began인지 확인합니다.
    if (myTouch.phase == TouchPhase.Began)
    {
        // touchOrigin을 해당 터치의 위치로 설정합니다.
        touchOrigin = myTouch.position;
    }

    // 터치의 상태가 Began이 아니고, Ended와 touchOrigin.x가 0보다 크거나 같은지 확인합니다.
    else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
    {
        // touchEnd를 이 터치의 위치로 설정합니다.
        Vector2 touchEnd = myTouch.position;

        // x 축의 시작과 끝 사이의 차이를 계산합니다.
        float x = touchEnd.x - touchOrigin.x;

        // y 축의 시작과 끝 사이의 차이를 계산합니다.
        float y = touchEnd.y - touchOrigin.y;

        // touchOrigin.x를 -1로 설정하여 else if 문이 즉시 반복되지 않도록 합니다.
        touchOrigin.x = -1;

        // x 축의 차이가 y 축의 차이보다 큰지 확인합니다.
        if (Mathf.Abs(x) > Mathf.Abs(y))
            // x가 0보다 크면 horizontal을 1로 설정하고, 그렇지 않으면 -1로 설정합니다.
            horizontal = x > 0 ? 1 : -1;
        else
            // y가 0보다 크면 vertical을 1로 설정하고, 그렇지 않으면 -1로 설정합니다.
            vertical = y > 0 ? 1 : -1;
    }
}

#endif // 모바일 플랫폼 종속 컴파일 섹션의 끝

            // horizontal 또는 vertical이 0이 아닌지 확인합니다.
            if (horizontal != 0 || vertical != 0)
            {
                // Player가 Wall과 상호작용할 수 있기 때문에 AttemptMove에 대한 제네릭 매개변수로 Wall을 전달하여
                // Player가 어느 방향으로 이동해야 하는지 지정합니다.
                AttemptMove<Wall>(horizontal, vertical);
            }
        }


        // AttemptMove는 MovingObject의 AttemptMove 함수를 오버라이드합니다.
        // AttemptMove는 Player의 경우 Wall과 같은 유형의 매개변수 T를 사용하며, x 및 y 방향의 정수 값을 가져옵니다.
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            // Player가 이동할 때마다 음식 포인트를 감소시킵니다.
            food--;

            // 음식 포인트를 반영하여 foodText를 업데이트합니다.
            foodText.text = "음식: " + food;

            // 베이스 클래스인 MovingObject의 AttemptMove 메서드를 호출하여 component T (여기서는 Wall)과 이동 방향을 전달합니다.
            base.AttemptMove<T>(xDir, yDir);

            // Move에서 수행한 Linecast의 결과를 참조할 수 있도록 hit을 설정합니다.
            RaycastHit2D hit;

            // Move가 true를 반환하는 경우, Player가 빈 공간으로 이동할 수 있는 것을 의미합니다.
            if (Move(xDir, yDir, out hit))
            {
                // SoundManager의 RandomizeSfx 함수를 호출하여 무작위로 이동 소리를 재생합니다.
                SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
            }

            // Player가 이동하고 음식 포인트를 잃었으므로 게임이 종료되었는지 확인합니다.
            CheckIfGameOver();

            // Player의 턴이 끝났으므로 GameManager의 playersTurn을 false로 설정합니다.
            GameManager.instance.playersTurn = false;
        }


        // OnCantMove는 MovingObject의 추상 함수인 OnCantMove를 오버라이드합니다.
        // Player의 경우 공격하고 파괴할 수 있는 Wall과 같은 유형인 T를 제네릭 매개변수로 사용합니다.
        protected override void OnCantMove<T>(T component)
        {
            // component 매개변수로 전달된 컴포넌트를 hitWall에 설정합니다.
            Wall hitWall = component as Wall;

            // 부딪힌 Wall의 DamageWall 함수를 호출합니다.
            hitWall.DamageWall(wallDamage);

            // Player의 애니메이션 컨트롤러에서 attack 트리거를 설정하여 playerChop 애니메이션을 재생합니다.
            animator.SetTrigger("playerChop");
        }


        // OnTriggerEnter2D는 다른 개체가 이 개체에 연결된 트리거 콜라이더에 들어올 때 호출됩니다(2D 물리 전용).
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 부딪힌 트리거의 태그가 Exit인지 확인합니다.
            if (other.tag == "Exit")
            {
                // restartLevelDelay (기본값 1초)의 지연 시간이 포함된 Restart 함수를 호출하여 다음 레벨을 시작합니다.
                Invoke("Restart", restartLevelDelay);

                // 레벨이 끝났으므로 Player 객체를 비활성화합니다.
                enabled = false;
            }

            // 부딪힌 트리거의 태그가 Food인지 확인합니다.
            else if (other.tag == "Food")
            {
                // players의 현재 음식 포인트에 pointsPerFood를 추가합니다.
                food += pointsPerFood;

                // 현재 총계를 나타내는 foodText를 업데이트하고, 플레이어가 포인트를 획득했음을 알립니다.
                foodText.text = "+" + pointsPerFood + " 음식: " + food;

                // SoundManager의 RandomizeSfx 함수를 호출하여 eating 소리 효과를 재생합니다.
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

                // 부딪힌 음식 객체를 비활성화합니다.
                other.gameObject.SetActive(false);
            }

            // 부딪힌 트리거의 태그가 Soda인지 확인합니다.
            else if (other.tag == "Soda")
            {
                // players의 음식 포인트에 pointsPerSoda를 추가합니다.
                food += pointsPerSoda;

                // 현재 총계를 나타내는 foodText를 업데이트하고, 플레이어가 포인트를 획득했음을 알립니다.
                foodText.text = "+" + pointsPerSoda + " 음식: " + food;

                // SoundManager의 RandomizeSfx 함수를 호출하여 drinking 소리 효과를 재생합니다.
                SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

                // 부딪힌 소다 객체를 비활성화합니다.
                other.gameObject.SetActive(false);
            }
        }


        // Restart는 호출될 때 장면을 다시로드합니다.
        private void Restart()
        {
            // 현재 로드된 마지막 장면(Main)을 로드합니다. "Single" 모드로 로드하여 기존 장면을 대체하고
            // 현재 장면의 모든 객체를 로드하지 않습니다.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }


        // LoseFood는 적이 플레이어를 공격할 때 호출됩니다.
        // 파라미터로 int loss를 받아 플레이어의 음식 포인트를 감소시킵니다.
        public void LoseFood(int loss)
        {
            // 플레이어 애니메이터의 트리거를 설정하여 playerHit 애니메이션으로 전환합니다.
            animator.SetTrigger("playerHit");

            // 인수로 전달된 손실을 음식 포인트에서 감소시킵니다.
            food -= loss;

            // 음식 포인트를 업데이트합니다.
            foodText.text = "-" + loss + " 음식: " + food;

            // 플레이어가 음식 포인트를 잃었는지 확인하고, 게임이 종료되었는지 확인합니다.
            CheckIfGameOver();
        }


        // CheckIfGameOver는 플레이어가 음식 포인트를 잃었는지 확인하고, 게임이 종료되었는지 확인합니다.
        private void CheckIfGameOver()
        {
            // 음식 포인트가 0 이하인지 확인합니다.
            if (food <= 0)
            {
                // SoundManager의 PlaySingle 함수를 호출하여 gameOverSound를 재생합니다.
                SoundManager.instance.PlaySingle(gameOverSound);

                // SoundManager의 musicSource를 정지합니다.
                SoundManager.instance.musicSource.Stop();

                // GameManager의 GameOver 함수를 호출하여 게임을 종료합니다.
                GameManager.instance.GameOver();
            }
        }
    }
}

