using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Battlegrounds.Campaigns;
using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Gfx;
using Battlegrounds.Locale;
using Battlegrounds.Lua;
using Battlegrounds.Util;

using CampaignArmies = System.Collections.Generic.List<coh2_battlegrounds_console.CampaignCompiler.CampaignArmy>;
using CampaignResources = System.Collections.Generic.List<coh2_battlegrounds_console.CampaignCompiler.CampaignResource>;

using static Battlegrounds.Lua.LuaNil;

namespace coh2_battlegrounds_console {
    
    public static class CampaignCompiler {

        public const uint COMPILER_VERSION = 10;

        public static string Output { get; set; }

        internal enum ResourceType : byte {
            MapImage = 0x1,
            Locale = 0x2,
            CampaignScript = 0x3, // lstate is stored
            MissionScript = 0x4, // Raw script file
            GfxMap = 0x5,
        }

        internal struct CampaignResource {
            public string Identifier;
            public ResourceType Rt;
            public byte[] Content;
        }

        internal struct CampaignDisplay {
            public LocaleKey name;
            public LocaleKey desc;
            public LocaleKey loc;
            public CampaignTheatre theatre;
            public HashSet<CampaignMode> types;
            public CampaignDate startdate;
            public CampaignDate enddate;
            public string start;
            public int playercount;
            public int turntime;
        }

        internal struct CampaignWeather {
            public CampaignDate winterStart;
            public CampaignDate winterEnd;
            public HashSet<string> winterAtmospheres;
            public HashSet<string> summerAtmospheres;
        }

        internal class CampaignArmyGoal {
            public LocaleKey name;
            public LocaleKey desc;
            public int priority;
            public bool hidden;
            public CampaignArmyGoal[] subGoals;
            public string ondone;
            public string onfail;
            public string onui;
            public string ontrigger;
            public byte goalType;
        }

        internal class CampaignArmy {
            public string army;
            public int min;
            public int max;
            public LocaleKey name;
            public LocaleKey desc;
            public LuaTable armyComposition;
            public CampaignArmyGoal[] goals;
        }
        
        public static void Compile(string dir) {

            // Create lua state
            LuaState settingsState = new LuaState();
            settingsState._G["MAP"] = LuaMarshal.ToLuaValue(ResourceType.MapImage);
            settingsState._G["LOCALE"] = LuaMarshal.ToLuaValue(ResourceType.Locale);
            settingsState._G["SCRIPT"] = LuaMarshal.ToLuaValue(ResourceType.CampaignScript);
            settingsState._G["GFX"] = LuaMarshal.ToLuaValue(ResourceType.GfxMap);
            settingsState._G["BINARY"] = LuaMarshal.ToLuaValue("binary");
            settingsState._G["UNARY"] = LuaMarshal.ToLuaValue("unary");
            settingsState._G["TEAM_AXIS"] = LuaMarshal.ToLuaValue("axis");
            settingsState._G["TEAM_ALLIES"] = LuaMarshal.ToLuaValue("allies");
            settingsState._G["OT_PRIMARY"] = 0;
            settingsState._G["OT_SECONDARY"] = 1;
            settingsState._G["OT_SPECIAL"] = 2;

            // Verify file exists
            string settingsFile = Path.Combine(dir, "campaign.lua");
            if (!File.Exists(settingsFile)) {
                Console.WriteLine("Invalid campaign folder.");
                return;
            }

            // List of resources loaded
            CampaignResources campaignResources = new ();
            CampaignArmies campaignArmies = new ();
            CampaignDisplay campaignDisplay = default;
            CampaignWeather campaignWeather = default;

            // Load lua file
            if (LuaVM.DoFile(settingsState, settingsFile) is LuaNil) {
                if (settingsState._G["campaign"] is not LuaTable manifest) {
                    Console.WriteLine("Invalid campaign file. Global table 'campaign' is missing");
                    return;
                }
                manifest.Pairs((k,v) => {
                    switch (k.Str()) {
                        case "display":
                            if (v is LuaTable d) {
                                if (!ParseDisplay(d, out campaignDisplay)) {
                                    Console.WriteLine("Failed to read display data!");
                                }
                            }
                            break;
                        case "armies":
                            if (v is LuaTable t) {
                                ParseArmies(t, dir, campaignArmies);
                            }
                            break;
                        case "resources":
                            if (v is LuaTable resources) {
                                resources.Pairs((k, v) => {
                                    if (v is LuaTable t) {
                                        if (ParseResource(t, dir, out CampaignResource res)) {
                                            campaignResources.Add(res);
                                        } else {
                                            Console.WriteLine($"Failed to load resource #{k.Str()}");
                                        }
                                    }
                                });
                            }
                            break;
                        case "weather":
                            if (v is LuaTable w) {
                                if (!ParseWeather(w, out campaignWeather)) {
                                    Console.WriteLine("Failed to read weather data!");
                                }
                            }
                            break;
                        default:
                            Console.WriteLine($"Undefined entry '{k.Str()}'");
                            break;
                    }
                });
            } else {
                Console.WriteLine("Failed read campaign.lua");
                Console.WriteLine(settingsState.GetError());
                return;
            }

            // Locate mapdef path
            string mapdefPath = Path.Combine(dir, "mapdef.lua");
            if (!File.Exists(mapdefPath)) {
                Console.WriteLine("Failed find mapdef.lua");
                return;
            }

            // Load mapdef
            LuaTable mapdef = null;
            if (LuaVM.DoFile(settingsState, mapdefPath) is not LuaNil) {
                Console.WriteLine("Failed read mapdef.lua");
                return;
            } else {
                mapdef = settingsState._G["map"] as LuaTable;
            }

            // Default localeID
            string id = Localize.UndefinedSource;

            // Fix locale IDS (if any locale)
            if (campaignResources.FirstOrDefault(x => x.Rt == ResourceType.Locale) is CampaignResource rs) {
                
                // Fix campaign IDS

                id = rs.Identifier;
                for (int i = 0; i < campaignArmies.Count; i++) { 
                    campaignArmies[i].name = campaignArmies[i].name with { LocaleSource = id };
                    campaignArmies[i].desc = campaignArmies[i].desc with { LocaleSource = id };
                }
                
                // Fix displays
                campaignDisplay.name = campaignDisplay.name with { LocaleSource = id };
                campaignDisplay.desc = campaignDisplay.desc with { LocaleSource = id };
                campaignDisplay.loc = campaignDisplay.loc with { LocaleSource = id };

            }            

            Compile(dir, id, campaignDisplay, campaignWeather, campaignArmies, campaignResources, mapdef);

        }

