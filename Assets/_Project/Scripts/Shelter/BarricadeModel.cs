using System;

namespace Shelter.ShelterDefense
{
    /// <summary>
    /// 바리케이드 하나의 판자 상태(순수 로직). MonoBehaviour와 분리해 두어, 네트워킹 도입 시 이 값(남은 판자 수)만
    /// 서버 권위로 동기화하면 되고 EditMode 테스트도 씬 없이 할 수 있다.
    /// </summary>
    public sealed class BarricadeModel
    {
        private readonly int _hitsPerBoard;
        private int _hitsOnCurrentBoard;

        public int Boards { get; private set; }
        public int MaxBoards { get; }
        public bool IsBreached => Boards == 0;

        public BarricadeModel(int maxBoards, int hitsPerBoard)
        {
            MaxBoards = Math.Max(0, maxBoards);
            _hitsPerBoard = Math.Max(1, hitsPerBoard);
            Boards = MaxBoards;
        }

        /// <summary>좀비 공격 1회를 적용한다. 이번 공격으로 판자가 한 장 떨어졌으면 true.</summary>
        public bool ApplyHit()
        {
            if (IsBreached) return false;

            _hitsOnCurrentBoard++;
            if (_hitsOnCurrentBoard < _hitsPerBoard) return false;

            _hitsOnCurrentBoard = 0;
            Boards--;
            return true;
        }

        /// <summary>판자 한 장을 되붙인다(수리). 이미 최대면 false — 수리 비용 처리는 호출하는 쪽(economy-shop) 책임.</summary>
        public bool TryRepairOne()
        {
            if (Boards >= MaxBoards) return false;

            Boards++;
            _hitsOnCurrentBoard = 0;
            return true;
        }
    }
}
