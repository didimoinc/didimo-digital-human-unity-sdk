using UnityEngine;

namespace DigitalSalmon
{
    /// <summary>
    /// Harmonic motion allows spring dampened calculations in ND space.
    /// The Calculate methods can be used along with transform assignments to create very juicy motion.
    /// http://www.ryanjuckett.com/programming/damped-springs/
    /// </summary>
    public static class HarmonicMotion
    {
        //-----------------------------------------------------------------------------------------
        // Type Definitions:
        //-----------------------------------------------------------------------------------------

        public struct DampenedSpringMotionParams
        {
            // newPos = posPosCoef*oldPos + posVelCoef*oldVel
            public float PosPosCoef, PosVelCoef;

            // newVel = velPosCoef*oldPos + velVelCoef*oldVel
            public float VelPosCoef, VelVelCoef;
        }

        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        public static void Calculate(ref Vector3 state, ref Vector3 velocity, Vector3 targetState, DampenedSpringMotionParams springMotionParams)
        {
            float velocityX = velocity.x;
            float stateX = state.x;
            float targetX = targetState.x;
            Calculate(ref stateX, ref velocityX, targetX, springMotionParams);

            float velocityY = velocity.y;
            float stateY = state.y;
            float targetY = targetState.y;
            Calculate(ref stateY, ref velocityY, targetY, springMotionParams);

            float velocityZ = velocity.z;
            float stateZ = state.z;
            float targetZ = targetState.z;
            Calculate(ref stateZ, ref velocityZ, targetZ, springMotionParams);

            velocity = new Vector3(velocityX, velocityY, velocityZ);
            state = new Vector3(stateX, stateY, stateZ);
        }

        public static void Calculate(ref Vector2 state, ref Vector2 velocity, Vector2 targetState, DampenedSpringMotionParams springMotionParams)
        {
            float velocityX = velocity.x;
            float stateX = state.x;
            float targetX = targetState.x;
            Calculate(ref stateX, ref velocityX, targetX, springMotionParams);

            float velocityY = velocity.y;
            float stateY = state.y;
            float targetY = targetState.y;

            Calculate(ref stateY, ref velocityY, targetY, springMotionParams);

            velocity = new Vector2(velocityX, velocityY);
            state = new Vector2(stateX, stateY);
        }

        /// <param name="state">position value to update</param>
        /// <param name="velocity">velocity value to update</param>
        /// <param name="targetState">velocity value to update</param>
        /// <param name="springMotionParams">motion parameters to use</param>
        public static void Calculate(ref float state, ref float velocity, float targetState, DampenedSpringMotionParams springMotionParams)
        {
            float oldPos = state - targetState; // update in equilibrium relative space
            float oldVel = velocity;

            state = oldPos * springMotionParams.PosPosCoef + oldVel * springMotionParams.PosVelCoef + targetState;
            velocity = oldPos * springMotionParams.VelPosCoef + oldVel * springMotionParams.VelVelCoef;
        }

        /// <summary>
        /// </summary>
        /// <param name="angularFrequency"> angular frequency of motion</param>
        /// <param name="dampingRatio">damping ratio of motion</param>
        public static DampenedSpringMotionParams CalcDampedSpringMotionParams(float dampingRatio, float angularFrequency)
        {
            const float epsilon = 0.0001f;

            // force values into legal range
            if (dampingRatio < 0.0f) dampingRatio = 0.0f;
            if (angularFrequency < 0.0f) angularFrequency = 0.0f;

            // if there is no angular frequency, the spring will not move and we can
            // return identity
            if (angularFrequency < epsilon)
            {
                return new DampenedSpringMotionParams {PosPosCoef = 1.0f, PosVelCoef = 0.0f, VelPosCoef = 0.0f, VelVelCoef = 1.0f};
            }

            if (dampingRatio > 1.0f + epsilon)
            {
                // over-damped
                float za = -angularFrequency * dampingRatio;
                float zb = angularFrequency * Mathf.Sqrt(dampingRatio * dampingRatio - 1.0f);
                float z1 = za - zb;
                float z2 = za + zb;

                float e1 = Mathf.Exp(z1 * Time.deltaTime);
                float e2 = Mathf.Exp(z2 * Time.deltaTime);

                float invTwoZb = 1.0f / (2.0f * zb); // = 1 / (z2 - z1)

                float e1_Over_TwoZb = e1 * invTwoZb;
                float e2_Over_TwoZb = e2 * invTwoZb;

                float z1e1_Over_TwoZb = z1 * e1_Over_TwoZb;
                float z2e2_Over_TwoZb = z2 * e2_Over_TwoZb;

                return new DampenedSpringMotionParams {PosPosCoef = e1_Over_TwoZb * z2 - z2e2_Over_TwoZb + e2, PosVelCoef = -e1_Over_TwoZb + e2_Over_TwoZb, VelPosCoef = (z1e1_Over_TwoZb - z2e2_Over_TwoZb + e2) * z2, VelVelCoef = -z1e1_Over_TwoZb + z2e2_Over_TwoZb};
            }

            if (dampingRatio < 1.0f - epsilon)
            {
                // under-damped
                float omegaZeta = angularFrequency * dampingRatio;
                float alpha = angularFrequency * Mathf.Sqrt(1.0f - dampingRatio * dampingRatio);

                float expTerm = Mathf.Exp(-omegaZeta * Time.deltaTime);
                float cosTerm = Mathf.Cos(alpha * Time.deltaTime);
                float sinTerm = Mathf.Sin(alpha * Time.deltaTime);

                float invAlpha = 1.0f / alpha;

                float expSin = expTerm * sinTerm;
                float expCos = expTerm * cosTerm;
                float expOmegaZetaSin_Over_Alpha = expTerm * omegaZeta * sinTerm * invAlpha;

                return new DampenedSpringMotionParams {PosPosCoef = expCos + expOmegaZetaSin_Over_Alpha, PosVelCoef = expSin * invAlpha, VelPosCoef = -expSin * alpha - omegaZeta * expOmegaZetaSin_Over_Alpha, VelVelCoef = expCos - expOmegaZetaSin_Over_Alpha};
            }
            else
            {
                // critically damped
                float expTerm = Mathf.Exp(-angularFrequency * Time.deltaTime);
                float timeExp = Time.deltaTime * expTerm;
                float timeExpFreq = timeExp * angularFrequency;

                return new DampenedSpringMotionParams {PosPosCoef = timeExpFreq + expTerm, PosVelCoef = timeExp, VelPosCoef = -angularFrequency * timeExpFreq, VelVelCoef = -timeExpFreq + expTerm};
            }
        }
    }
}