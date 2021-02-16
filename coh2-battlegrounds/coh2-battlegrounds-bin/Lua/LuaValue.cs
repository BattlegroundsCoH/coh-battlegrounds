namespace Battlegrounds.Lua {

    public abstract class LuaValue {

        public abstract string Str();

        public abstract bool Equals(LuaValue value);

        public abstract override int GetHashCode();

    }

}
