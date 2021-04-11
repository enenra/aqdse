using System.Collections.Generic;
using ProtoBuf;
using VRageMath;

namespace WeaponThread
{
    public class WeaponStructure
    {
        [ProtoContract]
        public struct WeaponDefinition
        {
            [ProtoMember(1)] internal ModelAssignmentsDef Assignments;
            [ProtoMember(2)] internal TargetingDef Targeting;
            [ProtoMember(3)] internal AnimationDef Animations;
            [ProtoMember(4)] internal HardPointDef HardPoint;
            [ProtoMember(5)] internal AmmoDef[] Ammos;
            [ProtoMember(6)] internal string ModPath;
            [ProtoMember(7)] internal Dictionary<string, UpgradeValues[]> Upgrades;

            [ProtoContract]
            public struct ModelAssignmentsDef
            {
                [ProtoMember(1)] internal MountPointDef[] MountPoints;
                [ProtoMember(2)] internal string[] Barrels;
                [ProtoMember(3)] internal string Ejector;
                [ProtoMember(4)] internal string Scope;

                [ProtoContract]
                public struct MountPointDef
                {
                    [ProtoMember(1)] internal string SubtypeId;
                    [ProtoMember(2)] internal string AimPartId; //Depreciated
                    [ProtoMember(3)] internal string MuzzlePartId;
                    [ProtoMember(4)] internal string AzimuthPartId;
                    [ProtoMember(5)] internal string ElevationPartId;
                    [ProtoMember(6)] internal float DurabilityMod;
                    [ProtoMember(7)] internal string IconName;
                }
            }

            [ProtoContract]
            public struct TargetingDef
            {
                public enum Threat
                {
                    Projectiles,
                    Characters,
                    Grids,
                    Neutrals,
                    Meteors,
                    Other
                }

                public enum BlockTypes
                {
                    Any,
                    Offense,
                    Utility,
                    Power,
                    Production,
                    Thrust,
                    Jumping,
                    Steering
                }

                [ProtoMember(1)] internal int TopTargets;
                [ProtoMember(2)] internal int TopBlocks;
                [ProtoMember(3)] internal double StopTrackingSpeed;
                [ProtoMember(4)] internal float MinimumDiameter;
                [ProtoMember(5)] internal float MaximumDiameter;
                [ProtoMember(6)] internal bool ClosestFirst;
                [ProtoMember(7)] internal BlockTypes[] SubSystems;
                [ProtoMember(8)] internal Threat[] Threats;
                [ProtoMember(9)] internal float MaxTargetDistance;
                [ProtoMember(10)] internal float MinTargetDistance;
                [ProtoMember(11)] internal bool IgnoreDumbProjectiles;
                [ProtoMember(12)] internal bool LockedSmartOnly;
            }


            [ProtoContract]
            public struct AnimationDef
            {
                [ProtoMember(1)] internal PartAnimationSetDef[] WeaponAnimationSets;
                [ProtoMember(2)] internal WeaponEmissive[] Emissives;
                [ProtoMember(3)] internal string[] HeatingEmissiveParts;
                [ProtoMember(4)] internal Dictionary<PartAnimationSetDef.EventTriggers, EventParticle[]> EventParticles;

                [ProtoContract(IgnoreListHandling = true)]
                public struct PartAnimationSetDef
                {
                    public enum EventTriggers
                    {
                        Reloading,
                        Firing,
                        Tracking,
                        Overheated,
                        TurnOn,
                        TurnOff,
                        BurstReload,
                        OutOfAmmo,
                        PreFire,
                        EmptyOnGameLoad,
                        StopFiring,
                        StopTracking
                    }


                    [ProtoMember(1)] internal string[] SubpartId;
                    [ProtoMember(2)] internal string BarrelId;
                    [ProtoMember(3)] internal uint StartupFireDelay;
                    [ProtoMember(4)] internal Dictionary<EventTriggers, uint> AnimationDelays;
                    [ProtoMember(5)] internal EventTriggers[] Reverse;
                    [ProtoMember(6)] internal EventTriggers[] Loop;
                    [ProtoMember(7)] internal Dictionary<EventTriggers, RelMove[]> EventMoveSets;
                    [ProtoMember(8)] internal EventTriggers[] TriggerOnce;
                    [ProtoMember(9)] internal EventTriggers[] ResetEmissives;

                }

