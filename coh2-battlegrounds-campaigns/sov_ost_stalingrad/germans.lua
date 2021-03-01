{
    templates = {
        ["inf_div@rifle"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "grenadier_squad_bg", rank_range = { 0, 3 }, weight = 0.6 },
                { sbp = "ostruppen_squad_bg", rank_range = { 0, 1 }, weight = 0.3 },
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
                { sbp = "panzer_grenadier_squad_bg", rank_range = { 2, 5 }, weight = 0.4 },
                { sbp = "panzer_iv_stubby_squad_bg", rank_range = { 2, 5 }, weight = 0.2 },
                { sbp = "panzer_iv_squad_bg", rank_range = { 2, 5 }, weight = 0.1 },
                { sbp = "panzer_iv_command_squad_bg", rank_range = { 2, 5 }, count = 2 },
                { sbp = "sdkfz_221_bg", rank_range = { 2, 5 }, weight = 0.1 },
                { sbp = "stug_iii_e_squad_bg", rank_range = { 2, 5 }, weight = 0.2 },
                { sbp = "stug_iii_squad_bg", rank_range = { 2, 5 }, weight = 0.2 },
            }
        },
        ["mot_div@rifle"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "grenadier_squad_bg", rank_range = { 2, 5 }, weight = 0.4 }, -- TODO: Add transports
                { sbp = "ostruppen_squad_bg", rank_range = { 2, 5 }, weight = 0.4 }, -- TODO: Add transports
                { sbp = "panzer_grenadier_squad_bg", rank_range = { 2, 5 }, weight = 0.4 }, -- TODO: Add transports
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
                }
            },
            ["ger_100th_jager"] = {
                tmpl = "jag_div",
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
            ["ger_14th_panz"] = {
                tmpl = "panz_div",
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
            ["ger_24th_panz"] = {
                tmpl = "panz_div",
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
        },
    }
}
