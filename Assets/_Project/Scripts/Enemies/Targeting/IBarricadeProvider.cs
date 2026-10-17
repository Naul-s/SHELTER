using Shelter.ShelterDefense;
using UnityEngine;

namespace Shelter.Enemies.Targeting
{
    public interface IBarricadeProvider
    {
        /// <summary>origin에서 가장 가까운 바리케이드(판자가 다 뜯겼어도 진입 지점이므로 포함).</summary>
        bool TryGetNearestBarricade(Vector3 origin, out Barricade barricade);
    }
}
