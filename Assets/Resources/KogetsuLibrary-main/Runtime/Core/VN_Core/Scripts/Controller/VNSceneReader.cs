using TMPro;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;
using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ตัวควบคุมหลักสำหรับระบบ Visual Novel ทำหน้าที่ประมวลผลโหนดเนื้อเรื่อง, จัดการ UI, ตัวละคร และเหตุการณ์ต่างๆ </para>
    /// <para> (EN) : Main controller for the Visual Novel system. Processes story nodes, manages UI, characters, and events. </para>
    /// </summary>
    [CreateHierarchyMenu("KogetsuLibrary/Core")]
    public class VNSceneReader : MonoBehaviour
    {
        #region Field Region

        [Header("Story Data")]
        [Required]
        [SerializeField] private VNSceneSO _currentScene;

        [Header("Obsever Channels")]
        [Required]
        [SerializeField] private BasicMovementInputObserverSO _basicObserverChannel;

        [Required]
        [SerializeField] private AudioObserverSO _audioObserver;

        [Required]
        [SerializeField] private AnimationClip _hideDialogueClip, _showDialogueClip;

        public AudioClip TypingSfx;

        [Header("UI References")]
        [SerializeField] private VNDialogueArea _standardDialogueArea;
        [SerializeField] private VNDialogueArea _logViewDialogueArea;
        [SerializeField] private VNDialogueArea _cinematicDialogueArea;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image  _speakerNameIcon;    // Image สำหรับแสดง icon ชื่อผู้พูด
        [SerializeField] private Sprite _unknownSpeakerIcon; // Sprite ที่ใช้เมื่อ NameDisplayMode = None (ถ้า null → แสดง "?")
        [SerializeField] private VNChoicePanel    _choicePanel;
        [SerializeField] private VNEnterNamePanel _enterNamePanel;

        [Header("Player Data")]
        [SerializeField] private VNPlayerDataSO _playerData;

        [Header("Auto Advance")]
        [SerializeField] private bool  _autoAdvance      = false;
        [SerializeField] private float _autoAdvanceDelay = 2f;

        [Header("Destroy On End")]
        [SerializeField] private List<string> _destroyOnEnd = new();

        [Header("Relationship Display")]
        [SerializeField] private List<Image> _relationshipHearts = new();
        [SerializeField] private float       _blinkSpeed         = 2f;
        [SerializeField] private float       _blinkDuration      = 20f;
        private Coroutine _blinkCoroutine;

        private readonly Queue<string>          _logViewQueue       = new();
        private readonly HashSet<VNCharacterSO>  _previousSpeakerSOs = new();
        private readonly List<Coroutine>         _actionCoroutines   = new();

        [Header("Runtime Data")]
        [ReadOnly]
        [SerializeField] private ushort _currentConversationIndex = 0;
        [SerializeField] private RectTransform _currentDialogueBox;
        [SerializeField] private Animator _currentDialogueAnimator;
        [SerializeField] private TextMeshProUGUI _currentDialogueTMP, _currentSpeakerNameTMP;

        [ReadOnly]
        [SerializeField] private VNDialogueMode _currentDialogueType = VNDialogueMode.None;

        [ReadOnly]
        [SerializeField] private VNCurrentPhase _currentPhase = VNCurrentPhase.None;

        [ReadOnly]
        [SerializeField] private bool _isTyping, _skipTyping, _waitForInput = false;

        [ReadOnly]
        [SerializeField] private bool _waitForChoiceInput = false;

        [ReadOnly]
        [SerializeField] private bool _waitForNameInput = false;

        [ReadOnly]
        [SerializeField] private int _selectedAnswerNumber = 0;

        #endregion //End Field Region

        private void OnEnable() => _basicObserverChannel.OnInteractionChannel += HandleInput;
        private void OnDisable() => _basicObserverChannel.OnInteractionChannel -= HandleInput;

        public void ToggleAutoAdvance() => _autoAdvance = !_autoAdvance;

        private void FinishScene()
        {
            if (_currentScene.UseEndScene)
            {
                foreach (var name in _destroyOnEnd)
                {
                    var obj = GameObject.Find(name);
                    if (obj != null) Destroy(obj);
                }
                BasicSceneEffectController.Instance.LoadScene(_currentScene.EndSceneIndex);
            }
            else
                BasicSceneEffectController.Instance.LoadNextScene(1);
        }

        private void ShowRelationshipBlink(int value)
        {
            if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = StartCoroutine(RelationshipBlinkRoutine(value));
        }

        private IEnumerator RelationshipBlinkRoutine(int value)
        {
            // เปิดเฉพาะหัวใจตาม value (ที่เหลือปิดไว้ใน scene อยู่แล้ว)
            int count = Mathf.Min(value, _relationshipHearts.Count);
            for (int i = 0; i < count; i++)
                if (_relationshipHearts[i] != null) _relationshipHearts[i].enabled = true;

            float elapsed    = 0f;
            float halfPeriod = 0.5f / Mathf.Max(_blinkSpeed, 0.1f);
            float nextToggle = halfPeriod;
            bool  visible    = true;
            while (elapsed < _blinkDuration)
            {
                elapsed += Time.deltaTime;
                if (elapsed >= nextToggle)
                {
                    visible     = !visible;
                    nextToggle += halfPeriod;
                    for (int i = 0; i < count; i++)
                        if (_relationshipHearts[i] != null) _relationshipHearts[i].enabled = visible;
                }
                yield return null;
            }

            // ปิดทั้งหมดหลังหมดเวลา
            for (int i = 0; i < count; i++)
                if (_relationshipHearts[i] != null) _relationshipHearts[i].enabled = false;
            _blinkCoroutine = null;
        }

        private void Awake()
        {
            if (_currentScene != null)
                InitializeActionDefaults(_currentScene);
        }

        private void Start()
        {
            if (_currentScene == null) return;
            SetUp();
            StartVNScene(_currentScene);
        }

        #region Helper Functions Region

        /// <summary>
        /// <para> (TH) : วนทุก conversation ทุก speaker ทุก action — ถ้า field ชื่อ (EmotionName ฯลฯ) ว่างเปล่า
        ///              ให้ตั้งค่า default เป็น option แรกของ character data เพื่อกัน runtime warning </para>
        /// <para> (EN) : Pre-fills empty action name fields (EmotionName, BehaviorAnimationName, SoundEffectName)
        ///              with the first available option from each character's data. Runs once in Awake. </para>
        /// </summary>
        private void InitializeActionDefaults(VNSceneSO scene)
        {
            foreach (var node in scene.Conversations)
            {
                if (node == null) continue;

                if (node.ConversationMode == VNConversationMode.DialogueMode && node.DialogueNode != null)
                {
                    InitializePhaseActionDefaults(node.DialogueNode.EnterPhase);
                    InitializePhaseActionDefaults(node.DialogueNode.MainPhase);
                    InitializePhaseActionDefaults(node.DialogueNode.ExitPhase);
                }
                else if (node.ConversationMode == VNConversationMode.ChoiceMode && node.ChoiceNode != null)
                {
                    InitializePhaseActionDefaults(node.ChoiceNode.QuestionState.EnterPhase);
                    InitializePhaseActionDefaults(node.ChoiceNode.QuestionState.MainPhase);
                    InitializePhaseActionDefaults(node.ChoiceNode.AnswerState.EnterPhase);
                    InitializePhaseActionDefaults(node.ChoiceNode.AnswerState.MainPhase);
                    InitializePhaseActionDefaults(node.ChoiceNode.AnswerState.ExitPhase);
                    foreach (var interact in node.ChoiceNode.InteractStates)
                        foreach (var subNode in interact.SubConversation)
                            if (subNode != null) InitializeActionDefaults_Node(subNode);
                }
            }
        }

        private void InitializeActionDefaults_Node(VNConversationNode node)
        {
            if (node.ConversationMode == VNConversationMode.DialogueMode && node.DialogueNode != null)
            {
                InitializePhaseActionDefaults(node.DialogueNode.EnterPhase);
                InitializePhaseActionDefaults(node.DialogueNode.MainPhase);
                InitializePhaseActionDefaults(node.DialogueNode.ExitPhase);
            }
        }

        private void InitializePhaseActionDefaults(VNChoicePhaseData phase)
        {
            if (phase == null) return;
            foreach (var speakerData in phase.Speakers)
            {
                if (speakerData.Character == null) continue;

                var actions = speakerData.Actions;
                for (int i = 0; i < actions.Count; i++)
                {
                    var action = actions[i]; // struct — must reassign after modification

                    if (action.UseEmotion && string.IsNullOrEmpty(action.EmotionName))
                    {
                        var first = speakerData.Character.GetFirstEmotionName();
                        if (first != null) action.EmotionName = first;
                    }

                    if (action.UseBehaviorAnimation && string.IsNullOrEmpty(action.BehaviorAnimationName))
                    {
                        var first = speakerData.Character.GetFirstAnimationName();
                        if (first != null) action.BehaviorAnimationName = first;
                    }

                    if (action.UseSoundEffect && string.IsNullOrEmpty(action.SoundEffectName))
                    {
                        var first = speakerData.Character.GetFirstSoundEffectName();
                        if (first != null) action.SoundEffectName = first;
                    }

                    actions[i] = action;
                }
            }
        }

        /// <summary>
        /// <para> (TH) : ตั้งค่าเริ่มต้นให้กับตัวควบคุมหลัก โดยเชื่อมโยง UI พื้นฐานของโหมด Standard </para>
        /// <para> (EN) : Prepares initial setup, linking default text components for the Standard mode. </para>
        /// </summary>
        private void SetUp()
        {
            if (_standardDialogueArea == null) return;
            _currentDialogueTMP    = _standardDialogueArea.DialogueTMP;
            _currentSpeakerNameTMP = _standardDialogueArea.SpeakerNameTMP;

            if (_currentSpeakerNameTMP != null) _currentSpeakerNameTMP.text = string.Empty;
            if (_speakerNameIcon != null) _speakerNameIcon.gameObject.SetActive(false);
        }

        private VNDialogueArea GetDialogueArea(VNDialogueMode mode) => mode switch
        {
            VNDialogueMode.Standard => _standardDialogueArea,
            VNDialogueMode.LogView => _logViewDialogueArea,
            VNDialogueMode.Cinematic => _cinematicDialogueArea,
            _ => null
        };

        private void StartVNScene(VNSceneSO chapter)
        {
            if (chapter == null) return;
            _currentScene = chapter;
            _currentConversationIndex = 0;
            _logViewQueue.Clear();
            if (_currentScene.Conversations.Count <= 0) return;
            StartCoroutine(PlayNodeRoutine(_currentScene.Conversations[_currentConversationIndex]));
        }

        private void HandleInput()
        {
            if (_waitForChoiceInput || _waitForNameInput) return;  // ล็อคขณะรอ choice / name input
            if (_isTyping)
            {
                _skipTyping = true;
                SkipActiveActions(); // หยุด delay + ให้ Animator/SFX ไปจุดสิ้นสุดทันที
                return;
            }
            if (!_waitForInput) return;
            _waitForInput = false;
        }

        /// <summary>
        /// <para> (TH) : หยุด coroutine ของ action ที่กำลังทำทั้งหมด และส่ง skip signal ให้ทุก speaker </para>
        /// <para> (EN) : Stops all running action coroutines and sends a skip signal to all active speakers. </para>
        /// </summary>
        /// <summary>
        /// <para> (TH) : เปิด panel กรอกชื่อและรอจนผู้เล่นกด Confirm </para>
        /// <para> (EN) : Shows the name input panel and waits until the player confirms. </para>
        /// </summary>
        private IEnumerator WaitForNameInputRoutine()
        {
            if (_enterNamePanel == null) yield break;
            _waitForNameInput = true;
            _enterNamePanel.Show(() => _waitForNameInput = false);
            while (_waitForNameInput) yield return null;
        }

        private void SkipActiveActions()
        {
            foreach (var cor in _actionCoroutines)
                if (cor != null) StopCoroutine(cor);
            _actionCoroutines.Clear();

            foreach (var so in _previousSpeakerSOs)
                so.SendVNSkipSignel();
        }

        /// <summary>
        /// <para> (TH) : รับค่าคำตอบที่ผู้เล่นเลือก และปลดล็อคสถานะเพื่อรันเนื้อเรื่องต่อไป </para>
        /// <para> (EN) : Receives the selected answer and unlocks the state to continue the story. </para>
        /// </summary>
        public void SelectChoice(int answerNumber)
        {
            if (!_waitForChoiceInput) return;
            _selectedAnswerNumber = answerNumber;
            _waitForChoiceInput = false;
        }

        /// <summary>
        /// <para> (TH) : จัดรูปแบบและแสดงผลข้อความตามโหมดบทสนทนา (Standard, LogView, Cinematic) </para>
        /// <para> (EN) : Formats and displays dialogue text based on the active dialogue mode. </para>
        /// </summary>
        private int FormatDialogueText(VNDialoguePhaseData phase, StringBuilder namesBuilder)
        {
            int visibleCount = 0;

            // แทนที่ {PlayerName} ด้วยชื่อผู้เล่นที่บันทึกไว้
            string dialogueText = (_playerData != null)
                ? phase.DialogueText.Replace("{PlayerName}", _playerData.PlayerName)
                : phase.DialogueText;

            switch (_currentDialogueType)
            {
                case VNDialogueMode.Standard:
                    if (_currentSpeakerNameTMP != null) _currentSpeakerNameTMP.text = namesBuilder.ToString();
                    _currentDialogueTMP.text = dialogueText;
                    break;

                case VNDialogueMode.LogView:
                    string speakerPrefix = namesBuilder.Length > 0 ? $"{namesBuilder} : " : "";
                    _logViewQueue.Enqueue($"{speakerPrefix}{dialogueText}");

                    while (_logViewQueue.Count > 3) _logViewQueue.Dequeue();

                    if (_logViewQueue.Count > 1)
                    {
                        var logsArray = _logViewQueue.ToArray();
                        _currentDialogueTMP.text = string.Join("\n\n", logsArray, 0, logsArray.Length - 1) + "\n\n";
                        _currentDialogueTMP.ForceMeshUpdate();
                        visibleCount = _currentDialogueTMP.textInfo.characterCount;
                    }

                    _currentDialogueTMP.text = string.Join("\n\n", _logViewQueue);
                    break;

                case VNDialogueMode.Cinematic:
                    if (_currentSpeakerNameTMP != null) _currentSpeakerNameTMP.text = string.Empty;
                    _currentDialogueTMP.text = dialogueText;
                    break;
            }

            return visibleCount;
        }

        /// <summary>
        /// <para> (TH) : รวบรวมรายชื่อผู้พูดและสั่งให้ตัวละครแต่ละตัวเริ่มประมวลผลคำสั่ง Action </para>
        /// <para> (EN) : Compiles speaker names and starts executing actions for each character. </para>
        /// </summary>
        /// <remarks>รับ VNChoicePhaseData เพราะ Speakers อยู่ใน base class</remarks>
        private StringBuilder SetupSpeakers(VNChoicePhaseData phase)
        {
            var namesBuilder = new StringBuilder();

            // รวบรวม SO ของ speaker ใน phase ใหม่ (ทำก่อน early return เพื่อให้ EndConversation ทำงานทุกโหมด)
            var newSpeakerSOs = new HashSet<VNCharacterSO>();
            foreach (var sd in phase.Speakers)
                if (sd.Character != null) newSpeakerSOs.Add(sd.Character);

            // ส่ง EndConversation ให้ตัวละครที่หมดช่วงพูดแล้ว (ไม่อยู่ใน phase ใหม่)
            // VNCharacterController จะ skip gracefully ถ้าตัวละครนั้นไม่มี Emotion ชื่อ "EndConversation"
            foreach (var so in _previousSpeakerSOs)
                if (!newSpeakerSOs.Contains(so))
                    so.SendVNEmotionSignel("EndConversation");

            _previousSpeakerSOs.Clear();
            _previousSpeakerSOs.UnionWith(newSpeakerSOs);
            _actionCoroutines.Clear();

            if (_currentDialogueType is VNDialogueMode.None or VNDialogueMode.Cinematic or VNDialogueMode.LogView)
                return namesBuilder;

            var addedNames = new HashSet<string>();

            foreach (var speakerData in phase.Speakers)
            {
                if (speakerData.Character == null) continue;

                if (speakerData.NameDisplayMode == VNNameDisplayMode.Text)
                {
                    var name = speakerData.Character.CharacterName;
                    if (!addedNames.Add(name)) continue;
                    if (namesBuilder.Length > 0) namesBuilder.Append(" , ");
                    namesBuilder.Append(name);
                }
                else if (speakerData.NameDisplayMode == VNNameDisplayMode.CustomText)
                {
                    var name = speakerData.CustomName;
                    if (string.IsNullOrEmpty(name)) continue;
                    if (!addedNames.Add(name)) continue;
                    if (namesBuilder.Length > 0) namesBuilder.Append(" , ");
                    namesBuilder.Append(name);
                }
                else if (speakerData.NameDisplayMode == VNNameDisplayMode.Icon)
                {
                    if (_speakerNameIcon != null && speakerData.Character.NameIcon != null)
                    {
                        _speakerNameIcon.sprite = speakerData.Character.NameIcon;
                        _speakerNameIcon.gameObject.SetActive(true);
                    }
                }
                else if (speakerData.NameDisplayMode == VNNameDisplayMode.None)
                {
                    // None: ใช้ _unknownSpeakerIcon ที่ตั้งไว้ใน VNSceneReader, ถ้า null → แสดง "?"
                    if (_unknownSpeakerIcon != null && _speakerNameIcon != null)
                    {
                        _speakerNameIcon.sprite = _unknownSpeakerIcon;
                        _speakerNameIcon.gameObject.SetActive(true);
                    }
                    else
                    {
                        if (namesBuilder.Length > 0) namesBuilder.Append(" , ");
                        namesBuilder.Append("?");
                    }
                }

                var cor = StartCoroutine(ExecuteActionsRoutine(speakerData, phase.Speakers.Count));
                _actionCoroutines.Add(cor); // track เพื่อ stop เมื่อ skip
            }

            return namesBuilder;
        }

        /// <summary>
        /// <para> (TH) : ตรวจสอบและสั่งใช้งานเหตุการณ์บรรยากาศ หรือเพลงประกอบ หากมีการตั้งค่าไว้ </para>
        /// <para> (EN) : Evaluates and triggers ambient events or background music if configured. </para>
        /// </summary>
        /// <remarks>รับ VNChoicePhaseData เพราะ event fields อยู่ใน base class</remarks>
        private void TriggerPhaseEvents(VNChoicePhaseData phase)
        {
            if (phase.UseCutScene)
                EventBus.Instance.Publish(new VNCutSceneEvent(phase.CutSceneMode == VNCutSceneMode.On));

            if (phase.UseAmbientEvent)
            {
                if (phase.AmbientEventType != VNAmbientEventType.None)
                    EventBus.Instance.Publish(new VNAmbientEvent(phase.AmbientEventType));
#if UNITY_EDITOR
                else Debug.LogWarning($"<b><color=yellow>[Skiped AmbientEvent]</color></b> AmbientEventType is None at Conversation {_currentConversationIndex + 1}");
#endif
            }

            if (phase.OverrideBmgClip)
            {
                if (phase.BmgClip != null) _audioObserver.SendBmgSignal(phase.BmgClip);
#if UNITY_EDITOR
                else Debug.LogWarning($"<b><color=yellow>[Skiped BmgClip]</color></b> BmgClip is null at Conversation {_currentConversationIndex + 1}");
#endif
            }

            if (_currentDialogueType != VNDialogueMode.None && phase.UseVoiceoverClip)
            {
                if (phase.VoiceoverClip != null) _audioObserver.SendVoiceoverSignal(phase.VoiceoverClip);
#if UNITY_EDITOR
                else Debug.LogWarning($"<b><color=yellow>[Skiped VoiceoverClip]</color></b> VoiceoverClip is null at Conversation {_currentConversationIndex + 1}");
#endif
            }
        }

        /// <summary>
        /// <para> (TH) : เปลี่ยนภาพพื้นหลังหากเปิดใช้งาน ChangeBackground ใน phase </para>
        /// <para> (EN) : Updates the background image if ChangeBackground is enabled for this phase. </para>
        /// </summary>
        private void ApplyBackground(VNChoicePhaseData phase)
        {
            if (!phase.ChangeBackground || _backgroundImage == null || phase.BackgroundSprite == null) return;
            _backgroundImage.sprite = phase.BackgroundSprite;
        }
        private void ProcessSoundEffectAction(VNAction action, VNSpeakerData speakerData, ushort actionIndex)
        {
            if (string.IsNullOrEmpty(action.SoundEffectName))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped ActionSoundEffect]</color></b> SoundEffectName is null or empty at [Conversation {_currentConversationIndex + 1}, {_currentPhase}, Action {actionIndex + 1}]");
#endif
                return;
            }

            var soundEffect = speakerData.Character.GetSoundEffect(action.SoundEffectName);
            if (soundEffect == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped ActionSoundEffect]</color></b> SoundEffect '{action.SoundEffectName}' not found in '{speakerData.Character.CharacterName}'");
