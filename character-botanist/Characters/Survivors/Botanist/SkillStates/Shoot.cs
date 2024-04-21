using EntityStates;
using BotanistMod.Survivors.Botanist;
using RoR2;
using UnityEngine;
using On.RoR2;
using UnityEngine.AddressableAssets;
using EntityStates.Missions.Arena.NullWard;
using System.IO;
using System;
using R2API.Utils;
using IL.RoR2.Orbs;
using RoR2.Projectile;
using EntityStates.Engi.EngiMissilePainter;

namespace BotanistMod.Survivors.Botanist.SkillStates
{
    public class Shoot : BaseSkillState
    {
        public static float damageCoefficient = BotanistStaticValues.gunDamageCoefficient;
        public static float procCoefficient = 1F;
        public static float baseDuration = 0.6F;
        //only modify this if you know what you're doing
        public static float baseDelayDuration = 0.0F;
        public static float fireDelay = 0.2F;
        public static float force = 800F;
        public static float recoil = 3F;
        public static float range = 256F;
        public static float minSpread = -0.6F; // controls minimum spread
        public static float maxSpread = 0.6F; // controls maximum spread
        public static float spreadYawScale = 1F;
        public static float spreadPitchScale = 1F;
        public static float bonusYaw = 0F;
        public static float bonusPitch = -15F; // controls arc of throw
        public int projectileCount = 1;
        private float stopwatch = 0F;
        public override void OnEnter()
        {
            base.OnEnter();
            fireDelay /= base.attackSpeedStat;
            baseDuration /= base.attackSpeedStat;
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch <= 0 && projectileCount > 0)
            {
                projectileCount--;
                Fire();
            }
            if(base.fixedAge >= baseDuration) {
                base.outer.SetNextStateToMain();
            }
        }
        private void Fire()
        {
            // define pot projecile using the clay pot prefab as a base
            FireProjectileInfo info = new FireProjectileInfo();
            info.projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBoss/ClayPotProjectile.prefab").WaitForCompletion();
            info.damage = base.damageStat * damageCoefficient;
            info.damageTypeOverride = DamageType.ClayGoo;
            info.damageColorIndex = DamageColorIndex.Default;
            info.crit = base.RollCrit();
            info.owner = gameObject;
            info.force = force;
            info.position = transform.position;
            info.rotation = RoR2.Util.QuaternionSafeLookRotation(RoR2.Util.ApplySpread(base.GetAimRay().direction, minSpread, maxSpread, spreadYawScale, spreadPitchScale, bonusYaw, bonusPitch));
            // throw the projectile
            if (base.isAuthority) ProjectileManager.instance.FireProjectile(info);
            Log.Debug("Botanist threw a pot");
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}