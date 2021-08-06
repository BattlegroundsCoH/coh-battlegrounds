using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests.JsonLib {

    [TestClass]
    public class JsonRecordTest {

        public record ValidRecord(List<string> Ls) : IJsonObject {
            public string ToJsonReference() => "";
            public ValidRecord() : this(new List<string>()) { }
        }

        [TestMethod]
        public void CanDetectValidRecord1() {
            var rec = new ValidRecord(new List<string> { "A", "B", "C" });
            Assert.IsTrue(JsonRecord.IsValidRecordType(rec, out _, out _));
        }

        [TestMethod]
        public void CanSerializeValidRecord1() {

            var rec = new ValidRecord(new List<string> { "A", "B", "C" });
            var props = rec.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.AreEqual(props.Length - 1, JsonRecord.Derecord(rec, props).Count());

        }

    }

}
