

using System;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.TimedInput {
[Serializable]
public class TimedNoteComposition {
    public List<InputChannel> inputChannels;

    public void Initialize(TimedInputManager manager) {
        for(int i = 0; i < inputChannels.Count; ++i) {
            inputChannels[i].Initialize(manager);
        }
    }

    public void Cleanup() {
        for(int i = 0; i < inputChannels.Count; ++i) {
            inputChannels[i].Cleanup();
        }
    }
}
}