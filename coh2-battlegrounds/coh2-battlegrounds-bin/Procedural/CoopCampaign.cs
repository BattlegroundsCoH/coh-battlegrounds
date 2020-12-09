using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Procedural {

    public class CoopCampaign : ICampaign {
        public bool HasContent => throw new NotImplementedException();

        public int MinPlayer => throw new NotImplementedException();

        public int MaxPlayer => throw new NotImplementedException();

        public void FromFile(string presetfile) => throw new NotImplementedException();
        public void GenerateLinear() => throw new NotImplementedException();
        public virtual string ToJsonReference() => throw new NotImplementedException();
    }

}
