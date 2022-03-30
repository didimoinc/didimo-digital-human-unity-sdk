using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateChooser : StateMachineBehaviour
{
    public List<Trigger> triggers;

    [Header("Debug:")]
    public Trigger currentTrigger;
    public float timeRemaining;

    float lastTime;
    static float rnd => UnityEngine.Random.value;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        if (currentTrigger == null) return;
        timeRemaining = Mathf.Max(0, currentTrigger.time - Time.time);
        if (Time.time >= currentTrigger.time && lastTime < currentTrigger.time)
            animator.SetTrigger(currentTrigger.name);
        lastTime = Time.time;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (triggers.Count == 0) return;
        currentTrigger = GetRandomTrigger();
        currentTrigger.setTime();
    }

    Trigger GetRandomTrigger()
    {
        var sumWeight = triggers.Sum(t => t.weight) * rnd;
        for (int i = 0; i < triggers.Count; i++, sumWeight -= triggers[i].weight)
            if (sumWeight <= triggers[i].weight)
                return triggers[i];
        return triggers[0];
    }

    [Serializable]
    public class Trigger
    {
        public string name;
        [Range(0, 120)] public float delayMinimum = 15;
        [Range(0, 120)] public float delayVariation = 15;
        [Range(0, 100)] public float weight = 1;
        [HideInInspector] public float time;
        public void setTime() => time = Time.time + delayMinimum + delayVariation * rnd;
    }
}