        private static void Compile(string relative, string iddef, CampaignDisplay display, CampaignWeather weather, CampaignArmies armies, CampaignResources resources, LuaTable mapdef) {

            // Output path
            string output = Output ?? Path.Combine(relative, Path.GetFileNameWithoutExtension(relative) + ".dat");

            // Log where it's saved to
            Console.WriteLine($"Saving binary to: {output}");

            // Open overall binary writer
            using BinaryWriter bw = new BinaryWriter(File.Open(output, FileMode.Create), Encoding.Unicode);

            // Write intro
            bw.Write(2021); // Identifying number, very obvious where this is from...
            bw.Write("BG\x04\x05\x05\x04".Encode()); // BG (+ 'magic')
            bw.Write(COMPILER_VERSION); // Write version
            bw.Write(resources.Count); // Write amount of resources
            bw.Write(armies.Count); // Write the amount of armies

            // Write default locale string
            byte[] id = iddef.Encode(Encoding.Unicode);
            bw.Write(id.Length);
            bw.Write(id);

            // Write display data
            byte[] fe_name = display.name.LocaleID.Encode(Encoding.Unicode);
            byte[] fe_desc = display.desc.LocaleID.Encode(Encoding.Unicode);
            byte[] fe_loc = display.loc.LocaleID.Encode(Encoding.Unicode);

            bw.Write(fe_name.Length);
            bw.Write(fe_desc.Length);
            bw.Write(fe_loc.Length);

            bw.Write(fe_name);
            bw.Write(fe_desc);
            bw.Write(fe_loc);

            bw.Write((byte)display.theatre);
            bw.Write(display.playercount);

            bw.Write((byte)display.types.Count);
            display.types.ForEach(x => bw.Write((byte)x));

            bw.Write(display.turntime);

            bw.Write(display.startdate.Year);
            bw.Write(display.startdate.Month);
            bw.Write(display.startdate.Day);

            bw.Write(display.enddate.Year);
            bw.Write(display.enddate.Month);
            bw.Write(display.enddate.Day);

            byte[] startSide = display.start.Encode(Encoding.Unicode);
            bw.Write(startSide.Length);
            bw.Write(startSide);

            // Write weather data
            if (weather.winterStart != weather.winterEnd) {
                bw.Write(weather.winterStart.Year);
                bw.Write(weather.winterStart.Month);
                bw.Write(weather.winterStart.Day);
                bw.Write(weather.winterEnd.Year);
                bw.Write(weather.winterEnd.Month);
                bw.Write(weather.winterEnd.Day);
            } else {
                bw.Write(-1);
            }

            // Writer atmosphere counts
            bw.Write(weather.summerAtmospheres.Count);
            bw.Write(weather.winterAtmospheres.Count);

            void WriteAtmosphere(HashSet<string> atmos) {
                foreach (string summerAtmosphere in atmos) {
                    byte[] bytes = summerAtmosphere.Encode(Encoding.Unicode);
                    bw.Write(bytes.Length);
                    bw.Write(bytes);
                }
            }

            // Write atmospheres
            WriteAtmosphere(weather.summerAtmospheres);
            WriteAtmosphere(weather.winterAtmospheres);

            // Write each army
            armies.ForEach(x => {
                
                // Write army
                byte[] arm = x.army.Encode();
                bw.Write(arm.Length);
                bw.Write(arm);

                // Write numbers
                bw.Write(x.min);
                bw.Write(x.max);

                // Write locs
                byte[] name = x.name.LocaleID.Encode(Encoding.Unicode);
                byte[] desc = x.desc.LocaleID.Encode(Encoding.Unicode);

                // Write lengths
                bw.Write(name.Length);
                bw.Write(desc.Length);

                // Write actual values
                bw.Write(name);
                bw.Write(desc);

                // Save army file (if any)
                if (x.armyComposition is not null) {
                    byte[] armyFille = LuaBinary.SaveAsBinary(x.armyComposition, Encoding.Unicode);
                    bw.Write(armyFille.Length);
                    bw.Write(armyFille);
                } else {
                    Console.WriteLine("Warning: Army entry '{0}' Has no army file defined!", x.army);
                    bw.Write(0);
                }

                // Method for recursively write nested goals
                void WriteGoals(CampaignArmyGoal[] goals) {

                    // Write script function names if any.
                    void WriteIfFlagged(byte flag, byte mask, string text) {
                        if ((flag & mask) != 0) {
                            byte[] bytes = Encoding.Unicode.GetBytes(text);
                            bw.Write(bytes.Length);
                            bw.Write(bytes);
                        }
                    }

                    // Write goals
                    byte goalCount = (byte)(goals?.Length ?? 0);
                    bw.Write(goalCount);
                    for (byte i = 0; i < goalCount; i++) {

                        // Get unicode text ID
                        byte[] goalTitle = goals[i].name.LocaleID.Encode(Encoding.Unicode);
                        byte[] goalDesc = goals[i].desc.LocaleID.Encode(Encoding.Unicode);

                        // Write visual data
                        bw.Write(goalTitle.Length);
                        bw.Write(goalDesc.Length);
                        bw.Write(goals[i].priority);
                        bw.Write(goals[i].hidden);

                        // Write bytes
                        bw.Write(goalTitle);
                        bw.Write(goalDesc);

                        // Write script flag
                        byte hasFail = (byte)(string.IsNullOrEmpty(goals[i].onfail) ? 0x0 : 0x1);
                        byte hasDone = (byte)(string.IsNullOrEmpty(goals[i].ondone) ? 0x0 : 0x1);
                        byte hasUI = (byte)(string.IsNullOrEmpty(goals[i].onui) ? 0x0 : 0x1);
                        byte hasTrigger = (byte)(string.IsNullOrEmpty(goals[i].ontrigger) ? 0x0 : 0x1);
                        byte scriptFlag = (byte)((hasFail << 0) | (hasDone << 1) | (hasUI << 2) | (hasTrigger << 3) | (goals[i].goalType << 4));
                        bw.Write(scriptFlag);

                        // Write names if flagged
                        WriteIfFlagged(scriptFlag, 0b_0001, goals[i].onfail);
                        WriteIfFlagged(scriptFlag, 0b_0010, goals[i].ondone);
                        WriteIfFlagged(scriptFlag, 0b_0100, goals[i].onui);
                        WriteIfFlagged(scriptFlag, 0b_1000, goals[i].ontrigger);

                        // Call recursively to write subgoals.
                        WriteGoals(goals[i].subGoals);

                    }

                }

                // Write goals
                WriteGoals(x.goals);

            });

            // Write map definition table
            byte[] binaryData = LuaBinary.SaveAsBinary(mapdef, Encoding.Unicode);
            bw.Write(binaryData.Length);
            bw.Write(binaryData);

            // Write all resources
            foreach (var res in resources) {
                bw.Write(res.Identifier.Encode(Encoding.Unicode).Then(x => {
                    bw.Write(x.Length); return x;
                }));
                bw.Write((byte)res.Rt);
                bw.Write(res.Content.Length);
                bw.Write(res.Content);
            }

            // Finalize
            bw.Close();

        }

