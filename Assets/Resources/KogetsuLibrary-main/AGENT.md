# AGENT.md — AI Coding Guide for Kogetsu Library

คู่มือนี้ให้ AI ของทุกคนในทีม generate และแก้ไขโค้ดภายใน **library** นี้ได้ถูกต้อง  
อ่านก่อนทุกครั้ง — ห้าม guess convention

> Library นี้แยกจาก project เกมที่นำไปใช้  
> โค้ดใน repo นี้คือ **เครื่องมือ** ไม่ใช่ logic ของเกม

---

## 0. Library Overview — อ่านอันนี้ก่อนเลย

### หัวใจของ Library

Library นี้ทำงานบน **3 เสา**:

| เสา | ที่อยู่ | บทบาท |
|-----|---------|-------|
| **DesignPatternCore** | `Runtime/DesignPatternCore/` | รากฐานทุกอย่าง — Singleton, EventBus, ObjectPool, StateMachine |
| **Core Systems** | `Runtime/Core/` | ระบบเกมสำเร็จรูป — Audio, Input, Stats, Animation, VN, Skill |
| **Editor Tools** | `Editor/` | เครื่องมือช่วย Dev — DocAssistant, TagAssistant, VN Editor |

### วิธีทำงานหลัก — Observer SO Pattern

ระบบทุกอย่างใน Core ไหลผ่าน **ScriptableObject Channel**:

```
[ผู้ส่ง] → ObserverSO.SendSignal(data) → [Manager] → handle logic
```

- **ObserverSO** คือสะพานข้อมูล — ไม่มี dependency ตรง ไม่ต้อง GetComponent
- **Manager** (Singleton) subscribe ใน `OnEnable` / unsubscribe ใน `OnDisable` เสมอ
- **ผู้ส่ง** (Input, Trigger, UI) รู้จักแค่ channel SO — ไม่รู้จัก Manager

### การสื่อสารข้ามระบบ — EventBus

เมื่อหลายระบบต้องรับ event เดียวกัน ใช้ **EventBus + IEvent**:

```
[ผู้ส่ง] → EventBus.Publish(new DamageEvent(10f))
[ผู้รับ A] ← EventBus.Subscribe<DamageEvent>(OnDamage)
[ผู้รับ B] ← EventBus.Subscribe<DamageEvent>(OnShake)
```

- IEvent ต้องเป็น `record struct` เสมอ (zero allocation)
- EventBus เป็น Singleton — เข้าถึงผ่าน `EventBus.Instance`

### ลำดับ dependency

```
Third Party (NaughtyAttributes, HierarchyDecorator)
    ↑
DesignPatternCore (Singleton, EventBus, StateMachine, ObjectPool)
    ↑
Core Systems (AudioCore, InputCore, StatsCore, AnimationCore ...)
    ↑
Editor Tools (อ่าน Runtime ได้ แต่ Runtime ห้ามอ่าน Editor)
```

### กฎเหล็กที่ห้ามทำ

- ❌ Runtime อ้าง Editor namespace
- ❌ แก้ไขโค้ดใน `Third Party Library/`
- ❌ Manager ไม่ unsubscribe ใน OnDisable
- ❌ IEvent เป็น class (ต้อง record struct)

---

## กฎการอัปเดตไฟล์นี้

**ทุกครั้งที่ AI เพิ่ม/แก้ไข/ลบสิ่งต่อไปนี้ ต้องอัปเดต AGENT.md และ EDITLOG.md ด้วย:**

- เพิ่ม Core System ใหม่ → อัปเดต Project Structure (section 1) + Overview table
- เปลี่ยน namespace หรือ assembly → อัปเดต section 2 และ Namespace map
- เปลี่ยน naming convention → อัปเดต section 3
- เพิ่ม built-in IEvent → อัปเดต section 8
- เพิ่ม Design Pattern → อัปเดต section 5

---

## 1. Project Structure

```
com.kogetsu.library/
├── Runtime/
│   ├── DesignPatternCore/     ← StateMachine, EventBus, ObjectPool, Singleton
│   │   ├── StateMachine/
│   │   ├── EventBus/
│   │   ├── ObjectPool/
│   │   └── Singleton/
│   ├── Core/                  ← ระบบ reusable ระดับเกม
│   │   ├── AbilityCore/       MoveController2D/3D, BaseMoveAbility
│   │   ├── AnimationCore/     AnimationController2D/3D, DirectionalAnimation
│   │   ├── AudioCore/         AudioManager, AudioObserverSO
│   │   ├── InputCore/         BasicInputManager, MovementInputObserverSO
│   │   ├── StatsCore/         StatsController, BaseStatsDataSO
│   │   ├── SkillCore/         BaseSkillMono, CooldownController
│   │   ├── GameManagerCore/   GameManager, GameState
│   │   ├── TimeManagerCore/   TimeManager
│   │   ├── CameraCore/        CameraFollow3D
│   │   ├── ButtonCore/        BasicButtonStateController
│   │   ├── VN_Core/           Visual Novel system
│   │   └── …
│   ├── Extension/
│   ├── Attribute/
│   └── Database/
├── Editor/
│   ├── Assistant/             DocAssistant, TagAssistant
│   ├── Attribute Helper/
│   ├── VN Editor/
│   └── …
├── Third Party Library/       ← ห้ามแก้ไข
├── package.json               name: "com.kogetsu.library"
├── Runtime/KogetsuLibrary-RuntimeAD.asmdef
└── Editor/KogetsuLibrary-EditorAD.asmdef
```

