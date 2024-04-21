using EntityStates;
using BotanistMod.Survivors.Botanist;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace BotanistMod.Survivors.Botanist.SkillStates
{
    public class WaterHop : BaseSkillState
    {
        public static float duration = 0.5F;
        public static float initialSpeedCoefficient = 4F;
        public static float finalSpeedCoefficient = initialSpeedCoefficient/2F;
        public static string hopSoundString = "BotanistWaterHop";
        private float speed;
        private Vector3 movementDirection;
        private Vector3 previousPosition;

        public override void OnEnter()
        {
            base.OnEnter();
            // play cleanse splash effect
            var effectPosition = transform.position;
            effectPosition.y -= 0.5F; // move the effect origin below the character
            RoR2.EffectManager.SimpleEffect(
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Cleanse/CleanseEffect.prefab").WaitForCompletion(),
                effectPosition,
                RoR2.Util.QuaternionSafeLookRotation(Vector3.forward),
                true
            );

            if (isAuthority && inputBank && characterDirection)
            {
                // set the movement position either based on control direction or character direction
                movementDirection = (inputBank.moveVector == Vector3.zero ? characterDirection.forward : inputBank.moveVector).normalized;
                // aim vector upwards
                movementDirection.y = 0.8F;
            }

            RecalculateSpeed();

            if (characterMotor)
            {
                characterMotor.velocity = movementDirection * speed;
            }

            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

            Util.PlaySound(hopSoundString, gameObject);

            // modify character state
            if (NetworkServer.active)
            {
                // remove all player debuffs
                foreach(var index in RoR2.BuffCatalog.debuffBuffIndices)
                {
                    // only if character actually has debuff
                    if(characterBody.HasBuff(index))
                    {
                        characterBody.RemoveBuff(index);
                    }
                }
                
            }
        }

        private void RecalculateSpeed()
        {
            speed = moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RecalculateSpeed();

            Vector3 normalized = (transform.position - previousPosition).normalized;
            if (characterMotor && characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * speed;
                float d = Mathf.Max(Vector3.Dot(vector, movementDirection), 0f);
                vector = movementDirection * d;

                characterMotor.velocity = vector;
            }
            previousPosition = transform.position;

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            characterMotor.disableAirControlUntilCollision = false;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(movementDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            movementDirection = reader.ReadVector3();
        }
    }
}