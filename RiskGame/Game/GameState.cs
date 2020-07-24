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
    [Serializable]
    public enum GameMode
    {
        NewRisk,
        Classic
    }
    [Serializable]
    public enum GameMap
    {
        Default,
        NewYork
    }
}
