using System;
using JHTGJ.Scene;
using UnityEngine;

namespace JHTGJ.Interaction
{
    [Serializable]
    public class RoomDestinationEntry
    {
        [SerializeField] string label = "客厅";
        [SerializeField] RoomType targetRoom = RoomType.LivingRoom;
        [SerializeField] SpawnSide spawnSide = SpawnSide.Left;

        public string Label => label;
        public RoomType TargetRoom => targetRoom;
        public SpawnSide SpawnSide => spawnSide;
    }
}
