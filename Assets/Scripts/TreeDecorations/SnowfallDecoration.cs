using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SnowfallDecoration : MonoBehaviour
{
    public void SetColor(Color color)
    {
        var particle = GetComponent<ParticleSystem>();
        var main = particle.main;
        var sub = particle.subEmitters.GetSubEmitterSystem(0).main;

        main.startColor = color;
        sub.startColor = color;
    }
}
