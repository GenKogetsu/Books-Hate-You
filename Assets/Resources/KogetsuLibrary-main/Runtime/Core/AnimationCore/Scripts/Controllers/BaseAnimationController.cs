using Kogetsu.Library.DesignPatternCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kogetsu.Library.Core
{
    public abstract class BaseAnimationController : MonoBehaviour
    {
        protected StateMachine<AnimationContext> StateMachine;
        protected AnimationContext Context;

        [Header("Directional Animations")]
        [SerializeField] protected DirectionalAnimationData[] IdleAnimationArray;
        [SerializeField] protected DirectionalAnimationData[] MoveAnimationArray;
        [SerializeField] protected DirectionalAnimationData[] JumpAnimationArray;
        [SerializeField] protected DirectionalAnimationData[] FallAnimationArray;

        [Header("Override Animations")]
        [SerializeField] protected List<AnimationClip> ActionAnimations = new();
        protected Dictionary<string, AnimationClip> AnimationDictionary = new();

        protected virtual void OnEnable()
        {
            if (!EventBus.Instance) return;
            
            EventBus.Instance.Subscribe<EventName>(OnPlayAnimationEvent);
        }

        protected virtual void OnDisable()
        {
            if (!EventBus.Instance) return;

            EventBus.Instance.Unsubscribe<EventName>(OnPlayAnimationEvent);
        }

        protected virtual void Awake()
        {
            StateMachine = new StateMachine<AnimationContext>();
            Context = new AnimationContext();

            InitializeAnimationDictionary();
        }

        protected virtual void Update() => StateMachine.Update();

        protected virtual void FixedUpdate() => StateMachine.FixedUpdate();


        protected abstract void PlayOverrideAnimation(AnimationClip clip);

        protected abstract void AssignComponent();

        protected virtual void InitializeAnimationDictionary()
        {
            if (ActionAnimations == null || ActionAnimations.Count == 0) return;

            foreach (var clip in ActionAnimations)
            {
                if (clip != null && !AnimationDictionary.ContainsKey(clip.name))
                {
                    AnimationDictionary.Add(clip.name, clip);
                }
            }
        }

        protected virtual void OnPlayAnimationEvent(EventName Event)
        {
            if (AnimationDictionary.TryGetValue(Event.Name, out var clip))
            {
                PlayOverrideAnimation(clip);
            }
        }

        protected virtual void ResizeArray(byte expected)
        {
            Array.Resize(ref IdleAnimationArray, expected);
            Array.Resize(ref MoveAnimationArray, expected);
            Array.Resize(ref JumpAnimationArray, expected);
            Array.Resize(ref FallAnimationArray, expected);
        }

        
    }
}