using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.AI;
using Battlegrounds.Compiler;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match;
using Battlegrounds.Testing.TestUtil;

namespace Battlegrounds.Testing.Core.Compiler;

[TestFixture]
public class SessionCompilerTest : TestWithCompanies {

    SessionCompiler compiler;
    Company coh3_british, coh3_afrika_korps;

    [SetUp] 
    public void SetUp() {
        compiler = new SessionCompiler();
        coh3_british = GetCompany("coh3_british");
        coh3_afrika_korps = GetCompany("coh3_afrika_korps");
    }

    [Test]
    public void CanCompileCoH3Session() {

        SessionInfo sessionInfo = new SessionInfo() {
            Allies = new SessionParticipant[] {
                new SessionParticipant("CoDiEx", 76561198003529969L, null, ParticipantTeam.TEAM_ALLIES, 0, 0)
            },
            Axis = new SessionParticipant[] {
                new SessionParticipant(AIDifficulty.AI_Hard, null, ParticipantTeam.TEAM_AXIS, 0, 1)
            }
        };
        ISession session = Session.CreateSession(sessionInfo);

        string luacode = compiler.CompileSession(session);
        Assert.That(luacode, Is.Not.Empty);

    }

}
