namespace Shelter.ShelterDefense
{
    /// <summary>진입 지점의 종류 — 좀비가 판자를 다 뜯은 뒤 들어오는 방식이 다르다.</summary>
    public enum EntryKind
    {
        /// <summary>출입구: 걸어서 통과한다.</summary>
        Door,
        /// <summary>창문: 창틀을 뛰어넘는다.</summary>
        Window
    }
}