                [ProtoContract]
                public struct WeaponEmissive
                {
                    [ProtoMember(1)] internal string EmissiveName;
                    [ProtoMember(2)] internal string[] EmissivePartNames;
                    [ProtoMember(3)] internal bool CycleEmissivesParts;
                    [ProtoMember(4)] internal bool LeavePreviousOn;
                    [ProtoMember(5)] internal Vector4[] Colors;
                    [ProtoMember(6)] internal float[] IntensityRange;
                }

                [ProtoContract]
                public struct EventParticle
                {
                    [ProtoMember(1)] internal string[] EmptyNames;
                    [ProtoMember(2)] internal string[] MuzzleNames;
                    [ProtoMember(3)] internal ParticleDef Particle;
                    [ProtoMember(4)] internal uint StartDelay;
                    [ProtoMember(5)] internal uint LoopDelay;
                    [ProtoMember(6)] internal bool ForceStop;
                }

                [ProtoContract]
                internal struct RelMove
                {
                    public enum MoveType
                    {
                        Linear,
                        ExpoDecay,
                        ExpoGrowth,
                        Delay,
                        Show, //instant or fade
                        Hide, //instant or fade
                    }

                    [ProtoMember(1)] internal MoveType MovementType;
                    [ProtoMember(2)] internal XYZ[] LinearPoints;
                    [ProtoMember(3)] internal XYZ Rotation;
                    [ProtoMember(4)] internal XYZ RotAroundCenter;
                    [ProtoMember(5)] internal uint TicksToMove;
                    [ProtoMember(6)] internal string CenterEmpty;
                    [ProtoMember(7)] internal bool Fade;
                    [ProtoMember(8)] internal string EmissiveName;

                    [ProtoContract]
                    internal struct XYZ
                    {
                        [ProtoMember(1)] internal double x;
                        [ProtoMember(2)] internal double y;
                        [ProtoMember(3)] internal double z;
                    }
                }
            }

            [ProtoContract]
            public struct UpgradeValues
            {
                [ProtoMember(1)] internal string[] Ammo;
                [ProtoMember(2)] internal int RateOfFireMod;
                [ProtoMember(3)] internal int BarrelsPerShotMod;
                [ProtoMember(4)] internal int ReloadMod;
                [ProtoMember(5)] internal int MaxHeatMod;
                [ProtoMember(6)] internal int HeatSinkRateMod;
                [ProtoMember(7)] internal int ShotsInBurstMod;
                [ProtoMember(8)] internal int DelayAfterBurstMod;
                [ProtoMember(9)] internal int AmmoPriority;
            }

            [ProtoContract]
            public struct HardPointDef
            {
                public enum Prediction
                {
                    Off,
                    Basic,
                    Accurate,
                    Advanced,
                }

                [ProtoMember(1)] internal string WeaponName;
                [ProtoMember(2)] internal int DelayCeaseFire;
                [ProtoMember(3)] internal float DeviateShotAngle;
                [ProtoMember(4)] internal double AimingTolerance;
                [ProtoMember(5)] internal Prediction AimLeadingPrediction;
                [ProtoMember(6)] internal LoadingDef Loading;
                [ProtoMember(7)] internal AiDef Ai;
                [ProtoMember(8)] internal HardwareDef HardWare;
                [ProtoMember(9)] internal UiDef Ui;
                [ProtoMember(10)] internal HardPointAudioDef Audio;
                [ProtoMember(11)] internal HardPointParticleDef Graphics;
                [ProtoMember(12)] internal OtherDef Other;
                [ProtoMember(13)] internal bool AddToleranceToTracking;
                [ProtoMember(14)] internal bool CanShootSubmerged;