### Assemblies

| Assembly | rootNamespace | ใช้สำหรับ |
|----------|--------------|-----------|
| `KogetsuLibrary` | `Kogetsu.Library` | ทุกไฟล์ใน `Runtime/` |
| `KogetsuLibrary.Editor` | `Kogetsu.Library.Editor` | ทุกไฟล์ใน `Editor/` |

---

## 2. Namespace Convention

**Rule**: file-scoped namespace สำหรับทุกไฟล์ **ยกเว้น** `MonoBehaviour` และ `ScriptableObject`

```csharp
// ✅ Interface / record struct / enum / plain class → file-scoped
namespace Kogetsu.Library.DesignPatternCore;

public interface IState { }
public record struct EventName(string Name) : IEvent;
```

```csharp
// ✅ MonoBehaviour / ScriptableObject → block namespace
namespace Kogetsu.Library.DesignPatternCore
{
    [Serializable]
    public class StateMachine<TContext> { }
}
```

```csharp
// ✅ Editor script → Kogetsu.Library.Editor
namespace Kogetsu.Library.Editor
{
    [CustomEditor(typeof(StatsController))]
    public class StatsControllerEditor : UnityEditor.Editor { }
}
```

### Namespace map ตามโฟลเดอร์

| โฟลเดอร์ | Namespace |
|----------|-----------|
| `Runtime/DesignPatternCore/` | `Kogetsu.Library.DesignPatternCore` |
| `Runtime/Core/*` | `Kogetsu.Library.Core` |
| `Runtime/Attribute/` | `Kogetsu.Library.Attribute` |
| `Editor/` | `Kogetsu.Library.Editor` |

---

## 3. Naming Convention

| สิ่งที่ตั้งชื่อ | รูปแบบ | ตัวอย่าง |
|----------------|--------|----------|
| Private field | `_camelCase` | `_currentState`, `_instance` |
| `[SerializeField]` | `[SerializeField] private` + `_camelCase` | `[SerializeField] private float _speed` |
| Property | `PascalCase` | `CurrentState { get; private set; }` |
| Method | `PascalCase` | `void ChangeState()`, `T Get<T>()` |
| Interface | `I` prefix | `IState`, `IEnterState`, `IMoveContext2D` |
| Abstract base | `Base` prefix | `BaseState<T>`, `BaseMoveAbility2D` |
| ScriptableObject | `SO` suffix | `BaseStatsDataSO`, `PoolTableDataSO` |
| IEvent | `Event` suffix | `EventName`, `GameStateEvent`, `DealDamageEvent` |
| Observer SO | `ObserverSO` suffix | `BasicMovementInputObserverSO`, `AudioObserverSO` |
| Observer channel | `Channel` suffix | `OnMoveChannel`, `OnJumpChannel` |

---

## 4. C# 10 Syntax

### Fields & Properties

```csharp
// ✅ SerializeField — ใช้ private เสมอ
[SerializeField] private bool _useDontDestroyOnLoad = true;
[SerializeField] private PoolTableDataSO _poolTable;

// ✅ Auto-property read-only จากภายนอก
public BaseState<TContext> CurrentState { get; private set; }
public static T Instance { get; private set; }

// ✅ Target-typed new
private readonly Dictionary<Type, Action<IEvent>> _handlers = new();
private readonly Queue<T> _pool = new();
```

### Features ที่ใช้ใน Library

```csharp
// record struct สำหรับ IEvent (zero allocation)
public record struct GameStateEvent(GameState State) : IEvent;

// Expression body สำหรับ method บรรทัดเดียว
public virtual void Initialize(TContext context) => Context = context;

// Pattern matching
if (CurrentState is IExitState exit) exit.OnExit();
if (CurrentState is IUpdateState update) update.OnUpdate();
```

---

## 5. เพิ่ม Pattern ใหม่ใน DesignPatternCore

ทุก pattern ใหม่อยู่ใน `Runtime/DesignPatternCore/<PatternName>/`  
ดู `StateMachine/` เป็น reference ของโครงสร้าง:

```
StateMachine/
├── IState.cs          ← interfaces (file-scoped namespace)
├── BaseState.cs       ← abstract base ([Serializable], block namespace)
└── StateMachine.cs    ← main class ([Serializable], block namespace)
```

ตัวอย่าง — เพิ่ม `CommandQueue`:

```csharp
// CommandQueue/ICommand.cs
namespace Kogetsu.Library.DesignPatternCore;

public interface ICommand
{
    void Execute();
    void Undo();
}
```

```csharp
// CommandQueue/CommandQueue.cs
namespace Kogetsu.Library.DesignPatternCore
{
    [Serializable]
    public class CommandQueue
    {
        private readonly Stack<ICommand> _history = new();

        public void Execute(ICommand cmd) { cmd.Execute(); _history.Push(cmd); }
        public void Undo() { if (_history.TryPop(out var cmd)) cmd.Undo(); }
    }
}
```

