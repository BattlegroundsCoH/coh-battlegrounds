using Battlegrounds.Lua.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests.LuaLib {
    
    public static class LuaTestUtility {

        public static void VerifyTupleOrder(LuaExpressionList tuple, params string[] expr) {
            if (tuple.Values.Count == expr.Length) {
                for (int i = 0; i < tuple.Values.Count; i++) {
                    Assert.IsInstanceOfType(tuple.Values[i], typeof(LuaIdentifierExpr));
                    Assert.AreEqual(expr[i], (tuple.Values[i] as LuaIdentifierExpr).Identifier);

                }
            } else {
                throw new AssertFailedException($"Tuple order does not match in length. Found <{tuple.Values.Count}>, expected <{expr.Length}>.");
            }
        }

    }

}
