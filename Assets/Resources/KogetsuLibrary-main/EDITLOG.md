# EDITLOG.md — AI Edit History

บันทึกการเปลี่ยนแปลงที่ AI ทำใน library นี้ทุก version  
เก็บสะสมไปเรื่อยๆ ห้ามลบ entry เก่า

---

## กฎการเขียน EDITLOG

### ต้องเขียนเมื่อไหร่
- AI เพิ่ม / แก้ไข / ลบ ไฟล์ใดก็ตามใน library
- รวมถึง AGENT.md, SKILL.md, EDITLOG.md เอง

### รูปแบบ entry

```
## [vX.Y.Z] — YYYY-MM-DD
**AI:** <ชื่อ model ที่ทำ>

### Added
- <สิ่งที่เพิ่มใหม่>

### Changed
- <สิ่งที่แก้ไข — ระบุว่าเปลี่ยนจากอะไรเป็นอะไร>

### Removed
- <สิ่งที่ลบออก>

### Fixed
- <bug หรือ issue ที่แก้>
```

### กฎ versioning

| เปลี่ยนอะไร | version bump |
|-------------|-------------|
| เพิ่ม Core System / Pattern ใหม่ | Minor `X.Y+1.0` |
| แก้ bug / เปลี่ยน implementation เล็กน้อย | Patch `X.Y.Z+1` |
| เปลี่ยน namespace, assembly, หรือ public API | Major `X+1.0.0` |
| แก้ doc / comment / AGENT.md เท่านั้น | Patch `X.Y.Z+1` |

Version ปัจจุบันดูได้ที่ `package.json` → `"version"`  
**อัปเดต `package.json` version พร้อมกันทุกครั้ง**

---

## [0.2.71] — 2026-05-26
**AI:** Claude Sonnet 4.6

### Fixed
- `CameraFollow3D.Start()` — เปลี่ยนจากดึงค่า `transform.eulerAngles` มาใส่ `_yaw`/`_pitch` เป็นตั้งค่า `0f` ตรงๆ ทำให้ rotation เริ่มต้นเป็น 0,0,0 เสมอ

---

## [0.2.70] — 2026-05-26
**AI:** Claude Sonnet 4.6

### Fixed
- `CameraFollow3D` — `Offset.x` ไม่มีผลต่อ position เพราะ hardcode `new Vector3(0, 0, Offset.z)` แก้เป็น `new Vector3(Offset.x, 0, Offset.z)`
- `CameraFollow3D._pitch` default เปลี่ยนจาก `15f` → `0f`
- `FollowAbility.Offset` default เปลี่ยนจาก `new(0, 10, -10)` → `new(0, 0, 0)`

### Changed
- `CameraFollow3D` — ย้าย `_rotationOffset` ขึ้นมาเป็น field แรกใต้ header **"Transform Offset"** จัดกลุ่ม position offset (`Offset` จาก parent) และ rotation offset ไว้ด้วยกันในใน Inspector

---

## [0.2.69] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Added
- `CameraFollow3D._rotationOffset` (Vector3, default zero) — offset บวกเข้ากับ rotation สุดท้าย: x=pitch offset, y=yaw offset, z=roll offset

---

## [0.2.68] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Added
- `CameraFollow3D._enableMouseLook` (bool, default true) — เปิด/ปิดการหันกล้องตามเมาส์; ถ้าปิดกล้องยังคง follow target และ zoom ได้ตามปกติ
- `CameraFollow3D.ToggleMouseLook()` — toggle เปิด/ปิด
- `CameraFollow3D.SetMouseLook(bool)` — ตั้งค่าตรงๆ

---

## [0.2.67] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `VNSceneReader._destroyOnEnd` — เปลี่ยนจาก `List<GameObject>` เป็น `List<string>`; ใช้ `GameObject.Find(name)` ตอน `FinishScene` เพื่อ Destroy แทนการลาก reference ตรงๆ

---

## [0.2.66] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Added
- `VNSceneReader._destroyOnEnd` (List<GameObject>) — ลาก obj ที่ต้องการ Destroy ใส่ list; จะ Destroy ทุกชิ้นเมื่อ `UseEndScene = true` ก่อน LoadScene

---

