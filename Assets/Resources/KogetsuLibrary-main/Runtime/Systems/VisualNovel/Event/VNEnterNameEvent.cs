using Kogetsu.Library.DesignPatternCore;

#region Documentation - XML
/// <summary>
/// <para> Summary : </para>
/// <para> (TH) : Event สำหรับสั่งเปิด panel กรอกชื่อผู้เล่น </para>
/// <para> (EN) : Event that triggers the player name input panel. </para>
/// </summary>
#endregion
public record struct VNEnterNameEvent : IEvent;