                [ProtoContract]
                public struct LoadingDef
                {
                    [ProtoMember(1)] internal int ReloadTime;
                    [ProtoMember(2)] internal int RateOfFire;
                    [ProtoMember(3)] internal int BarrelsPerShot;
                    [ProtoMember(4)] internal int SkipBarrels;
                    [ProtoMember(5)] internal int TrajectilesPerBarrel;
                    [ProtoMember(6)] internal int HeatPerShot;
                    [ProtoMember(7)] internal int MaxHeat;
                    [ProtoMember(8)] internal int HeatSinkRate;
                    [ProtoMember(9)] internal float Cooldown;
                    [ProtoMember(10)] internal int DelayUntilFire;
                    [ProtoMember(11)] internal int ShotsInBurst;
                    [ProtoMember(12)] internal int DelayAfterBurst;
                    [ProtoMember(13)] internal bool DegradeRof;
                    [ProtoMember(14)] internal int BarrelSpinRate;
                    [ProtoMember(15)] internal bool FireFullBurst;
                    [ProtoMember(16)] internal bool GiveUpAfterBurst;
                    [ProtoMember(17)] internal bool DeterministicSpin;
                }


                [ProtoContract]
                public struct UiDef
                {
                    [ProtoMember(1)] internal bool RateOfFire;
                    [ProtoMember(2)] internal bool DamageModifier;
                    [ProtoMember(3)] internal bool ToggleGuidance;
                    [ProtoMember(4)] internal bool EnableOverload;
                }


                [ProtoContract]
                public struct AiDef
                {
                    [ProtoMember(1)] internal bool TrackTargets;
                    [ProtoMember(2)] internal bool TurretAttached;
                    [ProtoMember(3)] internal bool TurretController;
                    [ProtoMember(4)] internal bool PrimaryTracking;
                    [ProtoMember(5)] internal bool LockOnFocus;
                    [ProtoMember(6)] internal bool SuppressFire;
                }

                [ProtoContract]
                public struct HardwareDef
                {
                    public enum ArmorState
                    {
                        IsWeapon,
                        Passive,
                        Active,
                    }

                    [ProtoMember(1)] internal float RotateRate;
                    [ProtoMember(2)] internal float ElevateRate;
                    [ProtoMember(3)] internal Vector3D Offset;
                    [ProtoMember(4)] internal bool FixedOffset;
                    [ProtoMember(5)] internal int MaxAzimuth;
                    [ProtoMember(6)] internal int MinAzimuth;
                    [ProtoMember(7)] internal int MaxElevation;
                    [ProtoMember(8)] internal int MinElevation;
                    [ProtoMember(9)] internal float InventorySize;
                    [ProtoMember(10)] internal ArmorState Armor;
                    [ProtoMember(11)] internal int HomeAzimuth;
                    [ProtoMember(12)] internal int HomeElevation;
                }

                [ProtoContract]
                public struct HardPointAudioDef
                {
                    [ProtoMember(1)] internal string ReloadSound;
                    [ProtoMember(2)] internal string NoAmmoSound;
                    [ProtoMember(3)] internal string HardPointRotationSound;
                    [ProtoMember(4)] internal string BarrelRotationSound;
                    [ProtoMember(5)] internal string FiringSound;
                    [ProtoMember(6)] internal bool FiringSoundPerShot;
                    [ProtoMember(7)] internal string PreFiringSound;
                    [ProtoMember(8)] internal uint FireSoundEndDelay;
                }

                [ProtoContract]
                public struct OtherDef
                {
                    [ProtoMember(1)] internal int GridWeaponCap;
                    [ProtoMember(2)] internal int EnergyPriority;
                    [ProtoMember(3)] internal int RotateBarrelAxis;
                    [ProtoMember(4)] internal bool MuzzleCheck;
                    [ProtoMember(5)] internal bool Debug;
                    [ProtoMember(6)] internal double RestrictionRadius;
                    [ProtoMember(7)] internal bool CheckInflatedBox;
                    [ProtoMember(8)] internal bool CheckForAnyWeapon;
                }

                [ProtoContract]
                public struct HardPointParticleDef
                {
                    [ProtoMember(1)] internal ParticleDef Barrel1;
                    [ProtoMember(2)] internal ParticleDef Barrel2;
                }
            }

