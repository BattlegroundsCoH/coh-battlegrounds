{
    templates = {
        ["guards_rifle@rifle"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "frontoviki_squad_bg", rank_range = { 2, 5 }, weight = 0.6 },
                { sbp = "conscript_squad_bg", rank_range = { 0, 1 }, weight = 0.2 },
            }
        },
        ["guards_rifle@sapper"] = {
            size = 30, -- In total amount of units
            companies = 2, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "frontoviki_squad_bg", rank_range = { 2, 5 }, weight = 0.2 },
                { sbp = "soviet_sapper_squad_bg", rank_range = { 1, 5 }, weight = 0.5 }
            }
        },
        ["guards_rifle@artillery"] = {
            size = 30, -- In total amount of units
            companies = 2, -- (size / companies = actual company, remaining goes into a "reserve")
            is_artillery = true,
            units = {
                { sbp = "frontoviki_squad_bg", rank_range = { 2, 5 }, weight = 0.2 },
                { sbp = "soviet_sapper_squad_bg", rank_range = { 1, 5 }, weight = 0.5 }
            }
        },
        ["guards_rifle@at"] = {
            size = 30, -- In total amount of units
            companies = 2, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "frontoviki_squad_bg", rank_range = { 2, 5 }, weight = 0.2 },
                { sbp = "soviet_sapper_squad_bg", rank_range = { 1, 5 }, weight = 0.5 }
            }
        },
        ["rifle@rifle"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "frontoviki_squad_bg", rank_range = { 2, 5 }, weight = 0.6 },
                { sbp = "conscript_squad_bg", rank_range = { 0, 1 }, weight = 0.2 },
            }
        },
        ["rifle@artillery"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "frontoviki_squad_bg", rank_range = { 2, 5 }, weight = 0.6 },
                { sbp = "conscript_squad_bg", rank_range = { 0, 1 }, weight = 0.2 },
            }
        },
        ["tank@medium"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "frontoviki_squad_bg", rank_range = { 2, 5 }, weight = 0.6 },
                { sbp = "conscript_squad_bg", rank_range = { 0, 1 }, weight = 0.2 },
            }
        },
        ["tank@motorized"] = {
            size = 100, -- In total amount of units
            companies = 4, -- (size / companies = actual company, remaining goes into a "reserve")
            units = {
                { sbp = "frontoviki_squad_bg", rank_range = { 2, 5 }, weight = 0.6 },
                { sbp = "conscript_squad_bg", rank_range = { 0, 1 }, weight = 0.2 },
            }
        },
    },
    armies = {
        ["sov_army1"] = {
            ["sov_33rdgrds"] = {
                tmpl = "guards_rifle",
                regiments = {
                    ["rifle"] = {
                        "sov_33rdgrds_r1",
                        "sov_33rdgrds_r2",
                        "sov_33rdgrds_r3",
                    },
                    ["artillery"] = {
                        "sov_33rdgrds_s1",
                    },
                    ["at"] = {
                        "sov_33rdgrds_s2",
                    },
                    ["sapper"] = {
                        "sov_33rdgrds_s3",
                    },
                }
            },
            ["sov_35thgrds"] = { -- Initial defenders
                tmpl = "guards_rifle",
                regiments = {
                    ["rifle"] = {
                        "sov_35thgrds_r1",
                        "sov_35thgrds_r2",
                        "sov_35thgrds_r3",
                    },
                    ["artillery"] = {
                        "sov_35thgrds_s1",
                    },
                    ["at"] = {
                        "sov_35thgrds_s2",
                    },
                    ["sapper"] = {
                        "sov_35thgrds_s3",
                    },
                },
                deploy = "timberyard"
            },
            ["sov_87th"] = {
                tmpl = "rifle",
                regiments = {
                    ["rifle"] = {
                        "sov_87th_r1",
                        "sov_87th_r2",
                        "sov_87th_r3",
                    },
                    ["artillery"] = {
                        "sov_87th_s1",
                    },
                }
            },
            ["sov_133rd"] = {
                tmpl = "rifle",
                regiments = {
                    ["rifle"] = {
                        "sov_sov_133rd_r1",
                        "sov_sov_133rd_r2",
                        "sov_sov_133rd_r3",
                    },
                    ["artillery"] = {
                        "sov_sov_133rd_s1",
                    },
                },
                deploy = "southern_advances"
            },
            ["sov_244th"] = {
                tmpl = "rifle",
                regiments = {
                    ["rifle"] = {
                        "sov_sov_133rd_r1",
                        "sov_sov_133rd_r2",
                        "sov_sov_133rd_r3",
                    },
                    ["artillery"] = {
                        "sov_sov_133rd_s1",
                    },
                },
                deploy = { -- If table, the regiments are split 1/n'th to fit the deployments
                    "central_approach",
                    "central_train_station"
                },
            },
            ["sov_271st_nkvd"] = {
                tmpl = "nkvd",
                regiments = {
                    ["rifle"] = {
                        "sov_sov_133rd_r1"
                    },
                },
                deploy = "southern_advances"
            },
            ["sov_23rd_tcorps"] = {
                tmpl = "tank",
                regiments = {
                    ["medium"] = {
                        "sov_23rd_tcorps_t",
                        "sov_23rd_tcorps_t",
                        "sov_23rd_tcorps_t",
                    },
                    ["motorized"] = {
                        "sov_23rd_tcorps_m",
                    },
                }
            },
        },
    }
}