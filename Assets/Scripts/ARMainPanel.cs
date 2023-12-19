using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ARMainPanel : MonoBehaviour, IDragHandler
{
    /// <summary>
    /// 1秒ごとのランプの数
    /// </summary>
    public float LampCountPerSecond = 10;

    /// <summary>
    /// ランプを表示する半径
    /// </summary>
    public float LampSpawnRadius = 50;

    [SerializeField]
    MovingLamp m_movingLampPrefab;

    [SerializeField]
    RectTransform m_collectAnimationTarget;

    [SerializeField]
    LampParticles m_particles;

    public void OnDrag(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
        {
            var removedLamps = m_particles.RemoveLamp(data.position, 0.6f);
            var delayMax = removedLamps.Length / LampCountPerSecond;
            foreach (var lamp in removedLamps)
            {
                var movingLamp = Instantiate(m_movingLampPrefab, transform.parent);
                movingLamp.Initialise(
                    startPosition: data.position + Random.insideUnitCircle * LampSpawnRadius,
                    target: m_collectAnimationTarget,
                    delay: Random.Range(0, delayMax),
                    lamp: lamp
                );
            }
        }
    }
}