            [ProtoContract]
            public class AmmoDef
            {
                [ProtoMember(1)] internal string AmmoMagazine;
                [ProtoMember(2)] internal string AmmoRound;
                [ProtoMember(3)] internal bool HybridRound;
                [ProtoMember(4)] internal float EnergyCost;
                [ProtoMember(5)] internal float BaseDamage;
                [ProtoMember(6)] internal float Mass;
                [ProtoMember(7)] internal float Health;
                [ProtoMember(8)] internal float BackKickForce;
                [ProtoMember(9)] internal DamageScaleDef DamageScales;
                [ProtoMember(10)] internal ShapeDef Shape;
                [ProtoMember(11)] internal ObjectsHitDef ObjectsHit;
                [ProtoMember(12)] internal TrajectoryDef Trajectory;
                [ProtoMember(13)] internal AreaDamageDef AreaEffect;
                [ProtoMember(14)] internal BeamDef Beams;
                [ProtoMember(15)] internal ShrapnelDef Shrapnel;
                [ProtoMember(16)] internal GraphicDef AmmoGraphics;
                [ProtoMember(17)] internal AmmoAudioDef AmmoAudio;
                [ProtoMember(18)] internal bool HardPointUsable;
                [ProtoMember(19)] internal AmmoPatternDef Pattern;
                [ProtoMember(20)] internal int EnergyMagazineSize;
                [ProtoMember(21)] internal float DecayPerShot;
                [ProtoMember(22)] internal AmmoEjectionDef Ejection;
                [ProtoMember(23)] internal bool IgnoreWater;

                [ProtoContract]
                public struct DamageScaleDef
                {

                    [ProtoMember(1)] internal float MaxIntegrity;
                    [ProtoMember(2)] internal bool DamageVoxels;
                    [ProtoMember(3)] internal float Characters;
                    [ProtoMember(4)] internal bool SelfDamage;
                    [ProtoMember(5)] internal GridSizeDef Grids;
                    [ProtoMember(6)] internal ArmorDef Armor;
                    [ProtoMember(7)] internal CustomScalesDef Custom;
                    [ProtoMember(8)] internal ShieldDef Shields;
                    [ProtoMember(9)] internal FallOffDef FallOff;
                    [ProtoMember(10)] internal double HealthHitModifier;
                    [ProtoMember(11)] internal double VoxelHitModifier;

                    [ProtoContract]
                    public struct FallOffDef
                    {
                        [ProtoMember(1)] internal float Distance;
                        [ProtoMember(2)] internal float MinMultipler;
                    }

                    [ProtoContract]
                    public struct GridSizeDef
                    {
                        [ProtoMember(1)] internal float Large;
                        [ProtoMember(2)] internal float Small;
                    }

                    [ProtoContract]
                    public struct ArmorDef
                    {
                        [ProtoMember(1)] internal float Armor;
                        [ProtoMember(2)] internal float Heavy;
                        [ProtoMember(3)] internal float Light;
                        [ProtoMember(4)] internal float NonArmor;
                    }

                    [ProtoContract]
                    public struct CustomScalesDef
                    {
                        [ProtoMember(1)] internal CustomBlocksDef[] Types;
                        [ProtoMember(2)] internal bool IgnoreAllOthers;
                    }

                    [ProtoContract]
                    public struct ShieldDef
                    {
                        internal enum ShieldType
                        {
                            Heal,
                            Bypass,
                            Emp,
                            Energy,
                            Kinetic
                        }

                        [ProtoMember(1)] internal float Modifier;
                        [ProtoMember(2)] internal ShieldType Type;
                        [ProtoMember(3)] internal float BypassModifier;
                    }
                }

                [ProtoContract]
                public struct ShapeDef
                {
                    public enum Shapes
                    {
                        LineShape,
                        SphereShape,
                    }

                    [ProtoMember(1)] internal Shapes Shape;
                    [ProtoMember(2)] internal double Diameter;
                }

                [ProtoContract]
                public struct ObjectsHitDef
                {
                    [ProtoMember(1)] internal int MaxObjectsHit;
                    [ProtoMember(2)] internal bool CountBlocks;
                }


                [ProtoContract]
                public struct CustomBlocksDef
                {
                    [ProtoMember(1)] internal string SubTypeId;
                    [ProtoMember(2)] internal float Modifier;
                }

                [ProtoContract]
                public struct GraphicDef
                {
                    [ProtoMember(1)] internal bool ShieldHitDraw;
                    [ProtoMember(2)] internal float VisualProbability;
                    [ProtoMember(3)] internal string ModelName;
                    [ProtoMember(4)] internal AmmoParticleDef Particles;
                    [ProtoMember(5)] internal LineDef Lines;

                    [ProtoContract]
                    public struct AmmoParticleDef
                    {
                        [ProtoMember(1)] internal ParticleDef Ammo;
                        [ProtoMember(2)] internal ParticleDef Hit;
                        [ProtoMember(3)] internal ParticleDef Eject;
                    }

