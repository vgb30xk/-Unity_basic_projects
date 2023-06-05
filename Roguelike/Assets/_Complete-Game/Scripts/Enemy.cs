using UnityEngine;
using System.Collections;

namespace Completed
{
    // Enemy는 MovingObject를 상속받는다. 이는 움직일 수 있는 객체를 위한 기본 클래스이다. Player도 이를 상속받는다.
    public class Enemy : MovingObject
    {
        public int playerDamage;                            // 플레이어를 공격할 때 감소시킬 음식 포인트의 양.
        public AudioClip attackSound1;                      // 플레이어를 공격할 때 재생할 오디오 클립 1.
        public AudioClip attackSound2;                      // 플레이어를 공격할 때 재생할 오디오 클립 2.

        private Animator animator;                          // 적의 Animator 컴포넌트에 대한 참조를 저장하는 변수.
        private Transform target;                           // 매 회전마다 이동하려는 대상의 Transform을 저장하는 변수.
        private bool skipMove;                              // 적이 턴을 건너뛸지 여부를 결정하는 불리언 변수.

        // Start는 기본 클래스의 가상 Start 함수를 재정의한다.
        protected override void Start()
        {
            // 적을 GameManager의 Enemy 객체 목록에 추가하여 GameManager가 이동 명령을 내릴 수 있도록 한다.
            GameManager.instance.AddEnemyToList(this);

            // 첨부된 Animator 컴포넌트에 대한 참조를 가져와 저장한다.
            animator = GetComponent<Animator>();

            // "Player" 태그를 가진 Player GameObject를 찾아서 해당하는 Transform 컴포넌트에 대한 참조를 저장한다.
            target = GameObject.FindGameObjectWithTag("Player").transform;

            // 기본 클래스 MovingObject의 Start 함수를 호출한다.
            base.Start();
        }

        // MovingObject의 AttemptMove 함수를 재정의하여 적이 턴을 건너뛰는 기능을 포함시킨다.
        // MovingObject에 대한 기본 AttemptMove 함수의 작동 방식에 대한 자세한 내용은 주석을 참조하라.
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            // skipMove가 true인지 확인하고, 그렇다면 skipMove를 false로 설정하고 이번 턴에 건너뛴다.
            if (skipMove)
            {
                skipMove = false;
                return;
            }

            // MovingObject의 AttemptMove 함수를 호출한다.
            base.AttemptMove<T>(xDir, yDir);

            // 적이 이동했으므로 다음 이동을 건너뛰기 위해 skipMove를 true로 설정한다.
            skipMove = true;
        }

        // MoveEnemy는 GameManger가 매 턴마다 각 적에게 플레이어를 향해 이동하도록 지시하는 함수이다.
        public void MoveEnemy()
        {
            // X 및 Y 축 이동 방향을 위한 변수를 선언한다. 이 값들은 -1부터 1까지의 범위를 가진다.
            // 이 값들은 위, 아래, 왼쪽, 오른쪽과 같은 기본 방향을 선택하는 데 사용된다.
            int xDir = 0;
            int yDir = 0;

            // 위치의 차이가 거의 없을 경우 (Epsilon) 다음을 수행한다:
            if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
            {
                // 대상의 (플레이어) 위치의 y 좌표가 이 적의 위치의 y 좌표보다 큰 경우 y 방향을 1로 설정하여 위로 이동한다.
                // 그렇지 않으면 -1로 설정하여 아래로 이동한다.
                yDir = target.position.y > transform.position.y ? 1 : -1;
            }
            // 위치의 차이가 거의 없지 않을 경우 (Epsilon) 다음을 수행한다:
            else
            {
                // 대상의 x 좌표가 이 적의 x 좌표보다 큰 경우 x 방향을 1로 설정하여 오른쪽으로 이동한다.
                // 그렇지 않으면 -1로 설정하여 왼쪽으로 이동한다.
                xDir = target.position.x > transform.position.x ? 1 : -1;
            }

            // AttemptMove 함수를 호출하고, 적이 이동하며 플레이어를 만날 가능성이 있다는 점에서 generic 매개변수로 Player를 전달한다.
            AttemptMove<Player>(xDir, yDir);
        }

        // OnCantMove는 적이 플레이어가 차지한 공간으로 이동하려고 할 때 호출되며, MovingObject의 OnCantMove 함수를 재정의한다.
        // 이 때 generic 매개변수 T를 전달하여 예상하는 컴포넌트를 전달한다. 이 경우 Player가 전달된다.
        protected override void OnCantMove<T>(T component)
        {
            // component로 전달된 encountered component와 동일한 hitPlayer를 선언하고 설정한다.
            Player hitPlayer = component as Player;

            // hitPlayer의 LoseFood 함수를 호출하여 playerDamage를 전달하여 음식 포인트를 감소시킨다.
            hitPlayer.LoseFood(playerDamage);

            // 애니메이터의 attack 트리거를 설정하여 Enemy 공격 애니메이션을 실행한다.
            animator.SetTrigger("enemyAttack");

            // attackSound1과 attackSound2 중에서 무작위로 선택하기 위해 SoundManager의 RandomizeSfx 함수를 호출한다.
            SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
        }
    }
}
