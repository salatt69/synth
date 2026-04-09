using EntityStates;
using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Mod;
using RoR2;
using SyncLib.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProjectSynth.States.Synth.Weapon
{
    // TODO: how to not use any other skill when in this (or sustain) state? 
    public class MikuBeamLeap : BaseSkillState
    {
        public float upwardSpeed = 25f;
        public float forwardSpeed = 10f;

        private Vector3 worldLeapVector;
        private bool process;
        private double peakTime;
        private float stopwatch;
        private CameraTargetParams.CameraParamsOverrideHandle cameraParamsOverrideHandle;

        public override void OnEnter()
        {
            base.OnEnter();
            //this.PlayAnimation(this.animationLayerName, this.animationEnterStateName);
            //Util.PlaySound(this.enterSoundString, base.gameObject);
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.Slow80);
                base.characterBody.AddBuff(RoR2Content.Buffs.ElephantArmorBoost);
            }

            process = false;
            peakTime = MusicSync.BeatInterval * 4f;

            if (!base.characterMotor.isGrounded)
            {
                upwardSpeed *= 0.6f;
            }
            base.characterMotor.Motor.ForceUnground();

            Vector3 direction = base.GetAimRay().direction;
            direction.y = 0f;
            direction.Normalize();

            worldLeapVector = Matrix4x4.TRS(
                base.transform.position,
                Util.QuaternionSafeLookRotation(direction, Vector3.up),
                Vector3.one
            ).MultiplyPoint3x4(new Vector3(0f, upwardSpeed, forwardSpeed)) - base.transform.position;
        }

        public override void Update()
        {
            base.Update();
            if (MusicSync.OnBeat() && !process)
            {
                process = true;
                OverrideCameraParams((float)peakTime);
            }

            if (stopwatch >= peakTime - MusicSync.BeatInterval * 0.5f && MusicSync.OnBeat())
            {
                outer.SetNextState(new MikuBeamSustain(cameraParamsOverrideHandle));
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (process)
            {
                process = true;
                stopwatch += base.GetDeltaTime();
                float t = stopwatch / (float)peakTime;
                float derivative = 3f * Mathf.Pow(1f - t, 2f);

                base.characterMotor.velocity = new Vector3(
                    base.characterMotor.velocity.x,
                    0f,
                    base.characterMotor.velocity.z
                );
                base.characterMotor.rootMotion += worldLeapVector * derivative * base.GetDeltaTime();
            }
        }

        private void OverrideCameraParams(float transitionDuration)
        { 
            if (cameraParamsOverrideHandle.isValid) return;

            cameraParamsOverrideHandle = base.cameraTargetParams.AddParamsOverride(new CameraTargetParams.CameraParamsOverrideRequest
            {
                cameraParamsData = SynthAssets.ccpMikuBeam.data,
                priority = 1.0f
            }, transitionDuration);
        }
    }
}