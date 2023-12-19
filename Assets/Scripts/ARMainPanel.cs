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
    LampCollectionTarget m_lampCollectionTarget;

    [SerializeField]
    LampParticles m_particles;

    public void Start()
    {
        SaveManager.RequireInstance();
    }

    public void OnDrag(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
        {
            var removedLamps = m_particles.RemoveLamp(data.position, 0.6f);

            if (removedLamps.Length > 0)
            {
                // ランプ回収アニメーション
                var delayMax = removedLamps.Length / LampCountPerSecond;
                foreach (var lamp in removedLamps)
                {
                    var movingLamp = Instantiate(m_movingLampPrefab, transform.parent);
                    movingLamp.Initialise(
                        startPosition: data.position + Random.insideUnitCircle * LampSpawnRadius,
                        target: m_lampCollectionTarget,
                        delay: Random.Range(0, delayMax),
                        lamp: lamp
                    );
                }

                // 回収したランプの個数を追加
                var save = SaveManager.Instance.SaveData;
                foreach (var lamp in removedLamps)
                {
                    var lampCount = save.CollectedLampCount.GetValueOrDefault(lamp.Color, 0);
                    lampCount += 1;
                    save.CollectedLampCount[lamp.Color] = lampCount;
                }
                SaveManager.Instance.Save();
            }
        }
    }
}
