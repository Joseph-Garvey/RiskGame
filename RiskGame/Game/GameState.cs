using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.Game
{
    [Serializable]
    public enum GameState
    {
        None,
        Attacking,
        Conquer,
        Move,
        PlacingArmy,
        InitialArmyPlace
    }
    public enum GameMode
    {
        NewRisk,
        Classic
    }
    public enum GameMap
    {
        Default
    }
}
