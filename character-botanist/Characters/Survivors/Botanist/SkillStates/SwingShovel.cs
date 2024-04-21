using BotanistMod.Modules.BaseStates;
using EntityStates;
using EntityStates.Engi.EngiMissilePainter;
using JetBrains.Annotations;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BotanistMod.Survivors.Botanist.SkillStates
{
    public class SwingShovel : BaseSkillState
    {
        public static float damageCoefficient = BotanistStaticValues.swordDamageCoefficient;
        public static float procCoefficient = 1F;
        public static float baseDuration = 1.2F;
        public static float force = 10000F;
        public static float recoil = 100F;
        public static float radius = 10F;
        public int swingCount = 1;
        private float stopwatch = 0F;
        public override void OnEnter()
        {
            base.OnEnter();
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
            if(stopwatch <= 0 && swingCount > 0)
            {
                swingCount--;
                Fire();
            }
            if(base.fixedAge >= baseDuration) {
                base.outer.SetNextStateToMain();
            }
        }
        private void Fire()
        {
            // define 'melee' attack
            RoR2.BlastAttack swing = new RoR2.BlastAttack();
            swing.attacker = base.gameObject;
            swing.attackerFiltering = AttackerFiltering.NeverHitSelf;
            swing.baseDamage = base.damageStat * damageCoefficient;
            swing.baseForce = force;
            swing.bonusForce = Vector3.zero;
            swing.canRejectForce = false;
            swing.crit = base.RollCrit();
            swing.damageType = DamageType.AOE;
            swing.damageColorIndex = DamageColorIndex.Default;
            swing.falloffModel = BlastAttack.FalloffModel.Linear;
            swing.impactEffect = RoR2.EffectCatalog.FindEffectIndexFromPrefab(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/ImpactLoaderFistSmall.prefab").WaitForCompletion());
            swing.inflictor = base.gameObject;
            swing.losType = BlastAttack.LoSType.None;
            swing.position = base.GetAimRay().origin;
            swing.procChainMask = default;
            swing.procCoefficient = procCoefficient;
            swing.radius = radius;
            swing.teamIndex = base.GetTeam();
            // swing the shovel
            if(base.isAuthority)
            {
                // temporary effect until animation is created
                RoR2.EffectManager.SimpleEffect(
                    Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Cleanse/CleanseEffect.prefab").WaitForCompletion(),
                    transform.position,
                    RoR2.Util.QuaternionSafeLookRotation(Vector3.forward),
                    true
                );
                swing.Fire();
                Log.Debug("Botanist swung their shovel");
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}