        private static bool ParseResource(LuaTable lTable, string relativePath, out CampaignResource resource) {
            string file = lTable["file"].Str();
            string name = Path.GetFileNameWithoutExtension(file);
            if (lTable["type"] is LuaUserObject userObject && userObject.Object is ResourceType rt) {
                if (rt == ResourceType.Locale) {

                    LocalizedFile fl = new LocalizedFile(name);
                    if (fl.LoadFromString(File.ReadAllText(Path.Combine(relativePath, file)))) {
                        resource = new CampaignResource() {
                            Rt = ResourceType.Locale,
                            Identifier = name,
                            Content = fl.AsBinary()
                        };
                        return true;
                    }

                } else if (rt == ResourceType.MapImage) {

                    resource = new CampaignResource() {
                        Rt = ResourceType.MapImage,
                        Identifier = name,
                        Content = File.ReadAllBytes(Path.Combine(relativePath, file))
                    };

                    return true;

                } else if (rt == ResourceType.CampaignScript) {

                    // Load
                    LuaState campaignState = new LuaState();
                    string src = Path.Combine(relativePath, file);

                    // Run syntax check
                    if (LuaVM.LoadFile(campaignState, src) is not LuaNil) {
                       
                        // Save raw
                        resource = new CampaignResource() {
                            Rt = rt,
                            Identifier = name,
                            Content = File.ReadAllBytes(src)
                        };

                        // Return true
                        return true;

                    } else {
                        Console.WriteLine(campaignState.GetError());
                    }

                } else if (rt == ResourceType.MissionScript) {



                } else if (rt == ResourceType.GfxMap) {

                    LuaState gfxState = new LuaState();
                    if (LuaVM.DoFile(gfxState, Path.Combine(relativePath, file)) is LuaNil) {

                        // Get GFX table
                        LuaTable gfxTable = gfxState._G["gfx"] as LuaTable;

                        // Load atlas
                        GfxMap atlas = GfxMap.FromLua(gfxTable, Path.Combine(relativePath, name));

                        // Create resource
                        resource = new CampaignResource() {
                            Rt = ResourceType.GfxMap,
                            Identifier = name,
                            Content = atlas.AsBinary()
                        };

                        // Return true
                        return true;

                    } else {
                        Console.WriteLine(gfxState.GetError());
                    }

                }
            }
            resource = default;
            return false;
        }

