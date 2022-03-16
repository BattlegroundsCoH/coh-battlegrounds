using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Game.DataCompany.Builder;
public interface IBuilder {

    bool IsChanged { get; }

    bool CanUndo { get; }

    bool CanRedo { get; }

    void Undo();

    void Redo();

}
