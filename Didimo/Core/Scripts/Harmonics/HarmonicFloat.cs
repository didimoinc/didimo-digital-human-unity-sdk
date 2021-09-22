using System;
using UnityEngine;

namespace DigitalSalmon
{
    public class HarmonicFloat
    {
        private readonly Func<float>   getValue;
        private readonly Action<float> setValue;
        private readonly Func<float>   getDampingRatio;
        private readonly Func<float>   getAngularFrequency;

        private float velocity;
        private float targetValue;

        public bool IsRunning { get; private set; }

        public HarmonicFloat(Func<float> getValue, Action<float> setValue, Func<float> getDampingRatio, Func<float> getAngularFrequency)
        {
            this.getValue = getValue;
            this.setValue = setValue;
            this.getDampingRatio = getDampingRatio;
            this.getAngularFrequency = getAngularFrequency;
        }

        public HarmonicFloat(Func<float> getValue, Action<float> setValue, float dampingRatio, float angularFrequency)
        {
            this.getValue = getValue;
            this.setValue = setValue;
            getDampingRatio = () => dampingRatio;
            getAngularFrequency = () => angularFrequency;
        }

        public void Update(float target)
        {
            SetTarget(target);
            Update();
        }

        public void Update()
        {
            float state = getValue();
            HarmonicMotion.DampenedSpringMotionParams springParams = HarmonicMotion.CalcDampedSpringMotionParams(getDampingRatio(), getAngularFrequency());
            HarmonicMotion.Calculate(ref state, ref velocity, targetValue, springParams);
            setValue(state);
            IsRunning = velocity > 0.0001f || Mathf.Abs(state - targetValue) > 0.00001f;
        }

        public void SetValue(float value)
        {
            setValue(value);
            velocity = 0;
            targetValue = value;
        }

        public void SetTarget(float target) { targetValue = target; }
    }
}