## [0.2.65] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `BasicSceneEffectController` — เพิ่ม `LoadScene(int buildIndex)` overload; เพิ่ม `Time.timeScale = 1f` ใน `LoadScene(string)` และ `LoadNextScene(int)`
- `LoadSceneOnClick` — เปลี่ยน `_sceneName` (string) → `_sceneIndex` (int); เรียก `LoadScene(int)`
- `VNSceneSO` — เปลี่ยน `EndSceneName` (string) → `EndSceneIndex` (int)
- `VNSceneReader.FinishScene` — เรียก `LoadScene(int)` แทน `LoadScene(string)`; ลบ `string.IsNullOrEmpty` check ออก (int ไม่ต้องเช็ค)

---

## [0.2.64] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Added
- `LoadSceneOnClick` — MonoBehaviour; `_sceneName` field; `OnClick()` เรียก `LoadScene(name)`
- `LoadNextSceneOnClick` — MonoBehaviour; `OnClick()` เรียก `LoadNextScene(1)`

---

## [0.2.63] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Fixed
- `VNSceneReader.FinishScene` — แก้ StackOverflowException: `else` branch เรียก `FinishScene()` ตัวเองแทน `LoadNextScene(1)` เกิดจาก `replace_all` ตอน v0.2.60 แทนที่ทุก occurrence รวมถึงบรรทัดใน `FinishScene` เองด้วย

---

## [0.2.62] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Added
- `VNSceneReader.ToggleAutoAdvance()` — public method toggle `_autoAdvance`; ค่าเริ่มต้น false; ลาก method ใส่ Button OnClick ได้เลย

---

## [0.2.61] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `VNRelationshipResetter.ResetAll()` — เพิ่ม `_playerData?.ResetName()` reset ชื่อผู้เล่นด้วย; ค่าเริ่มต้นกำหนดได้ใน `VNPlayerDataSO._defaultName` (Inspector)

### Added
- `VNRelationshipResetter._playerData` (VNPlayerDataSO) — ลาก SO มาใส่เพื่อ reset ชื่อผู้เล่นตอน New Game

---

## [0.2.60] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Added
- `VNSceneSO.UseEndScene` (bool) + `EndSceneName` (string) — ถ้าติด flag จะโหลด scene ที่ระบุชื่อไว้แทน scene ถัดไปใน build index
- `VNSceneReader.FinishScene()` — helper รวม logic ตรวจ `UseEndScene`: `true` → `LoadScene(EndSceneName)`, `false` → `LoadNextScene(1)`; ใช้แทน 2 จุดเดิมใน `DialogueModeRotine` และ `ChoiceModeRotine`

---

## [0.2.59] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Fixed
- `VNSceneReader.ChoiceModeRotine` — ลืมเรียก `BasicSceneEffectController.Instance.LoadNextScene(1)` ตอนที่ choice node เป็น conversation สุดท้าย; มีแค่ `yield break` ทำให้ไม่โหลด scene ถัดไป

---

## [0.2.58] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Added
- `VNSceneReader._autoAdvance` (bool, default false) — เปิด/ปิดระบบ auto advance
- `VNSceneReader._autoAdvanceDelay` (float, default 2) — หน่วงกี่วินาทีหลังข้อความแสดงครบก่อนไปบทถัดไป
- `VNSceneReader.PlayTypingEffect` — ถ้า `_autoAdvance` เปิด จะนับเวลา `_autoAdvanceDelay` แล้วไปเองโดยไม่รอ input; ถ้าผู้เล่นคลิกก่อนครบเวลาก็ยังข้ามได้ตามปกติ

---

## [0.2.57] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `VNRelationshipResetter` — เปลี่ยนจาก `[ContextMenu]` เป็น `OnValidate()`; auto-scan `AssetDatabase` ทุกครั้งที่ inspector เปลี่ยนค่า; เพิ่มเฉพาะ SO ที่ยังไม่มีในลิส (ไม่ล้างค่า DefaultValue เดิม)

---

## [0.2.56] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Added
- `VNRelationshipResetter` — MonoBehaviour สำหรับใส่บนปุ่ม; มี `List<CharacterResetData>` (VNCharacterSO + DefaultValue); `public void ResetAll()` reset ทุก character ตาม DefaultValue; `[ContextMenu("Find All VN Characters")]` ใช้ `AssetDatabase.FindAssets` auto-populate list ใน Editor
- `VNCharacterSO.SetRelationship(int)` — ตั้งค่าความสัมพันธ์โดยตรง (clamp 0-10) แล้ว invoke channel