---

## 6. เพิ่ม Core System ใหม่

Core Systems อยู่ใน `Runtime/Core/<Name>Core/`  
ทุก system ใช้ **Observer SO + Manager** pattern:

```
NewThingCore/
├── Scripts/
│   ├── Manager/         ← MonoBehaviour Singleton (block namespace)
│   ├── ObserverChannel/ ← ScriptableObject channel (block namespace)
│   ├── DataContainer/   ← plain structs/classes (file-scoped)
│   └── Interfaces/      ← interfaces (file-scoped)
└── NewThingChannel.asset
```

```csharp
// Observer Channel SO
namespace Kogetsu.Library.Core
{
    [CreateAssetMenu(menuName = "KogetsuLibrary/Core/Observer/NewThingObserver",
                     fileName = "NewThingChannel")]
    public class NewThingObserverSO : ScriptableObject
    {
        public Action<NewThingData> OnChannel;
        public void SendSignal(NewThingData data) => OnChannel?.Invoke(data);
    }
}
```

```csharp
// Manager — subscribe ใน OnEnable, unsubscribe ใน OnDisable
namespace Kogetsu.Library.Core
{
    public class NewThingManager : Singleton<NewThingManager>
    {
        [SerializeField] private NewThingObserverSO _channel;

        private void OnEnable()  => _channel.OnChannel += Handle;
        private void OnDisable() => _channel.OnChannel -= Handle;

        private void Handle(NewThingData data) { ... }
    }
}
```

---

## 7. Singleton\<T\> — Pattern ที่ Library ใช้เอง

```csharp
// ทุก Manager ใน library extend Singleton<T>
// UseDontDestroyOnLoad [SerializeField] อยู่ใน base class แล้ว
namespace Kogetsu.Library.Core
{
    public class AudioManager : Singleton<AudioManager> { ... }
}

namespace Kogetsu.Library.DesignPatternCore
{
    public class EventBus : Singleton<EventBus> { ... }
    public sealed class ObjectPoolManager : Singleton<ObjectPoolManager> { ... }
}
```

---

## 8. IEvent ที่ Library Ship มาให้

Built-in events อยู่ใน `Runtime/DesignPatternCore/EventBus/` และ `Runtime/Core/EventCore/`:

```csharp
// generic — ใช้ได้โดยไม่ต้อง define ใหม่
public record struct EventName(string Name)                            : IEvent;
public record struct EventNameAndTag(string Name, string Tag)          : IEvent;

// game-level
public record struct GameStateEvent(GameState State)                   : IEvent;
public record struct DealDamageEvent(float Damage)                     : IEvent;
public record struct TakeDamageEvent(float Damage, float CurrentHp)   : IEvent;
```

เพิ่ม event ใหม่ใน library: ต้องเป็น `record struct : IEvent` เสมอ

---

## 9. CreateAssetMenu Convention

```csharp
// รูปแบบ: "KogetsuLibrary/<Category>/..."
[CreateAssetMenu(menuName = "KogetsuLibrary/DesignPattern/PoolTable")]
[CreateAssetMenu(menuName = "KogetsuLibrary/Core/BaseStatsData")]
[CreateAssetMenu(menuName = "KogetsuLibrary/DesignPattern/Observer/AudioObserver")]
[CreateAssetMenu(menuName = "KogetsuLibrary/Core/Observer/NewThingObserver")]
```

---

## 10. Editor Script Convention

```csharp
// อยู่ใน Editor/ folder เท่านั้น — namespace Kogetsu.Library.Editor
namespace Kogetsu.Library.Editor
{
    [CustomEditor(typeof(MoveController2D))]
    public class MoveController2DEditor : UnityEditor.Editor { ... }

    [CustomPropertyDrawer(typeof(DirectionalAnimationData))]
    public class DirectionalAnimationDataDrawer : PropertyDrawer { ... }
}
```

---

## 11. Comment Policy

```csharp
// ✅ WHY ที่ไม่ชัดเจน — เขียน
// FindFirstObjectByType fallback เพราะ Awake ของ scene ใหม่อาจยังไม่ถูกเรียก
if (_instance == null) _instance = FindFirstObjectByType<T>();

// ❌ WHAT — ไม่ต้องเขียน
// Reset timer          ← ชัดอยู่แล้ว
_timer = 0f;
```

---

## 12. Checklist ก่อน Commit

- [ ] ไฟล์อยู่ใน folder ที่ถูก (`DesignPatternCore` vs `Core` vs `Editor`)
- [ ] Namespace ถูกต้อง (file-scoped ยกเว้น MB/SO)
- [ ] `[SerializeField] private _camelCase`
- [ ] IEvent เป็น `record struct`
- [ ] `CreateAssetMenu` path ขึ้นต้นด้วย `"KogetsuLibrary/"`
- [ ] Editor script อยู่ใน `Editor/` folder เท่านั้น
- [ ] Comment มีเฉพาะ WHY ที่ซับซ้อน
