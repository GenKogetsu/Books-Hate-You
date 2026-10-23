# KogetsuLibrary structure

`Third Party Library/` is vendor code and is excluded from this refactor.

## Runtime

- `Patterns/`: EventBus, ObjectPool, Singleton and StateMachine infrastructure.
- `Systems/`: feature modules (Ability, Animation, Audio, Bullet, Button, Camera,
  Debug, Effect, Event, GameManager, General, Input, Player, Scene, Skill, Stats,
  TimeManager and VisualNovel).
- `Attributes/`: runtime attribute declarations, with drawers in Editor.
- `Extensions/`: shared extensions.
- `Database/`: asset collections.
- `Settings/`: assembly-wide imports.

Each feature owns its controllers, data, events and assets. Redundant `Scripts/`
levels have been removed. The assembly definitions remain at Runtime and Editor
so moving a feature does not change its assembly identity. Editor is restricted
to the Editor platform.

## APIs and dependencies

- `IEventBus` is the messaging contract. `EventDispatcher` implements typed
  dispatch independently of Unity; the existing `EventBus.Instance` component
  delegates to it for scene compatibility. Repeated subscriptions are balanced
  by repeated unsubscriptions.
- `IObjectPoolService` describes Get/Release operations. Existing
  `ObjectPoolManager.Instance` remains the scene entry point. Pool prewarming
  is owned by `ObjectPool<T>`, and the manager applies scene-return policy.
- Movement and animation use their existing context interfaces and state
  machine. Concrete abilities own dimension-specific physics.
- `ISceneLoader` describes scene navigation. Scene entry methods delegate to `LoadScene(int)` and retain existing public
  method names for UnityEvent bindings.
- Audio playback uses a common operation while channel-specific entry points
  retain their one-shot/loop behavior.
- Editor tools use `LibraryAssetLocator` instead of installation-specific paths.
- VisualNovel editor copying uses shared serialized-value readers/writers.

## Compatibility rules

Asset moves use `AssetDatabase` and retain `.meta` GUIDs. Existing namespaces,
serialized fields and public UnityEvent callback names are retained. A public
library type is not considered dead simply because this particular game does
not call it; it can be used by serialized data or external consumers.

The comment-only `PlayerInputObserverSO.cs` placeholder has no class and no
asset references; its input channels already live in
`BasicMovementInputObserverSO`. It is removed with the empty `InputCore 1` folder.

## Verification tools

Tools > KogetsuLibrary provides reference auditing and EventDispatcher contract
checks. The reference audit writes `Logs/KogetsuReferenceAudit.txt` and checks
library/game prefabs plus loaded scenes. It reports broken references, not
optional unassigned fields. `Organize Library Folders` is the repeatable folder
migration used by this refactor.

The VNSceneReader template has pre-existing missing references to a scene
(`23eb1d5a5dfbf6344b01f3e1dc3a1f0c`) and dialogue animation
(`bd24dbabe968aef40962dde746e2e3c6`). Their source assets are absent from this
project. Restore the originals or assign intended content before using that
template; unrelated scene/animation assets must not be substituted automatically.