---

## [0.2.55] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Added
- `VNCharacterSO.ResetRelationship()` — reset `_relationshipValue` เป็น 0 แล้ว invoke `OnRelationshipChanged`

---

## [0.2.54] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `AudioManager` — เปลี่ยน click SFX จาก `Mouse.current` ใน `Update()` เป็น subscribe `BasicMovementInputObserverSO.OnInteractionChannel`; ลบ `Update()` ออก

---

## [0.2.53] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Added
- `VNSceneReader.PlayTypingEffect` — ถ้า dialogue text ว่างเปล่า (`totalVisibleCharacters == 0`) จะรอ 1 วินาทีแล้วข้ามไป phase ถัดไปโดยอัตโนมัติ แทนที่จะรอ player input

---

## [0.2.52] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Fixed
- `VNSceneReader.RelationshipBlinkRoutine` — ใช้ assumption ที่ถูกต้อง: หัวใจทุกอันปิดไว้ใน scene ตั้งแต่แรก; เปิดเฉพาะ 0..value-1 แล้ว blink จากนั้นปิดทั้งหมดหลังหมดเวลา; ลบ logic ที่พยายามซ่อนหัวใจที่เกิน value ออก (ไม่จำเป็น)

### Removed
- `VNSceneReader._relationshipDisplayRoot` — ไม่ต้องใช้แล้วเพราะ hearts จัดการผ่าน `Image.enabled` ล้วนๆ

---

## [0.2.51] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Fixed
- `VNSceneReader.RelationshipBlinkRoutine` — แก้หัวใจแสดงผิด (แสดง 10 แทน 4): เปลี่ยนจาก `SetActive(i < value)` + `enabled = true` เป็น `enabled = i < value` อย่างเดียว; `SetActive` + `enabled = true` ทำให้ตอน parent กลับมา active หัวใจที่ควรซ่อนก็โผล่ขึ้นมาด้วย; ใช้ `Image.enabled` ล้วนๆ แทนซึ่งเป็น idiomatic วิธีสำหรับ UI

---

## [0.2.50] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Fixed
- `AudioManager` click SFX — เปลี่ยนกลับเป็น `Mouse.current.leftButton.wasPressedThisFrame` ใน `Update()` เพราะ `ClearAllChannels()` ตอน scene unload ล้าง subscription ของ AudioManager ทิ้ง ทำให้ไม่มีเสียง; ลบ `_basicObserverChannel` field ออก
- `AudioManager.OnTypingSfxSignal` — เปลี่ยนจาก `PlayOneShot` ทุก character เป็น `Play()` ครั้งเดียวตอนเริ่ม (`if isPlaying return`), `Stop()` ตอนรับ null; แก้ปัญหาเสียงซ้อนกันและ loop

---

## [0.2.49] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `AudioManager` — เปลี่ยนจาก `Input.GetMouseButtonDown` (legacy) เป็น subscribe `BasicMovementInputObserverSO.OnLeftClickChannel` (New Input System); ลบ `Update()` ออก; เพิ่ม `_basicObserverChannel` field

---

## [0.2.48] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `AudioManager` — เพิ่ม `_typing` + `_click` AudioSource; subscribe `OnTypingSfxChannel`; เพิ่ม `OnDisable` unsubscribe ทุก channel; เพิ่ม `Update()` ตรวจ `Input.GetMouseButtonDown(0)` เพื่อเล่น `_clickSfx` globally (ทุก Scene คลิกที่ไหนก็ได้)
- `VNSceneReader` — ลบ `ClickSfx` field และ `SendClickSfxSignal` call ออก (AudioManager จัดการ click แทน)

### Added
- `AudioManager._clickSfx` (AudioClip, Inspector) — clip ที่เล่นทุกครั้งที่คลิกซ้าย

---

