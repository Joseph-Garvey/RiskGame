using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.Game
{
    /// <summary>
    /// Current phase of game.
    /// </summary>
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
    /// <summary>
    /// Selected game-mode.
    /// </summary>
    [Serializable]
    public enum GameMode
    {
        NewRisk,
        Classic
    }
    /// <summary>
    /// Current game map.
    /// </summary>
    [Serializable]
    public enum GameMap
    {
        Default,
        NewYork
    }
}