#endif
                return;
            }

            speakerData.Character.SendVNSoundEffectSignel(soundEffect.Value.SoundEffectClip.name);
        }

        private void ProcessEmotionAction(VNAction action, VNSpeakerData speakerData, int speakersCount, ushort actionIndex)
        {
            if (string.IsNullOrEmpty(action.EmotionName))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped EmotionAction]</color></b> EmotionName is null or empty at [Conversation {_currentConversationIndex + 1}, {_currentPhase}, Action {actionIndex + 1}]");
#endif
                return;
            }

            var emotion = speakerData.Character.GetEmotion(action.EmotionName);
            if (emotion == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped EmotionAction]</color></b> Emotion '{action.EmotionName}' not found in '{speakerData.Character.CharacterName}'");
#endif
                return;
            }

            speakerData.Character.SendVNEmotionSignel(emotion.Value.EmoteClip.name);
        }

        private void ProcessAnimationAction(VNAction action, VNSpeakerData speakerData, ushort actionIndex)
        {
            if (string.IsNullOrEmpty(action.BehaviorAnimationName))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped AnimationAction]</color></b> AnimationName is null or empty at [Conversation {_currentConversationIndex + 1}, {_currentPhase}, Action {actionIndex + 1}]");
#endif
                return;
            }

            var animation = speakerData.Character.GetAnimation(action.BehaviorAnimationName);
            if (animation == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"<b><color=yellow>[Skiped AnimationAction]</color></b> Animation '{action.BehaviorAnimationName}' not found in '{speakerData.Character.CharacterName}'");
