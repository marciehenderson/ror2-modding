using EntityStates;
using BotanistMod.Survivors.Botanist;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using EntityStates.Engi.EngiMissilePainter;
using UnityEngine.AddressableAssets;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BotanistMod.Survivors.Botanist.SkillStates
{
    public class WaterTheGarden : BaseSkillState
    {
        public static float damageCoefficient = BotanistStaticValues.sprayDamageCoefficient;
        public static float procCoefficient = 1F;
        public static float baseDuration = 0.6F;
        public static float force = 800F;
        public static float recoil = 100F;
        public static float maxSpread = 4.2F;
        public static float minSpread = 0.1F;
        public static float radius = 0.5F;
        public static float range = 50F;
        public static int sprayTotal = 30;
        public static float baseDelayDuration = baseDuration/sprayTotal; // equal division of time for each call to Fire()
        public int sprayCount = sprayTotal;
        private float stopwatch = 0F;
        private float delayTimer = 0F;
        public override void OnEnter()
        {
            delayTimer = base.fixedAge;
            base.OnEnter();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch -= Time.fixedDeltaTime;
            // regulate fire rate with delay
            if(stopwatch <= 0 && sprayCount > 0 && base.fixedAge - delayTimer >= baseDelayDuration)
            {
                delayTimer = base.fixedAge; // reset delay timer
                sprayCount--;
                Fire();
            }
            if(base.fixedAge >= baseDuration)
            {
                base.outer.SetNextStateToMain();
            }
        }
        private void Fire()
        {
            RoR2.BulletAttack spray = new RoR2.BulletAttack();
            spray.aimVector = base.GetAimRay().direction;
            spray.bulletCount = 1;
            spray.damage = base.damageStat * damageCoefficient;
            spray.damageColorIndex = DamageColorIndex.Default;
            spray.damageType = DamageType.Generic;
            spray.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
            spray.force = force;
            spray.HitEffectNormal = false;
            spray.hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Cleanse/CleanseEffect.prefab").WaitForCompletion();
            spray.isCrit = base.RollCrit();
            spray.maxDistance = range;
            spray.maxSpread = maxSpread;
            spray.minSpread = minSpread;
            spray.muzzleName = "WaterSprayMuzzle";
            spray.origin = base.GetAimRay().origin;
            spray.owner = base.gameObject;
            spray.procCoefficient = procCoefficient;
            spray.radius = radius;
            spray.smartCollision = true;
            spray.sniper = false;
            spray.stopperMask = LayerIndex.world.mask;
            spray.tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunCryo.prefab").WaitForCompletion();
            spray.weapon = base.gameObject;
            // spray water
            if(base.isAuthority)
            {
                spray.Fire();
                Log.Debug("Botanist watered her garden");
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

    }
}