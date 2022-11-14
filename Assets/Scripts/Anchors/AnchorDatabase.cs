using System;
using System.Collections.Generic;
using static CustomEnums;
/// <summary>
/// Data structure used to serialize a <see cref="AnchorDatabase"/> object
/// </summary>

namespace SpatialAnchor
{
    [Serializable]
    public class AnchorDatabase
    {
        public List<AnchorData> AnchorData = new();
    }

    [Serializable]
    public class AnchorData
    {
        // String representation of the unique ID of the anchor
        public string SpaceUuid;
        // Name of the prefab associated with this anchor
        public string PrefabName;
        // Location of marker in the room
        public MarkerLocation MarkerLocation;
        // Room that is connected with the anchor
        public ContentRoom ContentRoom;

        public AnchorData(string spaceUuid, string prefabName, MarkerLocation markerLocation, ContentRoom contentRoom)
        {
            SpaceUuid = spaceUuid;
            PrefabName = prefabName;
            MarkerLocation = markerLocation;
            ContentRoom = contentRoom;
        }
    }
}