#endif
                return;
            }

            speakerData.Character.SendVNAnimationSignal(animation.Value.BehaviorAnimationClip.name);
        }

        private void ApplyTextSettings(VNTextSettings settings)
        {
            if (settings == null || !settings.OverrideGlobalSettings || _currentDialogueTMP == null) return;
            if (settings.CustomFont != null) _currentDialogueTMP.font = settings.CustomFont;
            _currentDialogueTMP.fontSize = settings.FontSize;
            _currentDialogueTMP.color = settings.FontColor;
            _currentDialogueTMP.alignment = settings.Alignment;
        }

        #endregion //End Helper Functions Region

        #region Corotines Region

        /// <summary>
        /// <para> (TH) : ตรวจสอบประเภทของโหนดบทสนทนา (Dialogue หรือ Choice) และเริ่มการทำงานตามรูปแบบนั้น </para>
        /// <para> (EN) : Evaluates the conversation node type and routes to the appropriate execution. </para>
        /// </summary>
        private IEnumerator PlayNodeRoutine(VNConversationNode node)
        {
            switch (node.ConversationMode)
            {
                case VNConversationMode.DialogueMode:
                    yield return DialogueModeRotine(node.DialogueNode);
                    break;
                case VNConversationMode.ChoiceMode:
                    yield return ChoiceModeRotine(node.ChoiceNode);
                    break;
            }

            if (_currentConversationIndex < _currentScene.Conversations.Count)
                StartCoroutine(PlayNodeRoutine(_currentScene.Conversations[_currentConversationIndex]));
        }

        /// <summary>
        /// <para> (TH) : ประมวลผลโหนดในรูปแบบการพูดคุยมาตรฐาน (Enter → Main → Exit) </para>
        /// <para> (EN) : Processes a standard dialogue node through phases (Enter → Main → Exit). </para>
        /// </summary>
        private IEnumerator DialogueModeRotine(VNDialogueNode node)
        {
            if (node.UseEnterPhase) yield return PlayPhaseRoutine(node.EnterPhase, VNCurrentPhase.EnterPhase);

            yield return PlayPhaseRoutine(node.MainPhase, VNCurrentPhase.MainPhase);

            if (node.UseExitPhase) yield return PlayPhaseRoutine(node.ExitPhase, VNCurrentPhase.ExitPhase);

            _currentConversationIndex++;

            if (_currentConversationIndex >= _currentScene.Conversations.Count)
            {
#if UNITY_EDITOR
                Debug.Log("<b><color=#A5D6A7>[VN Engine]</color></b> VNScene Finished!");
#endif
                FinishScene();

                yield break;
            }
        }

        /// <summary>
        /// <para> (TH) : ประมวลผลโหนดตัวเลือก QuestionState → AnswerState → รอเลือก → InteractState → SubConversation </para>
        /// <para> (EN) : Processes a choice node: Question → Answer (show choices) → wait → InteractState → SubConversation. </para>
        /// </summary>
        private IEnumerator ChoiceModeRotine(VNChoiceNode node)
        {
            // --- Question Enter Phase (ครั้งเดียว ไม่วนซ้ำ) ---
            if (node.QuestionState.UseEnterPhase)
                yield return PlayPhaseRoutine(node.QuestionState.EnterPhase, VNCurrentPhase.EnterPhase);

            bool returnToChoice = true;
            while (returnToChoice)
            {
                // --- Question Main Phase ---
                yield return PlayPhaseRoutine(node.QuestionState.MainPhase, VNCurrentPhase.MainPhase);

                // --- Answer Phase (ตัวเลือก) ---
                if (node.AnswerState.UseEnterPhase)
                    yield return PlayPhaseRoutine(node.AnswerState.EnterPhase, VNCurrentPhase.EnterPhase);

                yield return PlayPhaseRoutine(node.AnswerState.MainPhase, VNCurrentPhase.MainPhase);

                _choicePanel?.ShowChoices(node.AnswerState.Choices, SelectChoice);
                _waitForChoiceInput = true;
                while (_waitForChoiceInput) yield return null;
                if (_choicePanel != null) yield return _choicePanel.HideChoices();

                if (node.AnswerState.UseExitPhase)
                    yield return PlayPhaseRoutine(node.AnswerState.ExitPhase, VNCurrentPhase.ExitPhase);

                // --- หาค่า InteractState ที่ตรงกับคำตอบที่เลือก ---
                VNInteractState matchedInteract = default;
                bool hasInteractMatch = false;

                foreach (var interact in node.InteractStates)
                {
                    if (interact.TargetAnswerNumber != _selectedAnswerNumber) continue;
                    matchedInteract = interact;
                    hasInteractMatch = true;
                    break;
                }

                if (!hasInteractMatch) break;

                // --- อัปเดตค่าความสัมพันธ์ ---
                if (matchedInteract.ChangeRelationship && matchedInteract.RelationshipCharacter != null)
                {
                    matchedInteract.RelationshipCharacter.AddRelationship(matchedInteract.RelationshipDelta);
                    ShowRelationshipBlink(matchedInteract.RelationshipCharacter.RelationshipValue);
                }

                // --- เล่น SubConversation ตามคำตอบที่เลือก ---
                foreach (var subNode in matchedInteract.SubConversation)
                {
                    if (subNode == null) continue;
                    yield return PlaySubNodeRoutine(subNode);
                }

                // --- ตรวจว่าต้องวนกลับไปถามใหม่ไหม ---
                returnToChoice = matchedInteract.ReturnToChoicePhase;
            }

            _currentConversationIndex++;

            if (_currentConversationIndex >= _currentScene.Conversations.Count)
            {
#if UNITY_EDITOR
                Debug.Log("<b><color=#A5D6A7>[VN Engine]</color></b> VNScene Finished!");
#endif
                FinishScene();
                yield break;
            }
        }

        /// <summary>
        /// <para> (TH) : สลับกล่องบทสนทนาและเตรียม UI ก่อนเริ่ม Phase </para>
        /// <para> (EN) : Toggles dialogue boxes and prepares UI before starting the phase. </para>
        /// </summary>
        /// <remarks>รับ VNChoicePhaseData เป็น base เพื่อรองรับทั้ง VNDialoguePhaseData และ VNChoicePhaseData</remarks>
        private IEnumerator DialogueSetup(VNChoicePhaseData phase)
        {
            // TextSettings และ DialogueText มีเฉพาะใน VNDialoguePhaseData (subclass)
            if (phase is VNDialoguePhaseData dialoguePhase)
                ApplyTextSettings(dialoguePhase.TextSettings);

            ApplyBackground(phase);

            // VNChoicePhaseData (Answer/Enter phase) — ไม่ยุ่งกับ dialogue box
            // ให้ข้อความคำถามยังคงแสดงอยู่จนกว่าผู้เล่นจะเลือก
            if (phase is not VNDialoguePhaseData)
                yield break;

            bool isModeChanged = _currentDialogueType != phase.DialogueMode;

            if (isModeChanged && _currentDialogueType != VNDialogueMode.None && _currentDialogueAnimator != null)
            {
                var clipName = (phase.OverrideDialogueBoxAnimation && phase.DialogueBoxAnimation != null)
                    ? phase.DialogueBoxAnimation.name
                    : _hideDialogueClip?.name;

                // new: only play if the animator's GO is actually active in hierarchy
                if (clipName != null && _currentDialogueAnimator.gameObject.activeInHierarchy)
                    _currentDialogueAnimator.Play(clipName);

                yield return new WaitForSeconds(1.2f);
            }

            if (_standardDialogueArea != null && _standardDialogueArea.DialogueBox != null)
                _standardDialogueArea.DialogueBox.gameObject.SetActive(false);

            if (_logViewDialogueArea != null && _logViewDialogueArea.DialogueBox != null)
                _logViewDialogueArea.DialogueBox.gameObject.SetActive(false);

            if (_cinematicDialogueArea != null && _cinematicDialogueArea.DialogueBox != null)
                _cinematicDialogueArea.DialogueBox.gameObject.SetActive(false);

            var dialogueArea = GetDialogueArea(phase.DialogueMode);

            if (phase.DialogueMode == VNDialogueMode.None || dialogueArea == null)
            {
                _currentDialogueType = VNDialogueMode.None;
                _currentDialogueBox = null;
                _currentDialogueAnimator = null;
                _currentDialogueTMP = null;
                _currentSpeakerNameTMP = null;
                yield break;
            }

            _currentDialogueBox      = dialogueArea.DialogueBox;
            _currentDialogueAnimator = dialogueArea.DialogueAnimator;
            _currentDialogueTMP      = dialogueArea.DialogueTMP;
            _currentSpeakerNameTMP   = dialogueArea.SpeakerNameTMP;
            _currentDialogueType     = phase.DialogueMode;

            // ChoicePhaseData (Answer Phase) — คงข้อความและชื่อจาก Q Phase ไว้ ไม่ล้าง
            if (phase is VNDialoguePhaseData)
            {
                if (_currentDialogueTMP != null) { _currentDialogueTMP.text = string.Empty; _currentDialogueTMP.maxVisibleCharacters = 0; }
                if (_speakerNameIcon != null)     { _speakerNameIcon.sprite = null; _speakerNameIcon.gameObject.SetActive(false); }
            }

            // new: activate animator's GO first — it may be on a parent that is still inactive
            // (activating only the DialogueBox child does NOT activate an inactive parent)
            if (_currentDialogueAnimator != null && !_currentDialogueAnimator.gameObject.activeSelf)
                _currentDialogueAnimator.gameObject.SetActive(true);

            if (_currentDialogueBox != null) _currentDialogueBox.gameObject.SetActive(true);

            if (isModeChanged && _currentDialogueAnimator != null)
            {
                var clipName = (phase.OverrideDialogueBoxAnimation && phase.DialogueBoxAnimation != null)
                    ? phase.DialogueBoxAnimation.name
                    : _showDialogueClip?.name;

                if (clipName != null) _currentDialogueAnimator.Play(clipName);
                yield return new WaitForSeconds(1.2f);
            }
        }

        /// <summary>
        /// <para> (TH) : ประมวลผลการทำงานทั้งหมดภายใน Phase เดียว (ตัวละคร, ข้อความ, เหตุการณ์) </para>
        /// <para> (EN) : Processes all logic within a single phase (characters, text, events). </para>
        /// </summary>
        /// <remarks>
        /// รับ VNChoicePhaseData เป็น base — ถ้าเป็น VNDialoguePhaseData จะแสดง dialogue text ด้วย
        /// ถ้าเป็นแค่ VNChoicePhaseData (เช่น AnswerState.MainPhase) จะข้าม typing effect
        /// </remarks>
        private IEnumerator PlayPhaseRoutine(VNChoicePhaseData phase, VNCurrentPhase currentPhaseEnum)
        {
            _currentPhase = currentPhaseEnum;

            yield return DialogueSetup(phase);

            // เปิด panel กรอกชื่อก่อน phase นี้จะแสดงข้อความ
            if (phase.UseEnterName)
                yield return WaitForNameInputRoutine();

            var namesBuilder = SetupSpeakers(phase);
            TriggerPhaseEvents(phase);

            // แสดง dialogue text เฉพาะเมื่อ phase เป็น VNDialoguePhaseData และโหมดพร้อมแสดงผล
            if (phase is VNDialoguePhaseData dialoguePhase
                && _currentDialogueType != VNDialogueMode.None)
            {
                if (_currentDialogueTMP != null)
                {
                    int visibleCount = FormatDialogueText(dialoguePhase, namesBuilder);
                    yield return PlayTypingEffect(dialoguePhase, visibleCount);
                }
                else
                {
                    // new: TMP หายไป แต่ยังต้องรอ input ไม่งั้นบทสนทนาจะวิ่งผ่านไปเองโดยไม่รอ
                    _waitForInput = true;
                    while (_waitForInput) yield return null;
                }
            }
            else
            {
                yield return null; // ChoicePhaseData หรือ None mode → ไม่รอ input
            }
        }

        /// <summary>
        /// <para> (TH) : รันเอฟเฟกต์เครื่องพิมพ์ดีดสำหรับข้อความ พร้อมรอให้ผู้เล่นกดยืนยัน </para>
        /// <para> (EN) : Runs the typewriter text effect and waits for player confirmation input. </para>
        /// </summary>
        private IEnumerator PlayTypingEffect(VNDialoguePhaseData phase, int visibleCount)
        {
            _isTyping = true;
            _skipTyping = false;

            _currentDialogueTMP.ForceMeshUpdate();
            int totalVisibleCharacters = _currentDialogueTMP.textInfo.characterCount;

            if (totalVisibleCharacters == 0)
            {
                _isTyping = false;
                yield return new WaitForSeconds(1f);
                yield break;
            }

            // new: guard TypingSpeed ≤ 0 → instant (ป้องกัน 1f/0 = Infinity → WaitForSeconds ค้างตลอดไป)
            float speed = (phase.TextSettings != null && phase.TextSettings.TypingSpeed > 0f)
                ? phase.TextSettings.TypingSpeed
                : 30f;
            float interval = 1f / speed;

            while (visibleCount < totalVisibleCharacters)
            {
                if (_skipTyping)
                {
                    _currentDialogueTMP.maxVisibleCharacters = totalVisibleCharacters;
                    _audioObserver?.SendSfxSignal(null);
                    _audioObserver?.SendVoiceoverSignal(null);
                    _audioObserver?.SendTypingSfxSignal(null);
                    EventBus.Instance.Publish(new VNTypingSkipEvent(_currentConversationIndex, _currentPhase));
                    break;
                }

                visibleCount++;
                _currentDialogueTMP.maxVisibleCharacters = visibleCount;
                _audioObserver?.SendTypingSfxSignal(TypingSfx);
                yield return new WaitForSeconds(interval);
            }

            _audioObserver?.SendTypingSfxSignal(null);
            _isTyping = false;
            _waitForInput = true;
            if (_autoAdvance)
            {
                float t = 0f;
                while (_waitForInput && t < _autoAdvanceDelay)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
                _waitForInput = false;
            }
            else
            {
                while (_waitForInput) yield return null;
            }
        }

        /// <summary>
        /// <para> (TH) : เล่น VNConversationNode ที่อยู่ใน SubConversation — ไม่ยุ่งกับ _currentConversationIndex หรือ main-list </para>
        /// <para> (EN) : Plays a SubConversation node without touching _currentConversationIndex or advancing the main list. </para>
        /// </summary>
        private IEnumerator PlaySubNodeRoutine(VNConversationNode node)
        {
#if UNITY_EDITOR
            Debug.Log($"<b><color=#80CBC4>[VN Sub]</color></b> PlaySubNodeRoutine mode={node.ConversationMode}");
#endif
            switch (node.ConversationMode)
            {
                case VNConversationMode.DialogueMode:
                    yield return PlaySubDialogueRoutine(node.DialogueNode);
                    break;
                case VNConversationMode.ChoiceMode:
                    yield return ChoiceModeRotine(node.ChoiceNode);
                    break;
            }
        }

        private IEnumerator PlaySubDialogueRoutine(VNDialogueNode node)
        {
#if UNITY_EDITOR
            Debug.Log($"<b><color=#80CBC4>[VN Sub]</color></b> PlaySubDialogueRoutine start");
#endif
            if (node.UseEnterPhase) yield return PlayPhaseRoutine(node.EnterPhase, VNCurrentPhase.EnterPhase);
            yield return PlayPhaseRoutine(node.MainPhase, VNCurrentPhase.MainPhase);
            if (node.UseExitPhase) yield return PlayPhaseRoutine(node.ExitPhase, VNCurrentPhase.ExitPhase);
        }

        /// <summary>
        /// <para> (TH) : วนลูปอ่านและใช้งานคำสั่ง (Action) ต่างๆ ของตัวละครภายในเฟสนั้นๆ </para>
        /// <para> (EN) : Iterates and executes the list of actions defined for a character within the current phase. </para>
        /// </summary>
        private IEnumerator ExecuteActionsRoutine(VNSpeakerData speakerData, int speakersCount)
        {
            ushort actionIndex = 0;

            foreach (var action in speakerData.Actions)
            {
                if (action.UseEmotion) ProcessEmotionAction(action, speakerData, speakersCount, actionIndex);
                if (action.UseBehaviorAnimation) ProcessAnimationAction(action, speakerData, actionIndex);
                if (action.UseSoundEffect) ProcessSoundEffectAction(action, speakerData, actionIndex);

                if (action.UseDelay)
                {
                    if (action.DelayTime <= 0f)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning($"<b><color=yellow>[Skiped ActionDelay]</color></b> DelayTime is 0 or Negative at [Conversation {_currentConversationIndex + 1}, {_currentPhase}, Action {actionIndex + 1}]");
#endif
                    }
                    else yield return new WaitForSeconds(action.DelayTime);
                }

                actionIndex++;
                yield return null;
            }
        }

        #endregion //End Corotines Region
    }
}