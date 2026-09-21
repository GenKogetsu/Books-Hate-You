# com.kogetsu.library

<div align="center">
  <h1>Kogetsu Library</h1>
  <p><b>Core Utilities, Design Patterns, and Editor Enhancements for Unity 6</b></p>
  <a href="#english-version">English</a> • <a href="#thai-version-ภาษาไทย">ภาษาไทย</a>
</div>

---

<h2 id="english-version">🇬🇧 English Version</h2>

**Kogetsu Library** is a comprehensive, modular framework designed to accelerate game development in Unity 6000.0+. It provides a robust set of core utilities, implementation of essential design patterns, and powerful Editor enhancements.

### 🌟 Key Features

#### 1. Design Pattern Core (`Runtime/DesignPatternCore`)
* **Singleton:** Generic base classes for persistent and non-persistent Managers.
* **Object Pool:** Performance-optimized `ObjectPoolManager` with ScriptableObject-based pool tables.
* **Event Bus:** Decoupled communication using `IEvent` and a global Event Bus.
* **State Machine:** Robust `IState` and `BaseState` for AI and character logic.
* **Movement Controllers:** Ready-to-use 2D/3D controllers (`MoveController2D/3D`) and rotation abilities.

#### 2. Core Game Systems (`Runtime/Core`)
* **Visual Novel Core (VN_Core):** Data-driven system for VN games (Dialogue Nodes, Choices, Speaker Data).
* **Audio & Input Core:** Centralized management via `AudioManager` and `InputObserverChannelSO`.
* **Stats Core:** Modular RPG-like stat containers using ScriptableObjects.

#### 3. Editor Enhancements (`Editor/Assistant`)
* **Doc Assistant:** Generate C# script templates (Enum, Interface, Struct) directly from Unity menus.
* **Tag Assistant:** Advanced Tag and Layer management tools.

#### 4. Integrated Third-Party Libraries
* **NaughtyAttributes:** Enhanced Inspector attributes (e.g., `[Button]`, `[ShowIf]`).
* **Hierarchy Decorator:** Superior hierarchy organization with icons and breadcrumbs.

### 🚀 Installation
1. Open **Unity Package Manager**.
2. Click **+** -> **Add package from git URL...**
3. Enter: `git@github.com:GenKogetsu/kogetsuLibary6.git`

### ⚙️ Requirements
* **Unity:** 6000.0 or higher.
* **C# Version:** Internal enforcement of **C# 10.0** via `csc.rsp`.
* **Dependencies:** `com.unity.textmeshpro` is required and automatically resolved.

### 📄 License
This work is licensed under the CC BY-NC 4.0 (Attribution-NonCommercial) license. Commercial use without permission from Kogetsu is prohibited.

---

<h2 id="thai-version-ภาษาไทย">🇹🇭 Thai Version (ภาษาไทย)</h2>

**Kogetsu Library** คือเฟรมเวิร์กแบบ Modular สำหรับ Unity 6 (6000.0+) ที่รวบรวมระบบพื้นฐาน (Core Utilities) และ Design Pattern ที่สำคัญ เพื่อให้นักพัฒนาสามารถสร้างเกมได้อย่างรวดเร็วและเป็นระบบ

### 🌟 ฟีเจอร์หลัก

#### 1. ระบบ Design Pattern Core (`Runtime/DesignPatternCore`)
* **Singleton:** คลาสพื้นฐานสำหรับสร้าง Manager ที่ปลอดภัย
* **Object Pool:** ระบบจัดการหน่วยความจำเพื่อลดปัญหาการกระตุก (GC Spikes)
* **Event Bus:** ระบบส่งข้อมูลระหว่างสคริปต์แบบลดความผูกพัน (Decoupled)
* **State Machine:** โครงสร้างสำหรับทำระบบ AI หรือ State ของตัวละคร
* **Movement Controllers:** ตัวควบคุมการเคลื่อนที่ 2D และ 3D สำเร็จรูป

#### 2. ระบบเกมพื้นฐาน (`Runtime/Core`)
* **Visual Novel Core:** ระบบฐานข้อมูลสำหรับสร้างเกมแนว Visual Novel ครบวงจร
* **Audio & Input Core:** การจัดการเสียงและการรับค่า Input ผ่าน Observer Channels
* **Stats Core:** ระบบเก็บค่าสถานะตัวละคร (Stats) ผ่าน ScriptableObjects

#### 3. เครื่องมือเสริมสำหรับ Editor (`Editor/Assistant`)
* **Doc Assistant:** เครื่องมือสร้างสคริปต์พื้นฐาน (Enum, Interface, Struct) ผ่านเมนู Unity
* **Tag Assistant:** ระบบจัดการ Tag และ Layer ขั้นสูง

#### 4. ส่วนเสริมจาก Third-Party
* **NaughtyAttributes:** เพิ่มปุ่มและเงื่อนไขการแสดงผลในหน้า Inspector
* **Hierarchy Decorator:** ตกแต่งหน้าต่าง Hierarchy ให้เป็นระเบียบและอ่านง่าย

### 🚀 การติดตั้ง
ติดตั้งผ่าน **Unity Package Manager** โดยใช้ Git URL:
`git@github.com:GenKogetsu/kogetsuLibary6.git`

### ⚙️ ความต้องการของระบบ
* **Unity:** 6000.0 หรือ สูงกว่า.
* **C# Version:** บังคับใช้งาน **C# 10.0** โดยใช้ไฟล์ `csc.rsp`.
* **Dependencies:** `com.unity.textmeshpro` จะถูกติดตั้งโดยอัตโนมัติ.

### 📄 สัญญาอนุญาตสิทธิ์ (License)
อยู่ภายใต้สัญญาอนุญาตแบบ **CC BY-NC 4.0** (แสดงที่มา-ไม่ใช้เพื่อการค้า) **ห้ามใช้ในเชิงพาณิชย์โดยไม่ได้รับอนุญาตจาก kogetsu**

---


<div align="center"><i>Developed by Kogetsu</i></div>
