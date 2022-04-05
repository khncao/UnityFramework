// ref unity fpssample

using System.IO;
using UnityEngine;

namespace m4k.SaveLoad {
public class GameDataReader {
    BinaryReader reader;

    public GameDataReader (BinaryReader reader) {
        this.reader = reader;
    }

    public float ReadFloat(){
        return reader.ReadSingle();
    }

    public short ReadInt16() {
        return reader.ReadInt16();
    }

    public int ReadInt(){
        return reader.ReadInt32();
    }

    public bool ReadBool() {
        return reader.ReadInt32() == 1;
    }

    public string ReadString() {
        return reader.ReadString();
    }

    public Quaternion ReadQuaternion(){
        Quaternion value;
        value.x = reader.ReadSingle();
        value.y = reader.ReadSingle();
        value.z = reader.ReadSingle();
        value.w = reader.ReadSingle();
        return value;
    }

    public Vector3 ReadVector3(){
        Vector3 value;
        value.x = reader.ReadSingle();
        value.y = reader.ReadSingle();
        value.z = reader.ReadSingle();
        return value;
    }

    public void Dispose(){
        reader.Dispose();
    }
}
}