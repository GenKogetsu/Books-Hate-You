---
name: new-module
description: Create a new runtime module for Kogetsu Library following project conventions (namespace, asmdef reference, folder structure)
---

# New Library Module Creator

สร้าง module ใหม่ใน `Packages/com.kogetsu.library/Runtime/` ตาม convention ของโปรเจกต์

## Convention ที่ต้องตาม

- **Namespace:** `Kogetsu.Library.Core` หรือ `Kogetsu.Library.DesignPatternCore` ขึ้นอยู่กับประเภท
- **Assembly:** อยู่ใน `KogetsuLibrary` (asmdef ที่ root ของ Runtime)
- **โฟลเดอร์:** `Runtime/Core/<SystemName>Core/` หรือ `Runtime/DesignPatternCore/<SystemName>/`
- **ไฟล์ .meta:** Unity สร้างให้เองเมื่อ Editor เปิด — ไม่ต้องสร้างเอง

## กระบวนการ

1. **ถามชื่อ module** — เช่น `Inventory`, `Dialogue`, `Quest`

2. **ถามประเภท**:
   - **Core** — ระบบเกมทั่วไป (Audio, Input, Stats, VN, Animation)
   - **DesignPattern** — Pattern พื้นฐาน (Singleton, Pool, EventBus, StateMachine)

3. **ถาม interface ที่ต้องการ** (ถ้ามี) — เช่น `IResettable`, `IPausable`

4. **สร้างโครงสร้างไฟล์**:

```
Runtime/
└── Core/<Name>Core/          ← หรือ DesignPatternCore/<Name>/
    ├── <Name>Manager.cs      ← MonoBehaviour หรือ Singleton
    └── <Name>Channel.asset   ← ScriptableObject channel (ถ้าต้องการ observer)
```

5. **Template ไฟล์หลัก**:

```csharp
using UnityEngine;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Core
{
    public class <Name>Manager : Singleton<<Name>Manager>
    {
        // ...
    }
}
```

6. **แจ้งผู้ใช้** ให้เปิด Unity Editor เพื่อให้ Unity generate ไฟล์ `.meta` อัตโนมัติ