## [0.2.47] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Added
- `AudioObserverSO.OnTypingSfxChannel` (Action<AudioClip>) + `SendTypingSfxSignal` — channel แยกสำหรับเสียงพิมพ์ ควบคุม volume ได้อิสระจาก SFX ปกติ
- `AudioObserverSO.OnClickSfxChannel` (Action<AudioClip>) + `SendClickSfxSignal` — channel แยกสำหรับเสียงคลิก/continue
- `VNSceneReader.ClickSfx` (AudioClip) — ใส่เสียงคลิกใน inspector
- `VNSceneReader.PlayTypingEffect` — เรียก `SendTypingSfxSignal(TypingSfx)` ทุก character ขณะพิมพ์
- `VNSceneReader.HandleInput` — เรียก `SendClickSfxSignal(ClickSfx)` เมื่อผู้เล่นกด continue

---

## [0.2.46] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Fixed
- `VNCharacterController.OnDisable` — guard `if (EventBus.Instance != null)` ก่อน Unsubscribe เพื่อป้องกัน NullReferenceException เมื่อปิดเกมและ EventBus ถูก destroy ก่อน

---

## [0.2.45] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `VNSceneReader` — เปลี่ยน relationship display จาก static `UpdateRelationshipHearts` เป็นระบบ blink-then-hide: หัวใจซ่อนตลอดเวลา, แสดงและกระพริบเฉพาะเมื่อ `ChangeRelationship` ถูกเรียก, หลังครบ `_blinkDuration` (default 20s) ซ่อนอัตโนมัติ
- `VNSceneReader.SetupSpeakers` — ลบการเรียก `UpdateRelationshipHearts` ออก (ไม่แสดงหัวใจเมื่อเปลี่ยน phase)
- `VNSceneReader.ChoiceModeRotine` — เปลี่ยนจาก `UpdateRelationshipHearts` เป็น `ShowRelationshipBlink`

### Added
- `VNSceneReader._relationshipDisplayRoot` (GameObject, optional) — parent container ของ hearts, ถ้าใส่จะ SetActive ทั้งกลุ่ม; ถ้าไม่ใส่จะ SetActive รายตัว
- `VNSceneReader._blinkSpeed` (float, default 2) — ความเร็วกระพริบ (ครั้งต่อวินาที)
- `VNSceneReader._blinkDuration` (float, default 20) — ระยะเวลาแสดงผล (วินาที)
- `VNSceneReader.ShowRelationshipBlink(int)` — เริ่ม coroutine, หยุด coroutine เก่าก่อนถ้ามี
- `VNSceneReader.RelationshipBlinkRoutine(int)` — แสดงหัวใจตาม value, loop กระพริบด้วย `Image.enabled`, ซ่อนหลังครบ duration

---

## [0.2.44] — 2026-05-21
**AI:** Claude Sonnet 4.6

### Changed
- `VNSceneReader` — ลบ `_relationshipCharacter` field ออก; ใช้ speaker แรกของ phase เป็น source ของ `UpdateRelationshipHearts` แทน (ใน `SetupSpeakers`); อัปเดต hearts ทันทีหลัง `AddRelationship` ใน `ChoiceModeRotine`
- `VNSceneReader.SetupSpeakers` — เพิ่ม `VNNameDisplayMode.CustomText` case: ใช้ `speakerData.CustomName` เป็นชื่อแสดงผล

### Added
- `VNNameDisplayMode.CustomText` — mode ใหม่ให้พิมพ์ชื่อผู้พูดเองได้
- `VNSpeakerData.CustomName` (string) — แสดงในฟิลด์เมื่อเลือก `CustomText`
- `VNSpeakerDataDrawer` — แสดง `Custom Name` field เมื่อ mode เป็น `CustomText` + อัปเดต height

---

## [0.2.43] — 2026-05-20
**AI:** Claude Sonnet 4.6

### Changed
- ย้าย relationship display logic จาก `VNRelationshipDisplay` (ลบแล้ว) เข้า `VNSceneReader` โดยตรง
- `VNSceneReader` — เพิ่ม `_relationshipCharacter` (VNCharacterSO) + `_relationshipHearts` (List<Image>); subscribe `OnRelationshipChanged` ใน OnEnable/OnDisable; `UpdateRelationshipHearts(int)` loop SetActive

### Removed
- `VNRelationshipDisplay.cs` — ไม่ใช้แล้ว ย้าย logic เข้า VNSceneReader

---

