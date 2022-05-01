using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattlegroundsApp.LocalData;

/// <summary>
/// Static utility class for creating initial data on first startup
/// </summary>
public static class InitialDataCreator {

    public static void Init() {

        // Create default companies
        CreateDefaultSovietCompany();
        CreateDefaultGermanCompany();

    }


    private static void CreateDefaultSovietCompany() { }

    private static void CreateDefaultGermanCompany() { }

}
