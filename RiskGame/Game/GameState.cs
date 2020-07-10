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
    public enum Die
    {
        PlayerDie1,
        PlayerDie2,
        PlayerDie3,
        EnemyDie1,
        EnemyDie2
    }
}