## [0.2.42] — 2026-05-20
**AI:** Claude Sonnet 4.6

### Added
- `VNCharacterSO.RelationshipValue` (int 0–10, serialized) + `OnRelationshipChanged` (Action<int>) + `AddRelationship(int delta)` — clamp 0–10 แล้ว invoke channel
- `VNInteractState.ChangeRelationship` (bool) + `RelationshipCharacter` (VNCharacterSO) + `RelationshipDelta` (int) — เพิ่ม/ลดค่าความสัมพันธ์เมื่อผู้เล่นเลือก choice นั้น
- `VNSceneReader.ChoiceModeRotine` — เรียก `AddRelationship` ทันทีหลัง matchedInteract ถูกเลือก
- `VNRelationshipDisplay` — MonoBehaviour subscribe `OnRelationshipChanged`; loop `List<Image>` hearts SetActive(i < value)
- `VNInteractStateDrawer` — draw/reset/height สำหรับ ChangeRelationship + RelationshipCharacter + RelationshipDelta

---

## [0.2.41] — 2026-05-20
**AI:** Claude Sonnet 4.6

### Added
- `VNCutSceneMode` enum (`On` / `Off`) ใน Runtime/Core/VN_Core/Scripts/Enums/
- `VNChoicePhaseData.UseCutScene` (bool) + `CutSceneMode` (VNCutSceneMode) — เพิ่มหลัง `UseEnterName`
- `VNSceneReader.TriggerPhaseEvents` — publish `VNCutSceneEvent(CutSceneMode == On)` เมื่อ `UseCutScene = true`

### Fixed
- `VNConversationNodeDrawer.DrawPhaseContent` — แก้ `UseEnterName` ใช้ `EditorGUI.ToggleLeft` แทน `PropertyField` ให้ checkbox ตรงแถวเดียวกับ bool fields อื่น

---

## [0.2.40] — 2026-05-20
**AI:** Claude Sonnet 4.6

### Added
- `VNCutSceneEvent` — record struct `(bool IsCutScene) : IEvent` ใน Runtime/Core/VN_Core/Scripts/Event/
- `VNCharacterController.OnCutScene` — subscribe `VNCutSceneEvent` จาก EventBus: `IsCutScene = true` → ซ่อน `_characterSprite` GameObject, `false` → แสดงกลับ

---

## [0.2.39] — 2026-05-20
**AI:** Claude Sonnet 4.6

### Fixed
- `VNSceneReader.ChoiceModeRotine` — แก้ SubConversation เล่นแค่ node แรก: เปลี่ยนจาก `yield return PlayNodeRoutine(subNode)` เป็น `yield return PlaySubNodeRoutine(subNode)` ใน foreach loop
- เพิ่ม `PlaySubNodeRoutine` และ `PlaySubDialogueRoutine` — เล่น SubConversation node โดยไม่แตะ `_currentConversationIndex` และไม่ `StartCoroutine` main-list ต่อ ทำให้ foreach วน SubConversation ทุก node ได้ครบ

---

## [0.2.38] — 2026-05-20
**AI:** Claude Sonnet 4.6

### Fixed
- `VNSceneReader.ChoiceModeRotine` — แก้ `ReturnToChoicePhase` ไม่วนกลับ: เปลี่ยนจาก replay MainPhase ครั้งเดียว เป็น `while (returnToChoice)` loop ครอบทั้ง Question→Choices→SubConversation; EnterPhase ยังคงวิ่งครั้งเดียวก่อน loop
- `VNSceneReader.DialogueSetup` — แก้ข้อความคำถามหายตอนแสดง choices: `VNChoicePhaseData` (AnswerState phases) จะ `yield break` ก่อนยุ่ง dialogue box ทำให้ text คงอยู่จนเลือกได้
- `VNSceneReader.HandleInput` — แก้ Continue รั่วขณะ choices แสดง: เพิ่ม guard `if (_waitForChoiceInput || _waitForNameInput) return` ต้นฟังก์ชัน

---

## [0.2.34] — 2026-05-19
**AI:** Claude Sonnet 4.6

