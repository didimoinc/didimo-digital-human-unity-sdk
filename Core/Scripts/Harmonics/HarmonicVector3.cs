using System;
using UnityEngine;

namespace DigitalSalmon
{
    public class HarmonicVector3
    {
        private readonly Func<Vector3>   getValue;
        private readonly Action<Vector3> setValue;
        private readonly Func<float>     getDampingRatio;
        private readonly Func<float>     getAngularFrequency;

        private Vector3 velocity;

        public HarmonicVector3(Func<Vector3> getValue, Action<Vector3> setValue, Func<float> getDampingRatio, Func<float> getAngularFrequency)
        {
            this.getValue = getValue;
            this.setValue = setValue;
            this.getDampingRatio = getDampingRatio;
            this.getAngularFrequency = getAngularFrequency;
        }

        public void Update(Vector3 target)
        {
            Vector3 state = getValue();
            HarmonicMotion.DampenedSpringMotionParams springParams = HarmonicMotion.CalcDampedSpringMotionParams(getDampingRatio(), getAngularFrequency());
            HarmonicMotion.Calculate(ref state, ref velocity, target, springParams);
            setValue(state);
        }
    }

    public class HarmonicVector2
    {
        private readonly Func<Vector2>   getValue;
        private readonly Action<Vector2> setValue;
        private readonly Func<float>     getDampingRatio;
        private readonly Func<float>     getAngularFrequency;

        private Vector2 state;
        private Vector2 velocity;

        public HarmonicVector2(Func<Vector2> getValue, Action<Vector2> setValue, Func<float> getDampingRatio, Func<float> getAngularFrequency)
        {
            this.getValue = getValue;
            this.setValue = setValue;
            this.getDampingRatio = getDampingRatio;
            this.getAngularFrequency = getAngularFrequency;
        }

        public HarmonicVector2(Func<Vector2> getValue, Action<Vector2> setValue, float dampingRatio, float angularFrequency)
        {
            this.getValue = getValue;
            this.setValue = setValue;
            getDampingRatio = () => dampingRatio;
            getAngularFrequency = () => angularFrequency;
        }

        public void Update(Vector2 target, float dampingRatio = -1, float angularFrequency = -1)
        {
            state = getValue();
            HarmonicMotion.DampenedSpringMotionParams springParams = HarmonicMotion.CalcDampedSpringMotionParams(dampingRatio == -1 ? getDampingRatio() : dampingRatio, angularFrequency == -1 ? getAngularFrequency() : angularFrequency);
            HarmonicMotion.Calculate(ref state, ref velocity, target, springParams);
            setValue(state);
        }

        public static implicit operator Vector2(HarmonicVector2 self) => self.state;
    }
}