        private static void ParseArmies(LuaTable lTable, string dir, CampaignArmies armies) {
            lTable.Pairs((k, v) => {
                if (v is LuaTable table) {
                    if (Faction.FromName(k.Str()) is Faction) { // Verify it's a faction
                        CampaignArmy army = new CampaignArmy() {
                            army = k.Str(),
                            name = Loc(table["fe_army_name"]),
                            desc = Loc(table["fe_army_desc"]),
                            max = (int)(table["max_players"] as LuaNumber),
                            min = (int)(table["min_players"] as LuaNumber)
                        };
                        if (table["army_file"] is LuaString armyfile) {
                            string readfrom = Path.Combine(dir, armyfile.Str());
                            var state = new LuaState();
                            if (LuaVM.DoFile(state, readfrom) is LuaNil) {
                                if (state._G["army"] is LuaTable armyTable) {
                                    army.armyComposition = armyTable;
                                } else {
                                    Console.WriteLine($"Invalid army'{k.Str()}' (Missing global lua table 'army').");
                                    return;
                                }
                            } else {
                                Console.WriteLine(state.GetError());
                            }
                            armies.Add(army);
                        } else {
                            Console.WriteLine($"Invalid army'{k.Str()}' (Missing army data).");
                        }
                        static CampaignArmyGoal[] ReadGoals(LuaTable goals) {
                            CampaignArmyGoal[] result = new CampaignArmyGoal[goals.Size];
                            for (int i = 0; i < result.Length; i++) {
                                var kv = goals.KeyValueByRawIndex(i);
                                var vt = kv.Value as LuaTable;
                                result[i] = new CampaignArmyGoal() {
                                    name = new LocaleKey(kv.Key.Str()),
                                    desc = new LocaleKey($"{kv.Key.Str()}_desc"),
                                    priority = (vt.GetOrDefault("fe_priority", 0) as LuaNumber)?.ToInt() ?? 0,
                                    hidden = (vt.GetOrDefault("hidden", false) as LuaBool)?.IsTrue ?? false,
                                    goalType = (byte)((vt.GetOrDefault("type", 0) as LuaNumber)?.ToInt() ?? 0),
                                    ondone = vt.GetOrDefault("script_isdone", LuaString.Empty).Str(),
                                    onfail = vt.GetOrDefault("script_isfail", LuaString.Empty).Str(),
                                    onui = vt.GetOrDefault("script_ui", LuaString.Empty).Str(),
                                    ontrigger = vt.GetOrDefault("script_trigger", LuaString.Empty).Str(),
                                };
                                if (vt.GetOrDefault("subgoals", Nil) is LuaTable sub) {
                                    result[i].subGoals = ReadGoals(sub);
                                }
                            }
                            return result;
                        }
                        if (table.GetOrDefault("goals", Nil) is LuaTable goals) {
                            army.goals = ReadGoals(goals);
                        } else {
                            army.goals = Array.Empty<CampaignArmyGoal>();
                            Console.WriteLine($"Warning: Campaign for army {k.Str()} has no goals.");
                        }
                    } else {
                        Console.WriteLine($"Failed to read army data for entry '{k.Str()}'.");
                    }
                }
            });
        }