### Fixed
- `VNInteractStateDrawer` — แก้ bug `[SerializeReference]` shared reference ใน `List<VNInteractState>`: เมื่อ Unity duplicate element ใน array managed reference IDs จะ copy กัน ทำให้ทุก element ใน list ชี้ไปยัง `SubConversation` object เดียวกัน
- เพิ่ม `CheckAndClearIfNewlyAdded()` method ตรวจจับการเพิ่ม element ใหม่ (array size เพิ่มขึ้น) แล้วเรียก `ResetInteract()` บน element นั้นทันที เพื่อตัด shared reference และให้ข้อมูลเริ่มต้นสะอาด

---

## [0.2.33] — 2026-05-19
**AI:** Claude Sonnet 4.6

### Added
- `BasicMoveAbility3D` — `BypassCameraTransform` bool: เมื่อ `true` จะใช้ input เป็น world-space ตรงๆ ไม่แปลงผ่าน camera transform เหมาะสำหรับ AI ที่ส่ง direction เป็น world-space โดยตรง

---

## [0.2.32] — 2026-05-18
**AI:** Claude Sonnet 4.6

### Removed
- `BaseMoveAbility2D` — ลบ Damage Flash ออก (ซ้ำกับ StatsController Color Lerp)
- `BaseMoveAbility3D` — ลบ Damage Flash ออก (ซ้ำกับ StatsController Color Lerp)

---

## [0.2.31] — 2026-05-18
**AI:** Claude Sonnet 4.6

### Added
- `MoveController3D` — Ground Check: `_enableJump`, `_groundCheck`, `_groundRadius`, `_groundLayer`, `UpdateGroundCheck()` ใน FixedUpdate ด้วย `Physics.CheckSphere`
- `MoveController3D` — Gizmo sphere (สีเขียว = grounded, แดง = airborne) เหมือน 2D

---

## [0.2.30] — 2026-05-18
**AI:** Claude Sonnet 4.6

### Added
- `BaseMoveAbility2D` — Damage Flash สำหรับ SpriteRenderer: `Flash()` protected method, coroutine lerp, Inspector fields (_flashTarget, _flashColor, _flashDuration), auto-cache ใน `Initialize()`
- `BaseMoveAbility3D` — Damage Flash สำหรับ MeshRenderer + MaterialPropertyBlock: ไม่ GC spike, รองรับ custom shader property name

### Removed
- `DirectionMode.OneDiraction` — ซ้ำซ้อนกับ `None` (SnapDirection คืน rawInput เหมือนกันทั้งคู่)
- `DirectionModeExtension.ToByte()` case `OneDiraction => 1`

---

## [0.2.28] — 2026-05-17
**AI:** Claude Sonnet 4.6

### Changed
- Renamed namespace root `Genoverrei.Library.*` → `Kogetsu.Library.*` ทุกไฟล์ใน Runtime และ Editor
- Renamed assembly `GenoverreiLibrary` → `KogetsuLibrary`, `GenoverreiLibrary.Editor` → `KogetsuLibrary.Editor`
- Updated `rootNamespace` ใน asmdef ทั้งสองตัว
- Updated `m_EditorClassIdentifier` ใน scene/prefab/asset files ใน Assets/
- Updated `using` statements ใน `ProjectGlobalUsing.cs` และ `SkillQController.cs`
- Updated tags `*.GenoverreiLibrary` → `*.KogetsuLibrary` ใน TagManager.asset และ prefabs
- Updated `productName` ใน ProjectSettings.asset: `GenoverreiLibary6` → `KogetsuLibrary`
- Updated `package.json` name: `com.genoverrei.library` → `com.kogetsu.library`

### Added
- `ignore.conf` — UVC ignore rules สำหรับ `.claude/`, git, IDE files
- `SKILL.md` — Claude Code skill สำหรับสร้าง module ใหม่ตาม library convention
- `EDITLOG.md` — ไฟล์นี้ (AI edit history)
- Section 0 "Library Overview" ใน AGENT.md — สรุปสถาปัตยกรรม, Observer SO pattern, EventBus flow

### Fixed
- แก้ `com.kogetsu.library` ที่ถูก git track เป็น submodule → เปลี่ยนเป็น regular files ใน main repo
- แก้ git remote URL จาก `genoverrei/GenoverreiLibary6` → `GenKogetsu/KogetsuLibrary`
- แก้ Windows Credential Manager ให้ใช้ account `GenKogetsu` แทน `genoverrei`
