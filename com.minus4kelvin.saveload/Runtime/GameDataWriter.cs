// ref unity fpssample

using System.IO;
using UnityEngine;

namespace m4k.SaveLoad {
public class GameDataWriter {
    BinaryWriter writer;

    public GameDataWriter (BinaryWriter writer) {
        this.writer = writer;
    }

    public void Write (float value) {
        writer.Write(value);
    }

    public void Write (short value) {
        writer.Write(value);
    }

    public void Write (int value) {
        writer.Write(value);
    }

    public void Write (bool value) {
        writer.Write(value ? 1 : 0);
    }

    public void Write (string value) {
        writer.Write(value);
    }

    public void Write (Quaternion value) {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
        writer.Write(value.w);
    }

    public void Write (Vector3 value) {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }
    
    public void Dispose(){
        writer.Dispose();
    }
}
}