                    [ProtoContract]
                    public struct LineDef
                    {
                        internal enum Texture
                        {
                            Normal,
                            Cycle,
                            Chaos,
                            Wave,
                        }

                        [ProtoMember(1)] internal TracerBaseDef Tracer;
                        [ProtoMember(2)] internal string TracerMaterial;
                        [ProtoMember(3)] internal Randomize ColorVariance;
                        [ProtoMember(4)] internal Randomize WidthVariance;
                        [ProtoMember(5)] internal TrailDef Trail;
                        [ProtoMember(6)] internal OffsetEffectDef OffsetEffect;

                        [ProtoContract]
                        public struct OffsetEffectDef
                        {
                            [ProtoMember(1)] internal double MaxOffset;
                            [ProtoMember(2)] internal double MinLength;
                            [ProtoMember(3)] internal double MaxLength;
                        }

                        [ProtoContract]
                        public struct TracerBaseDef
                        {
                            [ProtoMember(1)] internal bool Enable;
                            [ProtoMember(2)] internal float Length;
                            [ProtoMember(3)] internal float Width;
                            [ProtoMember(4)] internal Vector4 Color;
                            [ProtoMember(5)] internal uint VisualFadeStart;
                            [ProtoMember(6)] internal uint VisualFadeEnd;
                            [ProtoMember(7)] internal SegmentDef Segmentation;
                            [ProtoMember(8)] internal string[] Textures;
                            [ProtoMember(9)] internal Texture TextureMode;

                            [ProtoContract]
                            public struct SegmentDef
                            {
                                [ProtoMember(1)] internal string Material; //retired
                                [ProtoMember(2)] internal double SegmentLength;
                                [ProtoMember(3)] internal double SegmentGap;
                                [ProtoMember(4)] internal double Speed;
                                [ProtoMember(5)] internal Vector4 Color;
                                [ProtoMember(6)] internal double WidthMultiplier;
                                [ProtoMember(7)] internal bool Reverse;
                                [ProtoMember(8)] internal bool UseLineVariance;
                                [ProtoMember(9)] internal Randomize ColorVariance;
                                [ProtoMember(10)] internal Randomize WidthVariance;
                                [ProtoMember(11)] internal string[] Textures;
                                [ProtoMember(12)] internal bool Enable;
                            }
                        }

                        [ProtoContract]
                        public struct TrailDef
                        {
                            [ProtoMember(1)] internal bool Enable;
                            [ProtoMember(2)] internal string Material;
                            [ProtoMember(3)] internal int DecayTime;
                            [ProtoMember(4)] internal Vector4 Color;
                            [ProtoMember(5)] internal bool Back;
                            [ProtoMember(6)] internal float CustomWidth;
                            [ProtoMember(7)] internal bool UseWidthVariance;
                            [ProtoMember(8)] internal bool UseColorFade;
                            [ProtoMember(9)] internal string[] Textures;
                            [ProtoMember(10)] internal Texture TextureMode;

                        }
                    }
                }

                [ProtoContract]
                public struct BeamDef
                {
                    [ProtoMember(1)] internal bool Enable;
                    [ProtoMember(2)] internal bool ConvergeBeams;
                    [ProtoMember(3)] internal bool VirtualBeams;
                    [ProtoMember(4)] internal bool RotateRealBeam;
                    [ProtoMember(5)] internal bool OneParticle;
                }

                [ProtoContract]
                public struct ShrapnelDef
                {
                    [ProtoMember(1)] internal string AmmoRound;
                    [ProtoMember(2)] internal int Fragments;
                    [ProtoMember(3)] internal float ForwardDegrees;
                    [ProtoMember(4)] internal float BackwardDegrees;
                    [ProtoMember(5)] internal float Degrees;
                    [ProtoMember(6)] internal bool Reverse;
                    [ProtoMember(7)] internal bool RandomizeDir;
                }

                [ProtoContract]
                public struct AmmoPatternDef
                {
                    [ProtoMember(1)] internal string[] Ammos;
                    [ProtoMember(2)] internal bool Enable;
                    [ProtoMember(3)] internal float TriggerChance;
                    [ProtoMember(4)] internal bool SkipParent;
                    [ProtoMember(5)] internal bool Random;
                    [ProtoMember(6)] internal int RandomMin;
                    [ProtoMember(7)] internal int RandomMax;
                    [ProtoMember(8)] internal int PatternSteps;
                }

