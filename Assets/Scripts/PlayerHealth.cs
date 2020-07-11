using UnityEngine;
using UnityEngine.UI; // UI 관련 코드

// 플레이어 캐릭터의 생명체로서의 동작을 담당
public class PlayerHealth : LivingEntity
{
    public Slider healthSlider; // 체력을 표시할 UI 슬라이더

    public AudioClip deathClip; // 사망 소리
    public AudioClip hitClip; // 피격 소리
    public AudioClip itemPickupClip; // 아이템 습득 소리

    private AudioSource playerAudioPlayer; // 플레이어 소리 재생기
    private Animator playerAnimator; // 플레이어의 애니메이터

    private PlayerMovement playerMovement; // 플레이어 움직임 컴포넌트
    private PlayerShooter playerShooter; // 플레이어 슈터 컴포넌트

    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();
        playerAudioPlayer = GetComponent<AudioSource>();

        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
    }

    protected override void OnEnable()
    {
        base.OnEnable(); // LivingEntity의 OnEnable() 실행 (상태 초기화)

        healthSlider.gameObject.SetActive(true); // 체력 슬라이더 활성화
        healthSlider.maxValue = startingHealth; // 체력 슬라이더의 최댓값을 기본 체력값으로 변경
        healthSlider.value = health; // 체력 슬라이더의 값을 현재 체력값으로 변경

        // 플레이어 조작을 받는 컴포넌트 활성화
        playerMovement.enabled = true;
        playerShooter.enabled = true;
    
    }

    public override void RestoreHealth(float newHealth)
    {
        base.RestoreHealth(newHealth); // LivingEntity의 RestoreHealth()실행 (체력 증가)

        healthSlider.value = health; // 갱신된 체력으로 체력 슬라이더 갱신
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if(!dead)
        {
            playerAudioPlayer.PlayOneShot(hitClip); // 사망하지 않은 경우에만 효과음 재생
        }

        base.OnDamage(damage, hitPoint, hitNormal); // LivingEntity의 OnDamage()실행 (대미지 적용)

        healthSlider.value = health; // 갱신된 체력을 체력 슬라이더에 반영
    }

    public override void Die()
    {
        base.Die(); //  LivingEntity의 Die()실행 (사망 적용)

        healthSlider.gameObject.SetActive(false); // 체력 슬라이더 비활성화

        playerAudioPlayer.PlayOneShot(deathClip); // 사망음 재생
        playerAnimator.SetTrigger("Die"); // 애니메이터의 Die 트리거를 발동시켜 사망 애니메이션 재생

        // 플레이어 조작을 받는 컴포넌트 비활성화
        playerMovement.enabled = false;
        playerShooter.enabled = false;
    }

    private void OnTriggerEnter(Collider other) // 아이템과 충돌한 경우 해당 아이템을 사용하는 처리
    {
        if(!dead) // 사망하지 않은 경우에만 아이템 사용 가능
        {
            IItem item = other.GetComponent<IItem>(); // 충돌한 상대방으로부터 IItem 컴포넌트 가져오기

            if(item != null) // 충돌한 상대방으로부터 IItem 컴포넌트를 가져오는데 성공했다면
            {
                item.Use(gameObject); // Use 메서드를 실행하여 아이템 사용
                playerAudioPlayer.PlayOneShot(itemPickupClip); // 아이템 습득 소리 재생
            }
        }
    }
}