        private static bool ParseDisplay(LuaTable lTable, out CampaignDisplay display) {
            display = new CampaignDisplay {
                name = Loc(lTable["fe_name"]),
                desc = Loc(lTable["fe_desc"]),
                loc = Loc(lTable["fe_location"]),
                playercount = (int)(lTable["max_players"] as LuaNumber),
                start = lTable["start"].Str(),
                startdate = GetDate(lTable["start_date"] as LuaTable),
                enddate = GetDate(lTable["end_date"] as LuaTable),
                turntime = GetTurnTime(lTable["turn_time"] as LuaTable),
                theatre = GetTheatre(lTable["theatre"] as LuaString),
                types = GetTypes(lTable["modes"] as LuaTable)
            };
            return true;
        }

        private static bool ParseWeather(LuaTable table, out CampaignWeather weather) {
            weather = new CampaignWeather();
            if (table["winter"] is LuaTable winterTable) {
                weather.winterStart = GetDate(winterTable["start_date"] as LuaTable);
                weather.winterEnd = GetDate(winterTable["end_date"] as LuaTable);
            } else {                
                weather.winterStart = weather.winterEnd = new CampaignDate(2021, 1, 1);
            }
            if (table["allowed_atmospheres"] is LuaTable atmosTable) {
                var w = new HashSet<string>();
                var s = new HashSet<string>();
                if (atmosTable["s"] is LuaTable summer) {
                    summer.ToArray().ForEach(x => s.Add(x.Str()));
                }
                if (atmosTable["w"] is LuaTable winter) {
                    winter.ToArray().ForEach(x => w.Add(x.Str()));
                }
                weather.summerAtmospheres = s;
                weather.winterAtmospheres = w;
            } else {
                weather.summerAtmospheres = weather.winterAtmospheres = new HashSet<string>();
            }
            return true;
        }

        private static LocaleKey Loc(LuaValue lv) => new LocaleKey(lv.Str());

        private static CampaignDate GetDate(LuaTable t) 
            => new CampaignDate((int)(t["year"] as LuaNumber), (int)(t["month"] as LuaNumber), (int)(t["day"] as LuaNumber));

        private static int GetTurnTime(LuaTable table) {
            int total = 0;
            if (table["months"] is LuaNumber months) {
                total += (int)(months * (7 * 4 * 24));
            }
            if (table["days"] is LuaNumber days) {
                total += (int)days * 24;
            }
            if (table["hours"] is LuaNumber hours) {
                total += (int)(hours * 24);
            }
            return total;
        }

        private static CampaignTheatre GetTheatre(LuaString ls) => ls.Str().ToLower() switch {
            "east" => CampaignTheatre.East,
            "west" => CampaignTheatre.West,
            "east-west" => CampaignTheatre.EastWest,
            _ => CampaignTheatre.Undefined
        };

        private static HashSet<CampaignMode> GetTypes(LuaTable table) {
            HashSet<CampaignMode> ts = new HashSet<CampaignMode>();
            table.Pairs((k, v) => {
                ts.Add(v.Str().ToLower() switch {
                    "competitive" => CampaignMode.Competitive,
                    "cooperative" => CampaignMode.Cooperative,
                    "singleplayer" => CampaignMode.Singleplayer,
                    _ => CampaignMode.Undefined,
                });
            });
            return ts;
        }

    }

}
