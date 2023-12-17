using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ARMainPanel : MonoBehaviour, IDragHandler
{
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
            foreach (var lamp in removedLamps)
            {
                var movingLamp = Instantiate(m_movingLampPrefab, transform.parent);
                movingLamp.Initialise(
                    startPosition: data.position + Random.insideUnitCircle * 80f,
                    target: m_collectAnimationTarget,
                    lamp: lamp
                );
            }
        }
    }
}