                [ProtoContract]
                public struct AmmoEjectionDef
                {
                    public enum SpawnType
                    {
                        Item,
                        Particle,
                    }
                    [ProtoMember(1)] internal float Speed;
                    [ProtoMember(2)] internal float SpawnChance;
                    [ProtoMember(3)] internal SpawnType Type;
                    [ProtoMember(4)] internal ComponentDef CompDef;

                    [ProtoContract]
                    public struct ComponentDef
                    {
                        [ProtoMember(1)] internal string ItemDefinition;
                        [ProtoMember(2)] internal int LifeTime;
                        [ProtoMember(3)] internal int Delay;
                    }
                }

                [ProtoContract]
                public struct AreaDamageDef
                {
                    public enum AreaEffectType
                    {
                        Disabled,
                        Explosive,
                        Radiant,
                        AntiSmart,
                        JumpNullField,
                        EnergySinkField,
                        AnchorField,
                        EmpField,
                        OffenseField,
                        NavField,
                        DotField,
                        PushField,
                        PullField,
                    }

                    [ProtoMember(1)] internal double AreaEffectRadius; //Depreciated
                    [ProtoMember(2)] internal float AreaEffectDamage; //Depreciated
                    [ProtoMember(3)] internal AreaEffectType AreaEffect;
                    [ProtoMember(4)] internal PulseDef Pulse;
                    [ProtoMember(5)] internal DetonateDef Detonation;
                    [ProtoMember(6)] internal ExplosionDef Explosions;
                    [ProtoMember(7)] internal EwarFieldsDef EwarFields;
                    [ProtoMember(8)] internal AreaInfluence Base;

                    [ProtoContract]
                    public struct AreaInfluence
                    {
                        [ProtoMember(1)] internal double Radius;
                        [ProtoMember(2)] internal float EffectStrength;
                    }


                    [ProtoContract]
                    public struct PulseDef
                    {
                        [ProtoMember(1)] internal int Interval;
                        [ProtoMember(2)] internal int PulseChance;
                        [ProtoMember(3)] internal int GrowTime;
                        [ProtoMember(4)] internal bool HideModel;
                        [ProtoMember(5)] internal bool ShowParticle;
                        [ProtoMember(6)] internal ParticleDef Particle;
                    }

                    [ProtoContract]
                    public struct EwarFieldsDef
                    {
                        [ProtoMember(1)] internal int Duration;
                        [ProtoMember(2)] internal bool StackDuration;
                        [ProtoMember(3)] internal bool Depletable;
                        [ProtoMember(4)] internal double TriggerRange;
                        [ProtoMember(5)] internal int MaxStacks;
                        [ProtoMember(6)] internal PushPullDef Force;
                        [ProtoMember(7)] internal bool DisableParticleEffect;

                        [ProtoContract]
                        public struct PushPullDef
                        {
                            public enum Force
                            {
                                ProjectileLastPosition,
                                ProjectileOrigin,
                                HitPosition,
                                TargetCenter,
                                TargetCenterOfMass,
                            }

                            [ProtoMember(1)] internal Force ForceFrom;
                            [ProtoMember(2)] internal Force ForceTo;
                            [ProtoMember(3)] internal Force Position;
                        }
                    }

                    [ProtoContract]
                    public struct DetonateDef
                    {
                        [ProtoMember(1)] internal bool DetonateOnEnd;
                        [ProtoMember(2)] internal bool ArmOnlyOnHit;
                        [ProtoMember(3)] internal float DetonationRadius;
                        [ProtoMember(4)] internal float DetonationDamage;
                        [ProtoMember(5)] internal int MinArmingTime;
                    }

                    [ProtoContract]
                    public struct ExplosionDef
                    {
                        [ProtoMember(1)] internal bool NoVisuals;
                        [ProtoMember(2)] internal bool NoSound;
                        [ProtoMember(3)] internal float Scale;
                        [ProtoMember(4)] internal string CustomParticle;
                        [ProtoMember(5)] internal string CustomSound;
                        [ProtoMember(6)] internal bool NoShrapnel;
                        [ProtoMember(7)] internal bool NoDeformation;
                    }
                }

