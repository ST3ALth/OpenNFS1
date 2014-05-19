﻿using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework.Audio;
using NfsEngine;
using NfsEngine;
using Microsoft.Xna.Framework;
using OpenNFS1.Physics;
using System.Diagnostics;
using OpenNFS1.Audio;

namespace OpenNFS1
{
    class VehicleAudioProvider
    {
        Vehicle _car;
        string _soundName;

        SoundEffectInstance _engineOnLow, _engineOnHigh, _engineOffLow, _engineOffHigh, _skidInstance, _offRoadInstance;
        SoundEffect _gearChange, _horn;
        List<SoundEffect> _skids = new List<SoundEffect>();
        Random _random;
                
        public VehicleAudioProvider(Vehicle car, string vehicleSoundName)
        {
            _car = car;
            _soundName = vehicleSoundName;
        }


        public void Initialize()
        {
            if (_random != null)
            {
                return;
            }

            _random = new Random();
            
            SoundEffect temp;
            temp = Engine.Instance.ContentManager.Load<SoundEffect>(String.Format("Content/Audio/Vehicles/{0}/engine-on-low", _soundName));
            _engineOnLow = temp.CreateInstance();
			//.Play(0.3f, 0, 0);
            temp = Engine.Instance.ContentManager.Load<SoundEffect>(String.Format("Content/Audio/Vehicles/{0}/engine-on-high", _soundName));
			_engineOnHigh = temp.CreateInstance(); // temp.Play(0.3f, 0, 0);
            temp = Engine.Instance.ContentManager.Load<SoundEffect>(String.Format("Content/Audio/Vehicles/{0}/engine-off-low", _soundName));
			_engineOffLow = temp.CreateInstance(); //temp.Play(0.3f, 0, 0);
            temp = Engine.Instance.ContentManager.Load<SoundEffect>(String.Format("Content/Audio/Vehicles/{0}/engine-off-high", _soundName));
			_engineOffHigh = temp.CreateInstance(); //temp.Play(0.3f, 0, 0);

            temp = Engine.Instance.ContentManager.Load<SoundEffect>("Content/Audio/Vehicles/common/grass_slide");
			_offRoadInstance = temp.CreateInstance(); //temp.Play(0.3f, 0, 0, true);
            _offRoadInstance.Pause();
            
            _skids.Add(Engine.Instance.ContentManager.Load<SoundEffect>("Content/Audio/Vehicles/common/skid1"));
            _skids.Add(Engine.Instance.ContentManager.Load<SoundEffect>("Content/Audio/Vehicles/common/skid2"));
            _skids.Add(Engine.Instance.ContentManager.Load<SoundEffect>("Content/Audio/Vehicles/common/skid3"));

            _engineOnLow.Pause();
            _engineOnHigh.Pause();
            _engineOffLow.Pause();
            _engineOffHigh.Pause();

            _gearChange = Engine.Instance.ContentManager.Load<SoundEffect>(String.Format("Content/Audio/Vehicles/{0}/gear-change", _soundName));

        }

        public void UpdateEngine()
        {
            float engineRpmFactor = (_car.Motor.Rpm - 0.8f) / _car.Motor.RedlineRpm;
            SoundEffectInstance low, high;

            if (_car.Motor.Throttle == 0 && !_car.Motor.AtRedline)
            {
                _engineOnHigh.Pause();
                _engineOnLow.Pause();
                _engineOffHigh.Resume();
                _engineOffLow.Resume();
                low = _engineOffLow;
                high = _engineOffHigh;
            }
            else
            {
                _engineOnHigh.Resume();
                _engineOnLow.Resume();
                _engineOffHigh.Pause();
                _engineOffLow.Pause();
                low = _engineOnLow;
                high = _engineOnHigh;
            }

            low.Volume = engineRpmFactor > 0.55f ? 0 : 0.3f;
            high.Volume = engineRpmFactor < 0.45f ? 0 : 0.3f;

            if (engineRpmFactor < 0.1f)
            {
                low.Volume = MathHelper.Lerp(0.1f, 0.3f, (engineRpmFactor) * 10);
            }
            
            if (engineRpmFactor > 0.45f && engineRpmFactor < 0.55f)
            {
                low.Volume = MathHelper.Lerp(0.3f, 0f, (engineRpmFactor - 0.45f) * 10);
                high.Volume = MathHelper.Lerp(0, 0.3f, (engineRpmFactor - 0.45f) * 10);
            }

            low.Pitch = engineRpmFactor - 0.1f;
            high.Pitch = engineRpmFactor - 0.5f;
        }

        public void PlaySkid(bool play)
        {
            if (play)
            {
                if (_skidInstance != null && _skidInstance.State == SoundState.Playing)
                    return;
                if (_skidInstance != null && _skidInstance.State != SoundState.Playing)
                {
                    _skidInstance.Resume();
                }
                else
                {
                    _skidInstance = _skids[_random.Next(_skids.Count)].CreateInstance(); //.Play(0.3f, 0, 0);
                }
            }
            else
            {
                if (_skidInstance != null && _skidInstance.State == SoundState.Playing)
                {
                    _skidInstance.Stop();
                    _skidInstance = null;
                }
            }                
        }

        public void ChangeGear()
        {
            _gearChange.Play();
        }

        public void BeepHorn()
        {
            _horn.Play();
        }

        public void HitGround()
        {
            EnvironmentAudioProvider.Instance.PlayCollision(2);
            SoundEngine2.Instance.PlayEffect(_skids[2].CreateInstance(), 0.2f);
        }


        public void PlayOffRoad(bool play)
        {
            if (play)
            {
                _offRoadInstance.Volume = 0.4f;
                if (_offRoadInstance.State == SoundState.Playing)
                    return;
                else
                    _offRoadInstance.Resume();
            }
            else
            {
                _offRoadInstance.Pause();
            }
        }

        public void StopAll()
        {
            if (_skidInstance != null)
                _skidInstance.Stop();
            _engineOffHigh.Stop();
            _engineOffLow.Stop();
            _engineOnHigh.Stop();
            _engineOnLow.Stop();
            
        }
    }
}