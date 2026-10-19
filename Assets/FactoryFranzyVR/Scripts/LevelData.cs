using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableVector3
{
    public float x, y, z;
    public SerializableVector3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[Serializable]
public class SerializableQuaternion
{
    public float x, y, z, w;
    public SerializableQuaternion(Quaternion q) { x = q.x; y = q.y; z = q.z; w = q.w; }
    public Quaternion ToQuaternion() => new Quaternion(x, y, z, w);
}

[Serializable]
public class ObjectData
{
    public string instanceId;
    public string itemType;
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public SerializableVector3 scale;
    public bool isLocked;
}

[Serializable]
public enum LevelItemType
{
    Start = 0,
    Classic = 1,
    Moving = 2,
    Checkpoint = 3,
    Bumper = 4,
    Trampoline = 5,
    Launcher = 6,
    Fan = 7,
    Finish = 8
}

[Serializable]
public class LevelData
{
    public string levelName;
    public string version = "1.0";
    public string exportedAt;
    public List<ObjectData> objects = new List<ObjectData>();
}