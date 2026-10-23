# รูปแบบเอกสาร KogetsuLibrary

ใช้กับโค้ดที่เขียนเองใน Runtime และ Editor; ไม่แก้ Third Party Library
และไฟล์ generate เช่น HierarchyMenuDefinitions.g.cs

## อ่านโค้ดโดยพับคำอธิบาย

บล็อก `///` อยู่ใน `#region Documentation - XML` ส่วนบล็อก `//` แยกบรรทัด
อยู่ใน `#region Documentation - Notes` กดพับ region ใน editor เพื่อซ่อนเฉพาะ
คำอธิบาย โดยตัว declaration และ body ของเมธอดยังมองเห็นอยู่
คอมเมนต์ที่อยู่ท้ายบรรทัดโค้ดเดิมยังเก็บไว้ตำแหน่งเดิม

## รูปแบบมาตรฐาน

```csharp
#region Documentation - XML
/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : อธิบายหน้าที่ ลำดับการทำงาน เงื่อนไข และผลข้างเคียงที่สำคัญ </para>
/// <para> (EN) : Describe the responsibility, execution order, conditions and significant side effects. </para>
/// <para> Refactor : </para>
/// <para> (TH) : อธิบายสิ่งที่ย้ายหรือแยกมาจากเวอร์ชันก่อน เมื่อมีการเปลี่ยนโครงสร้าง </para>
/// <para> (EN) : Explain what was extracted or reorganized when the implementation changed. </para>
/// </summary>
/// <param name="value">ความหมายของ input / Meaning of the input.</param>
/// <returns>
/// <para> Return : </para>
/// <para> (TH) : ระบุค่าที่คืน รวมถึงกรณี null หรือไม่พบข้อมูล </para>
/// <para> (EN) : Describe the result, including null or missing-data cases. </para>
/// </returns>
#endregion
```

- `Summary :` เป็น `<para>` แยกเสมอ
- `Return :` เป็น `<para>` แยกใน `<returns>` สำหรับเอกสารใหม่ของเมธอดที่คืนค่า
  ส่วน Return ที่อยู่ภายใน summary เดิมยังคงข้อความไว้
- เรียงภาษา TH ก่อน EN ในแต่ละส่วน
- เพิ่ม `Refactor :`, `Notes :`, `Flow :` หรือ `Setup :` เมื่อข้อมูลนั้นมีประโยชน์
- เมธอด void ไม่ต้องมี Return ที่ไม่มีความหมาย
- เก็บ `<see>`, `<paramref>`, `<typeparam>`, `<code>` และตัวอย่างเดิมไว้
- ไม่เปลี่ยนชื่อ serialized field หรือ public API เพื่อจัดเอกสาร

## จุดเริ่มอ่านส่วนที่ refactor

| ส่วน | เอกสารที่ควรอ่าน |
| --- | --- |
| EventBus | `Runtime/Patterns/EventBus/EventDispatcher.cs`: การส่งตามชนิด T, duplicate listeners, unsubscribe และ exception |
| Pool | `Runtime/Patterns/ObjectPool/ObjectPool.cs`: Prewarm และ lifecycle; `ObjectPoolManager.cs`: ReturnPools และ scene policy |
| State | `Runtime/Patterns/StateMachine/StateMachine.cs`: Exit → assign → Enter และ interface ของแต่ละ tick |
| API | `IEventBus`, `IObjectPoolService`, `ISceneLoader`: สัญญาที่ผู้เรียกใช้ |
| Scene | `Runtime/Systems/Scene/SceneEffect/BasicSceneEffectController.cs`: จุดกลางโหลดฉาก และข้อจำกัดการ resolve ชื่อเดิม |
| Audio/Input | `AudioManager.PlaySound`, `BasicInputManager.SendClick`: helper ที่ใช้ร่วมกัน |
| Stats | `StatsController.ClampHp`, `StatsController.SetStamina`: การ clamp และอัปเดต UI |
| Editor | `LibraryAssetLocator.Find`, `LibraryMaintenance`: ค้นหา asset, ย้ายโฟลเดอร์ และ audit reference |
| VN cache | `Editor/VisualNovel/VNCacheHelper.cs`: snapshot ที่ ReadValue/WriteValue รองรับ |

## ตรวจฟอร์แมตซ้ำ

จาก root ของโปรเจกต์:

```text
python Tools/format_library_docs.py --check
```

ถ้าต้องการจัดรูปแบบใหม่ให้รันโดยไม่ใส่ `--check` เครื่องมือจะรักษา BOM/รูปแบบ newline
และตรวจเทียบ token ของโค้ดก่อนเขียนทุกไฟล์ โดยไม่นับคอมเมนต์และ region
เครื่องมือนี้จัดรูปแบบเอกสารที่มีอยู่ ไม่แต่งคำอธิบายเมธอดทุกตัวจากชื่ออัตโนมัติ
