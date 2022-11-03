using System;
using System.Collections.Generic;
using static CustomEnums;
/// <summary>
/// Data structure used to serialize a <see cref="AnchorData"/> object
/// </summary>
[Serializable]
public class AnchorData
{
    // String representation of the unique ID of the anchor
    public List<string> SpaceUuids;
    // Name of the prefab associated with this anchor
    public List<string> PrefabNames;
    // Location of marker in the room
    public List<MarkerLocation> MarkerLocations;
}
