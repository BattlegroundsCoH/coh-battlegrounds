{
    templates = {
        ["inf_div@rifle"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "grenadier_squad_bg", rank_range = { 0, 3 }, weight = 0.6 },
                { sbp = "ostruppen_squad_bg", rank_range = { 0, 1 }, weight = 0.3 },
                { sbp = "mg42_heavy_machine_gun_squad_bg", rank_range = { 0, 3 }, count = 3 },
                { sbp = "mortar_team_81mm_bg", rank_range = { 0, 3 }, count = 3 },
            }
        },
        ["inf_div@artillery"] = {
            size = 14, -- In total amount of units
            companies = 1, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "howitzer_105mm_le_fh18_artillery_bg", rank_range = { 0, 3 }, weight = 0.6 },
                { sbp = "assault_officer_squad_bg", rank_range = { 0, 1 }, count = 1 },
            }
        },
        ["inf_div@at"] = {
            size = 14, -- In total amount of units
            companies = 1, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "ost_tankhunter_squad_bg", rank_range = { 0, 3 }, weight = 0.6 },
                { sbp = "pak40_75mm_at_gun_squad_bg", rank_range = { 0, 3 }, weight = 0.8 },
                { sbp = "ostruppen_squad_bg", rank_range = { 0, 1 }, weight = 0.1 },
            }
        },
        ["inf_div@pioneer"] = {
            size = 10, -- In total amount of units
            companies = 1, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "pioneer_squad_bg", rank_range = { 0, 3 }, weight = 1.0 },
                { sbp = "assault_officer_squad_bg", rank_range = { 0, 1 }, count = 1 },
            }
        },
        ["jag_div@rifle"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "grenadier_squad_bg", rank_range = { 2, 5 }, weight = 0.4 },
                { sbp = "jaeger_squad_bg", rank_range = { 0, 2 }, weight = 0.6 },
            }
        },
        ["panz_div@rifle"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "panzer_grenadier_squad_bg", rank_range = { 2, 5 }, weight = 0.6 },
                { sbp = "ostruppen_squad_bg", rank_range = { 0, 1 }, weight = 0.2 },
                { sbp = "sdkfz_221_bg", rank_range = { 2, 5 }, weight = 0.1 },
            }
        },
        ["panz_div@td"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "ost_tankhunter_squad_bg", rank_range = { 2, 5 }, weight = 0.6 },
                { sbp = "pak40_75mm_at_gun_squad_bg", rank_range = { 0, 3 }, weight = 0.8 },
                { sbp = "stug_iii_squad_bg", rank_range = { 2, 5 }, weight = 0.2 },
            }
        },
        ["panz_div@pioneer"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "pioneer_squad_bg", rank_range = { 2, 5 }, weight = 0.8 },
                { sbp = "opel_blitz_supply_squad_bg", rank_range = { 2, 5 }, weight = 0.2 },
            }
        },
        ["panz_div@panz"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "panzer_iv_stubby_squad_bg", rank_range = { 2, 5 }, weight = 0.2 },
                { sbp = "panzer_iv_squad_bg", rank_range = { 2, 5 }, weight = 0.1 },
                { sbp = "panzer_iv_command_squad_bg", rank_range = { 2, 5 }, count = 2 },
                { sbp = "stug_iii_e_squad_bg", rank_range = { 2, 5 }, weight = 0.2 },
            }
        },
        ["mot_div@rifle"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "grenadier_squad_bg", rank_range = { 2, 5 }, weight = 0.4, transport_sbp = "opel_blitz_transport_squad_bg" },
                { sbp = "ostruppen_squad_bg", rank_range = { 2, 5 }, weight = 0.4, transport_sbp = "opel_blitz_transport_squad_bg" },
                { sbp = "panzer_grenadier_squad_bg", rank_range = { 2, 5 }, weight = 0.4, transport_sbp = "opel_blitz_transport_squad_bg" },
            }
        },
        ["mot_div@artillery"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "howitzer_105mm_le_fh18_artillery_bg", rank_range = { 2, 5 }, weight = 1.0, transport_sbp = "opel_blitz_transport_squad_bg" },
            }
        },
        ["mot_div@at"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "ost_tankhunter_squad_bg", rank_range = { 2, 5 }, weight = 0.4, transport_sbp = "opel_blitz_transport_squad_bg" },
                { sbp = "pak40_75mm_at_gun_squad_bg", rank_range = { 2, 5 }, weight = 0.4, transport_sbp = "opel_blitz_transport_squad_bg"  },
                { sbp = "pak43_88mm_at_gun_squad_bg", rank_range = { 2, 5 }, weight = 0.4, transport_sbp = "opel_blitz_transport_squad_bg"  },
            }
        },
        ["mot_div@pioneer"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "pioneer_squad_bg", rank_range = { 2, 5 }, weight = 0.8, transport_sbp = "opel_blitz_transport_squad_bg" },
                { sbp = "opel_blitz_supply_squad_bg", rank_range = { 2, 5 }, weight = 0.4, },
            }
        },
    },
    army = {
        name = "ger_6th",
        divisions = {
            ["ger_71st"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                }
            },
            ["ger_79th"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                }
            },
            ["ger_295th"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                }
            },
            ["ger_305th"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                }
            },
            ["ger_389th"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                },
                deploy = "northern_german_entrance"
            },
            ["ger_100th_jager"] = {
                tmpl = "jag_div",
                regiments = {
                    ["rifle"] = {
                        "ger_100th_jager_r1", -- 54th Jäger Regiment
                        "ger_100th_jager_r1", -- 227th Jäger Regiment
                    },
                    ["artillery"] = {
                        { name = "ger_100th_jager_s1", tmpl = "inf_div@artillery" } -- 83rd Artillery Regiment
                    },
                    ["at"] = {
                        { name = "ger_100th_jager_s2", tmpl = "inf_div@at" }, -- 100th Tank Destroyer Battalion
                    },
                    ["pioneer"] = {
                        { name = "ger_100th_jager_s3", tmpl = "inf_div@pioneer" } -- 100th Engineer Battalion
                    },
                },
                deploy = "central_west_german_entrance"
            },
            ["ger_94th"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                }
            },
            ["ger_76th"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                }
            },
            ["ger_113th"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                }
            },
            ["ger_44th"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                }
            },
            ["ger_376th"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                }
            },
            ["ger_384th"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                }
            },
            ["ger_297th"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                }
            },
            ["ger_371st"] = {
                tmpl = "inf_div",
                regiments = {
                    ["rifle"] = {
                        "ger_71st_r1", -- 191
                        "ger_71st_r2", -- 194
                        "ger_71st_r3", -- 211
                    },
                    ["artillery"] = {
                        "ger_71st_s1", -- Batalion
                    },
                    ["at"] = {
                        "ger_71st_s2", -- Batalion
                    },
                    ["pioneer"] = {
                        "ger_71st_s3", -- Batalion
                    },
                }
            },
            ["ger_60th"] = {
                tmpl = "mot_div",
                max_move = 2, -- Maximum move distance
                regiments = {
                    ["rifle"] = {
                        "ger_60th_r1", -- 92th Infantry Regiment
                        "ger_60th_r1", -- 120th Infantry Regiment
                    },
                    ["artillery"] = {
                        "ger_60th_s1", -- 160th Artillery Regiment
                    },
                    ["at"] = {
                        "ger_60th_s2", -- 160th Tank Destroyer Battalion
                    },
                    ["pioneer"] = {
                        "ger_60th_s3", -- 160th Engineer Battalion
                    },
                },
                deploy = "central_west_german_entrance"
            },
            ["ger_14th_panz"] = {
                tmpl = "panz_div",
                max_move = 2, -- Maximum move distance
                regiments = {
                    ["rifle"] = {
                        "ger_14th_panz_r1", -- 103rd Panzergrenadiere
                        "ger_14th_panz_r2", -- 108th Panzergrenadiere
                    },
                    ["artillery"] = {
                        {
                            name = "ger_14th_panz_s1", tmpl = "inf_div@artillery",
                            custom_units = {
                                { sbp = "panzerwerfer_squad_bg", rank_range = { 0, 1 }, weight = 0.2 },
                            },
                        }, -- 4th Panzer Artillery
                    },
                    ["td"] = {
                        "ger_14th_panz_td", -- 4th Tank Destroyer Battalion
                    },
                    ["panz"] = {
                        "ger_14th_panz_pz", -- 36th Panzer Regiment
                    },
                    ["pioneer"] = {
                        "ger_14th_panz_s2", -- 13th Panzer Engineer Battalion
                    },
                },
                deploy = "south_german_entrance"
            },
            ["ger_24th_panz"] = {
                tmpl = "panz_div",
                max_move = 2, -- Maximum move distance
                regiments = {
                    ["rifle"] = {
                        "ger_24th_panz_r1", -- 21st Rifle Regiment
                        "ger_24th_panz_r2", -- 26th Rilfe Regiment
                    },
                    ["artillery"] = {
                        {
                            name = "ger_24th_panz_s1", tmpl = "inf_div@artillery",
                            custom_units = {
                                { sbp = "panzerwerfer_squad_bg", rank_range = { 0, 1 }, weight = 0.2 },
                            },
                        }, -- 89th Artillery Regiment
                    },
                    ["td"] = {
                        "ger_24th_panz_td", -- 40th Tank Destroyer Battalion (40. Panzerjäger Abteilung)
                    },
                    ["panz"] = {
                        "ger_24th_panz_pz", -- 24th Panzer Regiment
                    },
                    ["pioneer"] = {
                        "ger_24th_panz_s2", -- 40th Engineer Battalion
                    },
                },
                deploy = "northern_german_entrance"
            },
        },
    }
}
