using EntityStates;
using BotanistMod.Survivors.Botanist;
using RoR2;
using UnityEngine;
using On.RoR2;
using UnityEngine.AddressableAssets;
using EntityStates.Missions.Arena.NullWard;
using System.IO;

namespace BotanistMod.Survivors.Botanist.SkillStates
{
    public class Shoot : GenericProjectileBaseState
    {
        public static float DamageCoefficient = BotanistStaticValues.gunDamageCoefficient;
        public static float ProcCoefficient = 1f;
        public static float BaseDuration = 0.6f;
        //only modify this if you know what you're doing
        public static float BaseDelayDuration = 0.0F;
        public static float FirePercentTime = 0.0f;
        public static float Force = 800f;
        public static float Recoil = 3f;
        public static float Range = 256f;
        public static GameObject TracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBruiser/TracerClayBruiserMinigun.prefab").WaitForCompletion();

        // private float projectileDuration;
        // private float fireTime;
        // private bool hasFired;
        // private string muzzleString;

        public override void OnEnter()
        {
            projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBoss/ClayPotProjectile.prefab").WaitForCompletion();
            attackSoundString = "BotanistPotThrow";
            baseDuration = BaseDuration;
            baseDelayBeforeFiringProjectile = BaseDelayDuration;
            damageCoefficient = DamageCoefficient;
            force = Force;
            recoilAmplitude = Recoil;
            bloom = 10F;
            // projectileDuration = BaseDuration / attackSpeedStat;
            // fireTime = FirePercentTime * projectileDuration;
            base.OnEnter();
            
            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // if (fixedAge >= fireTime)
            // {
            //     Fire();
            // }

            // if (fixedAge >= duration && isAuthority)
            // {
            //     outer.SetNextStateToMain();
            //     return;
            // }
        }

        // private void Fire()
        // {
        //     if (!hasFired)
        //     {
        //         hasFired = true;

        //         characterBody.AddSpreadBloom(1.5f);
        //         RoR2.EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
        //         RoR2.Util.PlaySound("BotanistShootPistol", gameObject);

        //         if (isAuthority)
        //         {
        //             Ray aimRay = GetAimRay();
        //             AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);
        //             // new RoR2.BlastAttack
        //             // {

        //             // }.Fire();
        //             // new RoR2.OverlapAttack
        //             // {
        //             //     attacker = gameObject,
        //             //     inflictor = gameObject,
        //             //     teamIndex = TeamIndex.Player,
        //             //     attackerFiltering = AttackerFiltering.Default,
        //             //     forceVector = aimRay.direction,
        //             //     pushAwayForce = force,
        //             //     damage = damageCoefficient * damageStat,
        //             //     isCrit = RollCrit(),
        //             //     procChainMask = default,
        //             //     procCoefficient = procCoefficient,
        //             //     hitBoxGroup = default,
        //             //     hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBoss/ClayPotProjectileExplosion.prefab").WaitForCompletion(),
        //             //     impactSound = default,
        //             //     damageColorIndex = DamageColorIndex.Default,
        //             //     damageType = DamageType.ClayGoo,
        //             //     maximumOverlapTargets = 100,
        //             // }.Fire();
        //             // new RoR2.BulletAttack
        //             // {
        //             //     owner = gameObject,
        //             //     weapon = gameObject,
        //             //     origin = aimRay.origin,
        //             //     aimVector = aimRay.direction,
        //             //     minSpread = 0.0F,
        //             //     maxSpread = characterBody.spreadBloomAngle,
        //             //     bulletCount = 1U,
        //             //     procCoefficient = procCoefficient,
        //             //     damage = damageCoefficient * damageStat,
        //             //     damageType = DamageType.ClayGoo,
        //             //     force = force,
        //             //     falloffModel = RoR2.BulletAttack.FalloffModel.DefaultBullet,
        //             //     tracerEffectPrefab = tracerEffectPrefab,
        //             //     muzzleName = muzzleString,
        //             //     hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBoss/ClayPotProjectileExplosion.prefab").WaitForCompletion(),
        //             //     isCrit = RollCrit(),
        //             //     HitEffectNormal = false,
        //             //     stopperMask = RoR2.LayerIndex.world.mask,
        //             //     smartCollision = true,
        //             //     maxDistance = range,
        //             // }.Fire();
        //         }
        //     }
        // }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}