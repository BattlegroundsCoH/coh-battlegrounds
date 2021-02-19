using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Battlegrounds.Campaigns;
using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Lua;
using Battlegrounds.Util;

namespace coh2_battlegrounds_console {
    
    public static class CampaignCompiler {

        public const uint COMPILER_VERSION = 10;

        enum ResourceType : byte {
            MapImage,
            Locale,
            Script,
        }

        private struct CampaignResource {
            public string Identifier;
            public ResourceType Rt;
            public byte[] Content;
        }

        private struct CampaignDisplay {
            public LocaleKey name;
            public LocaleKey desc;
            public LocaleKey loc;
            public CampaignTheatre theatre;
            public HashSet<CampaignType> types;
            public CampaignDate startdate;
            public CampaignDate enddate;
            public string start;
            public int playercount;
            public int turntime;
        }

        private class CampaignArmy {
            public string army;
            public int min;
            public int max;
            public LocaleKey name;
            public LocaleKey desc;
            public LuaTable armyComposition;
        }

        public static void Compile(string dir) {

            // Create lua state
            LuaState settingsState = new LuaState();
            settingsState._G["MAP"] = LuaValue.ToLuaValue(ResourceType.MapImage);
            settingsState._G["LOCALE"] = LuaValue.ToLuaValue(ResourceType.Locale);
            settingsState._G["SCRIPT"] = LuaValue.ToLuaValue(ResourceType.Script);

            // Verify file exists
            string settingsFile = Path.Combine(dir, "campaign.lua");
            if (!File.Exists(settingsFile)) {
                Console.WriteLine("Invalid campaign folder.");
                return;
            }

            // List of resources loaded
            List<CampaignResource> campaignResources = new List<CampaignResource>();
            List<CampaignArmy> campaignArmies = new List<CampaignArmy>();
            CampaignDisplay campaignDisplay = default;

            // Load lua file
            if (settingsState.DoFile(settingsFile) is LuaTable manifest) {
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
                            LuaState armyReader = new LuaState();
                            if (v is LuaTable t) {
                                ParseArmies(t, dir, armyReader, campaignArmies);
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
                        default:
                            Console.WriteLine($"Undefined entry '{k.Str()}'");
                            break;
                    }
                });
            } else {
                Console.WriteLine("Failed read campaign.lua");
                return;
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

            Compile(dir, id, campaignDisplay, campaignArmies, campaignResources);

        }

        private static void Compile(string relative, string iddef, CampaignDisplay display, List<CampaignArmy> armies, List<CampaignResource> resources) {

            // Output path
            string output = Path.Combine(relative, Path.GetFileNameWithoutExtension(relative) + ".dat");

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
            bw.Write(display.turntime);
            bw.Write(display.playercount);

            bw.Write((byte)display.types.Count);
            display.types.ForEach(x => bw.Write((byte)x));

            bw.Write(display.startdate.Year);
            bw.Write(display.startdate.Month);
            bw.Write(display.startdate.Day);

            bw.Write(display.startdate.Year);
            bw.Write(display.startdate.Month);
            bw.Write(display.startdate.Day);

            byte[] startSide = display.start.Encode(Encoding.Unicode);
            bw.Write(startSide.Length);
            bw.Write(startSide);

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
                    bw.Write(0);
                }


            });

            // Write all resources
            foreach (var res in resources) {
                bw.Write(res.Identifier);
                bw.Write((byte)res.Rt);
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

                }
            }
            resource = default;
            return false;
        }

        private static void ParseArmies(LuaTable lTable, string dir, LuaState armyReadState, List<CampaignArmy> armies) {
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
                            if (armyReadState.DoFile(readfrom) is LuaTable armyTable) {
                                army.armyComposition = armyTable;
                            }
                            armies.Add(army);
                        } else {
                            Console.WriteLine($"Invalid army'{k.Str()}' (Missing army data).");
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

        private static HashSet<CampaignType> GetTypes(LuaTable table) {
            HashSet<CampaignType> ts = new HashSet<CampaignType>();
            table.Pairs((k, v) => {
                ts.Add(v.Str().ToLower() switch {
                    "competitive" => CampaignType.CompetitiveOnly,
                    "cooperative" => CampaignType.CooperativeOnly,
                    "singleplayer" => CampaignType.SingleplayerOnly,
                    _ => CampaignType.Undefined,
                });
            });
            return ts;
        }

    }

}