                [ProtoContract]
                public struct AmmoAudioDef
                {
                    [ProtoMember(1)] internal string TravelSound;
                    [ProtoMember(2)] internal string HitSound;
                    [ProtoMember(3)] internal float HitPlayChance;
                    [ProtoMember(4)] internal bool HitPlayShield;
                    [ProtoMember(5)] internal string VoxelHitSound;
                    [ProtoMember(6)] internal string PlayerHitSound;
                    [ProtoMember(7)] internal string FloatingHitSound;
                    [ProtoMember(8)] internal string ShieldHitSound;
                }

                [ProtoContract]
                public struct TrajectoryDef
                {
                    internal enum GuidanceType
                    {
                        None,
                        Remote,
                        TravelTo,
                        Smart,
                        DetectTravelTo,
                        DetectSmart,
                        DetectFixed,
                    }

                    [ProtoMember(1)] internal float MaxTrajectory;
                    [ProtoMember(2)] internal float AccelPerSec;
                    [ProtoMember(3)] internal float DesiredSpeed;
                    [ProtoMember(4)] internal float TargetLossDegree;
                    [ProtoMember(5)] internal int TargetLossTime;
                    [ProtoMember(6)] internal int MaxLifeTime;
                    [ProtoMember(7)] internal int FieldTime;
                    [ProtoMember(8)] internal Randomize SpeedVariance;
                    [ProtoMember(9)] internal Randomize RangeVariance;
                    [ProtoMember(10)] internal GuidanceType Guidance;
                    [ProtoMember(11)] internal SmartsDef Smarts;
                    [ProtoMember(12)] internal MinesDef Mines;
                    [ProtoMember(13)] internal float GravityMultiplier;
                    [ProtoMember(14)] internal uint MaxTrajectoryTime;

                    [ProtoContract]
                    public struct SmartsDef
                    {
                        [ProtoMember(1)] internal double Inaccuracy;
                        [ProtoMember(2)] internal double Aggressiveness;
                        [ProtoMember(3)] internal double MaxLateralThrust;
                        [ProtoMember(4)] internal double TrackingDelay;
                        [ProtoMember(5)] internal int MaxChaseTime;
                        [ProtoMember(6)] internal bool OverideTarget;
                        [ProtoMember(7)] internal int MaxTargets;
                        [ProtoMember(8)] internal bool NoTargetExpire;
                        [ProtoMember(9)] internal bool Roam;
                        [ProtoMember(10)] internal bool KeepAliveAfterTargetLoss;
                    }

                    [ProtoContract]
                    public struct MinesDef
                    {
                        [ProtoMember(1)] internal double DetectRadius;
                        [ProtoMember(2)] internal double DeCloakRadius;
                        [ProtoMember(3)] internal int FieldTime;
                        [ProtoMember(4)] internal bool Cloak;
                        [ProtoMember(5)] internal bool Persist;
                    }
                }

                [ProtoContract]
                public struct Randomize
                {
                    [ProtoMember(1)] internal float Start;
                    [ProtoMember(2)] internal float End;
                }
            }

            [ProtoContract]
            public struct ParticleOptionDef
            {
                [ProtoMember(1)] internal float Scale;
                [ProtoMember(2)] internal float MaxDistance;
                [ProtoMember(3)] internal float MaxDuration;
                [ProtoMember(4)] internal bool Loop;
                [ProtoMember(5)] internal bool Restart;
                [ProtoMember(6)] internal float HitPlayChance;
            }


            [ProtoContract]
            public struct ParticleDef
            {
                [ProtoMember(1)] internal string Name;
                [ProtoMember(2)] internal Vector4 Color;
                [ProtoMember(3)] internal Vector3D Offset;
                [ProtoMember(4)] internal ParticleOptionDef Extras;
                [ProtoMember(5)] internal bool ApplyToShield;
                [ProtoMember(6)] internal bool ShrinkByDistance;
            }
        }
    
        [ProtoContract]
        public struct ArmorCompatibilityDef
        {
            internal enum ArmorType
            {
                Light,
                Heavy,
            }
            [ProtoMember(1)] internal string SubtypeId;
            [ProtoMember(2)] internal ArmorType Kind;
        }
    }
}

