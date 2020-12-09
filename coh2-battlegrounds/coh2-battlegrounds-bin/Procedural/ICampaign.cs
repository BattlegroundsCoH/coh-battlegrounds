using System;
using System.Collections.Generic;
using System.Text;
using Battlegrounds.Json;

namespace Battlegrounds.Procedural {
    
    public interface ICampaign : IJsonObject {

        bool HasContent { get; }

        int MinPlayer { get; }

        int MaxPlayer { get; }

        void GenerateLinear();

        void FromFile(string presetfile);

    }

}
