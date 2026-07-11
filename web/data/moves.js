window.MOVEBOOK_DATA = {
  "platformModel": {
    "base": "arcade",
    "platforms": {
      "arcade": {
        "label": "АРКАДА",
        "description": "Аркадная Ultimate Mortal Kombat 3 через MAME",
        "defaultNotation": "arcade"
      },
      "sega": {
        "label": "SEGA",
        "description": "Стандартная Ultimate Mortal Kombat 3 для SEGA",
        "defaultNotation": "sega"
      }
    },
    "fighters": {
      "default": {
        "platforms": ["arcade", "sega"]
      },
      "overrides": {
        "human-smoke": {
          "platformNotes": {
            "sega": "Скрытый персонаж, доступный через Robot Smoke."
          }
        },
        "sheeva": {
          "platforms": ["arcade"],
          "platformStatus": {
            "sega": "unavailable"
          },
          "platformNotes": {
            "sega": "Нет в стандартной Ultimate Mortal Kombat 3 для SEGA."
          }
        },
        "rain": {
          "platformNotes": {
            "sega": "Присутствует в стандартной Ultimate Mortal Kombat 3 для SEGA."
          }
        },
        "noob-saibot": {
          "platformNotes": {
            "sega": "Присутствует в стандартной Ultimate Mortal Kombat 3 для SEGA."
          }
        },
        "motaro": {
          "platformNotes": {
            "sega": "Открывается отдельно."
          }
        },
        "shao-kahn": {
          "platformNotes": {
            "sega": "Открывается отдельно."
          }
        }
      }
    },
    "moves": {
      "default": {
        "platforms": ["arcade"],
        "platformStatus": {
          "sega": "unverified"
        },
        "platformNotes": {
          "sega": "Комбинация для SEGA ещё не проверена."
        }
      },
      "rules": [
        {
          "labelPattern": "^Animality\\b",
          "platforms": ["arcade"],
          "platformStatus": {
            "sega": "unavailable"
          },
          "platformNotes": {
            "sega": "Mercy и Animality отсутствуют в стандартной Ultimate Mortal Kombat 3 для SEGA."
          }
        }
      ]
    }
  },
  "seo": {
    "siteName": "MK3 MoveBook",
    "fighters": {
      "kabal": "Кабал",
      "human-smoke": "Хьюман Смоук",
      "kung-lao": "Кун Лао",
      "kano": "Кано",
      "reptile": "Рептилия",
      "ermac": "Ермак",
      "nightwolf": "Ночной Волк",
      "robot-smoke": "Робот Смоук",
      "sindel": "Синдел",
      "jax": "Джакс",
      "sonya": "Соня",
      "kitana": "Китана",
      "stryker": "Страйкер",
      "scorpion": "Скорпион",
      "unmasked-sub-zero": "Саб-Зиро без маски",
      "jade": "Джейд",
      "liu-kang": "Лю Кан",
      "sektor": "Сектор",
      "classic-sub-zero": "Классический Саб-Зиро",
      "cyrax": "Сайракс",
      "shang-tsung": "Шан Цзун",
      "mileena": "Милина",
      "sheeva": "Шива",
      "rain": "Рейн",
      "noob-saibot": "Нуб Сайбот",
      "motaro": "Мотаро",
      "shao-kahn": "Шао Кан"
    }
  },
  "versions": {
    "umk3uk": {
      "id": "umk3uk",
      "fighters": [
        {
          "id": "kabal",
          "name": "Kabal",
          "number": 1,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Dash","notation":"_4,_6,LK","pretty":"←,→,LK"},
                {"label":"Eye Spark (also in air)","notation":"_4,_4,HP","pretty":"←,←,HP"},
                {"label":"Ground Saw","notation":"_4,_4,_4,RUN","pretty":"←,←,←,RUN"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (sweep)","notation":"_2,_2,_4,_6,BL","pretty":"↓,↓,←,→,BL"},
                {"label":"Fatality 2              (close)","notation":"RUN,BL,BL,BL,HK","pretty":"RUN,BL,BL,BL,HK"},
                {"label":"Animality              (close)","notation":"hold HP _6,_6,_2,_6","pretty":"hold HP →,→,↓,→"},
                {"label":"Antifatality           (close)","notation":"hold HP _6,_6,_2,_6","pretty":"hold HP →,→,↓,→"},
                {"label":"Friendship            (sweep)","notation":"RUN,LK,RUN,RUN,_8","pretty":"RUN,LK,RUN,RUN,↑"},
                {"label":"Babality","notation":"RUN,RUN,LK","pretty":"RUN,RUN,LK"},
                {"label":"Stage Fatality","notation":"BL,BL,HK","pretty":"BL,BL,HK"},
                {"label":"Brutality","notation":"HP,BL,LK,LK,LK,HK,LP,LP,LP,HP,LP","pretty":"HP,BL,LK,LK,LK,HK,LP,LP,LP,HP,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(13%)","notation":"HP,HP,_2_+HP(juggle)","pretty":"HP,HP,↓+HP(juggle)"},
                {"label":"(18%)","notation":"[LK/HK],LK,_4_+HK","pretty":"[LK/HK],LK,←+HK"},
                {"label":"(17%)","notation":"HP,HP,_2_+LP,_2_+HP","pretty":"HP,HP,↓+LP,↓+HP"},
                {"label":"(19%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"},
                {"label":"(24%)","notation":"[LK/HK],LK,HK,_4_+HK","pretty":"[LK/HK],LK,HK,←+HK"},
                {"label":"(15%)","notation":"[LK/HK],LK,HP,HP,_2_+HP(juggle)","pretty":"[LK/HK],LK,HP,HP,↓+HP(juggle)"},
                {"label":"(17%)","notation":"[LK/HK],LK,HP,HP,_2_+LP,_2_+HP","pretty":"[LK/HK],LK,HP,HP,↓+LP,↓+HP"},
                {"label":"(18%)","notation":"[LK/HK],LK,HP,HP,HK,_4_+HK","pretty":"[LK/HK],LK,HP,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "human-smoke",
          "name": "Human Smoke",
          "number": 2,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Spear","notation":"_4,_4,LP","pretty":"←,←,LP"},
                {"label":"Teleport Punch","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Air Throw","notation":"BL in air","pretty":"BL in air"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1            (close)","notation":"RUN,BL,RUN,RUN,HK","pretty":"RUN,BL,RUN,RUN,HK"},
                {"label":"Babality","notation":"_2,_4,_4,_6,HP","pretty":"↓,←,←,→,HP"},
                {"label":"Stage Fatality","notation":"_6,_8,_8,LP","pretty":"→,↑,↑,LP"},
                {"label":"Brutality","notation":"HP,HP,BL,LK,HK,HP,HK,HP,HK,LP,LK","pretty":"HP,HP,BL,LK,HK,HP,HK,HP,HK,LP,LK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"[LK/HK],LP(juggle)","pretty":"[LK/HK],LP(juggle)"},
                {"label":"(18%)","notation":"HP,HP,_8_+LP","pretty":"HP,HP,↑+LP"},
                {"label":"(19%)","notation":"[LK/HK],_2_+LP,_2_+HP(juggle)","pretty":"[LK/HK],↓+LP,↓+HP(juggle)"},
                {"label":"(23%)","notation":"HK,HK,LK,_4_+HK","pretty":"HK,HK,LK,←+HK"},
                {"label":"(24%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "kung-lao",
          "name": "Kung Lao",
          "number": 3,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Hat Throw","notation":"_4,_6,LP","pretty":"←,→,LP"},
                {"label":"Teleport","notation":"_2,_8","pretty":"↓,↑"},
                {"label":"Torpedo Kick (in air)","notation":"_2_+HK in air","pretty":"↓+HK in air"},
                {"label":"Spin Attack (tap RUN to spin)","notation":"_6,_2,_6,RUN","pretty":"→,↓,→,RUN"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1","notation":"RUN,BL,RUN,BL,_2","pretty":"RUN,BL,RUN,BL,↓"},
                {"label":"Fatality 2               (sweep)","notation":"_6,_6,_4,_2,HP","pretty":"→,→,←,↓,HP"},
                {"label":"Animality            (close)","notation":"RUN,RUN,RUN,RUN,BL","pretty":"RUN,RUN,RUN,RUN,BL"},
                {"label":"Friendship                (sweep)","notation":"RUN,LP,RUN,LK","pretty":"RUN,LP,RUN,LK"},
                {"label":"Babality","notation":"_2,_6,_6,HP","pretty":"↓,→,→,HP"},
                {"label":"Stage Fatality","notation":"_2,_2,_6,_6,LK","pretty":"↓,↓,→,→,LK"},
                {"label":"Brutality","notation":"HP,LP,LK,HK,BL,HP,LP,LK,HK,BL,HP","pretty":"HP,LP,LK,HK,BL,HP,LP,LK,HK,BL,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,LK,_4_+HK","pretty":"HP,LK,←+HK"},
                {"label":"(19%)","notation":"[LK/HK],LK,HK","pretty":"[LK/HK],LK,HK"},
                {"label":"(34%)","notation":"HP,LP,HP,LP,LK,LK,HK","pretty":"HP,LP,HP,LP,LK,LK,HK"}
              ]
            }
          ]
        },
        {
          "id": "kano",
          "name": "Kano",
          "number": 4,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Knife Throw","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Knife Uppercut","notation":"_2,_6,HP","pretty":"↓,→,HP"},
                {"label":"Cannonball-straight","notation":"hold LK for 3 sec","pretty":"hold LK for 3 sec"},
                {"label":"Cannonball-upward","notation":"_6,_2,_6,HK","pretty":"→,↓,→,HK"},
                {"label":"Cannonball-random         (TRILOGY)","notation":"_6,_2,_6,LK","pretty":"→,↓,→,LK"},
                {"label":"Neck Choke","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Air Throw","notation":"BL in air","pretty":"BL in air"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1          (close)","notation":"hold LP _6,_2,_2,_6","pretty":"hold LP →,↓,↓,→"},
                {"label":"Fatality 2                  (sweep)","notation":"LP,BL,BL,HK","pretty":"LP,BL,BL,HK"},
                {"label":"Animality              (close)","notation":"hold HP BL,BL,BL","pretty":"hold HP BL,BL,BL"},
                {"label":"Friendship                (sweep)","notation":"LK,RUN,RUN,HK","pretty":"LK,RUN,RUN,HK"},
                {"label":"Babality","notation":"_6,_6,_2,_2,LK","pretty":"→,→,↓,↓,LK"},
                {"label":"Stage Fatality","notation":"_8,_8,_4,LK","pretty":"↑,↑,←,LK"},
                {"label":"Brutality","notation":"HP,LP,BL,LP,HP,BL,HK,LK,BL,HK,LK","pretty":"HP,LP,BL,LP,HP,BL,HK,LK,BL,HK,LK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"[LK/HK],LP(juggle)","pretty":"[LK/HK],LP(juggle)"},
                {"label":"(18%)","notation":"HP,HP,LP(juggle)","pretty":"HP,HP,LP(juggle)"},
                {"label":"(19%)","notation":"[LK/HK],_2_+LP,_2_+HP(juggle)","pretty":"[LK/HK],↓+LP,↓+HP(juggle)"},
                {"label":"(22%)","notation":"HP,HP,_2_+LP,_2_+HP(juggle)","pretty":"HP,HP,↓+LP,↓+HP(juggle)"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(26%)","notation":"HP,HP,HK,LK,_4_+HK","pretty":"HP,HP,HK,LK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "reptile",
          "name": "Reptile",
          "number": 5,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Acid Spit","notation":"_6,_6,HP","pretty":"→,→,HP"},
                {"label":"Forceball-slow","notation":"_4,_4,HP_+LP","pretty":"←,←,HP+LP"},
                {"label":"Forceball-fast","notation":"_6,_6,HP_+LP","pretty":"→,→,HP+LP"},
                {"label":"Slide","notation":"_4_+LP_+BL_+LK","pretty":"←+LP+BL+LK"},
                {"label":"Invisibility","notation":"_8,_2,HK","pretty":"↑,↓,HK"},
                {"label":"Super Elbow","notation":"_4,_6,LK","pretty":"←,→,LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                   (jump)","notation":"_4,_6,_2,BL","pretty":"←,→,↓,BL"},
                {"label":"Fatality 2               (sweep)","notation":"_6,_6,_8,_8,HK","pretty":"→,→,↑,↑,HK"},
                {"label":"Animality                (close)","notation":"_2,_2,_2,_8,HK","pretty":"↓,↓,↓,↑,HK"},
                {"label":"Friendship               (close)","notation":"_2,_6,_6,_4,HK","pretty":"↓,→,→,←,HK"},
                {"label":"Babality","notation":"_6,_6,_4,_2,LK","pretty":"→,→,←,↓,LK"},
                {"label":"Stage Fatality","notation":"BL,RUN,BL,BL","pretty":"BL,RUN,BL,BL"},
                {"label":"Brutality","notation":"HP,BL,HK,HK,BL,HP,LP,LK,LK,BL,LP","pretty":"HP,BL,HK,HK,BL,HP,LP,LK,LK,BL,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,_2_+LP(juggle)","pretty":"HP,HP,↓+LP(juggle)"},
                {"label":"(21%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(24%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "ermac",
          "name": "Ermac",
          "number": 6,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fire Teleport Punch","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Green Sphere","notation":"_2,_4,LP","pretty":"↓,←,LP"},
                {"label":"Soul Slam","notation":"_4,_2,_4,HK","pretty":"←,↓,←,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1            (sweep)","notation":"_2,_8,_2,_2,_2,BL","pretty":"↓,↑,↓,↓,↓,BL"},
                {"label":"Fatality 2            (close)","notation":"RUN,BL,RUN,RUN,HK","pretty":"RUN,BL,RUN,RUN,HK"},
                {"label":"Stage Fatality","notation":"RUN,RUN,RUN,RUN,LK","pretty":"RUN,RUN,RUN,RUN,LK"},
                {"label":"Babality","notation":"_2,_2,_4,_4,HP","pretty":"↓,↓,←,←,HP"},
                {"label":"Brutality","notation":"HP,HP,LP,BL,HK,LK,BL,HP,LP,LK,HK","pretty":"HP,HP,LP,BL,HK,LK,BL,HP,LP,LK,HK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"[LK/HK],LP(juggle)","pretty":"[LK/HK],LP(juggle)"},
                {"label":"(19%)","notation":"HP,HP,_4_+LP,_2,_6,LP(juggle)","pretty":"HP,HP,←+LP,↓,→,LP(juggle)"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(24%)","notation":"HP,HP,_4_+LP,HK,LK","pretty":"HP,HP,←+LP,HK,LK"}
              ]
            }
          ]
        },
        {
          "id": "nightwolf",
          "name": "Nightwolf",
          "number": 7,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Arrow","notation":"_2,_4,LP","pretty":"↓,←,LP"},
                {"label":"Hatchet Uppercut","notation":"_2,_6,HP","pretty":"↓,→,HP"},
                {"label":"Shadow Charge","notation":"_6,_6,LK","pretty":"→,→,LK"},
                {"label":"Reflect Shield","notation":"_4,_4,_4,HK","pretty":"←,←,←,HK"},
                {"label":"Red Shadow Charge (only TRILOGY profile)","notation":"_4,_4,_6,HK","pretty":"←,←,→,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_8,_8,_4,_6,BL","pretty":"↑,↑,←,→,BL"},
                {"label":"Fatality 2                  (sweep)","notation":"_4,_4,_2,HP","pretty":"←,←,↓,HP"},
                {"label":"Animality                   (close)","notation":"_6,_6,_2,_2","pretty":"→,→,↓,↓"},
                {"label":"Friendship               (sweep)","notation":"RUN,RUN,RUN,_2","pretty":"RUN,RUN,RUN,↓"},
                {"label":"Babality","notation":"_6,_4,_6,_4,LP","pretty":"→,←,→,←,LP"},
                {"label":"Stage Fatality","notation":"RUN,RUN,BL","pretty":"RUN,RUN,BL"},
                {"label":"Brutality","notation":"HP,HP,HK,LK,LK,BL,BL,LP,LP,HP,HK","pretty":"HP,HP,HK,LK,LK,BL,BL,LP,LP,HP,HK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(22%)","notation":"HP,HP,LP,HK","pretty":"HP,HP,LP,HK"},
                {"label":"(23%)","notation":"HP,HP,LP,Hatchet Uppercut(juggle)","pretty":"HP,HP,LP,Hatchet Uppercut(juggle)"},
                {"label":"(27%)","notation":"[LK/HK],HP,HP,LP,HK","pretty":"[LK/HK],HP,HP,LP,HK"},
                {"label":"(28%)","notation":"[LK/HK],HP,HP,LP,Hatchet Uppercut(juggle)","pretty":"[LK/HK],HP,HP,LP,Hatchet Uppercut(juggle)"}
              ]
            }
          ]
        },
        {
          "id": "robot-smoke",
          "name": "Robot Smoke",
          "number": 8,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Harpoon","notation":"_4,_4,LP","pretty":"←,←,LP"},
                {"label":"Teleport Punch (also in  air)","notation":"_6,_6,LK","pretty":"→,→,LK"},
                {"label":"Invisibility","notation":"_8,_8,RUN","pretty":"↑,↑,RUN"},
                {"label":"Air Throw","notation":"BL in air","pretty":"BL in air"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                    (far)","notation":"_8,_8,_6,_2","pretty":"↑,↑,→,↓"},
                {"label":"Fatality 2     (sweep)","notation":"hold BL_+RUN _2,_2,_6,_8","pretty":"hold BL+RUN ↓,↓,→,↑"},
                {"label":"Animality                    (jump)","notation":"_2,_6,_6,BL","pretty":"↓,→,→,BL"},
                {"label":"Frienship                (sweep)","notation":"RUN,RUN,RUN,HK","pretty":"RUN,RUN,RUN,HK"},
                {"label":"Babality","notation":"_2,_2,_4,_4,HK","pretty":"↓,↓,←,←,HK"},
                {"label":"Stage Fatality","notation":"_6,_6,_2,LK","pretty":"→,→,↓,LK"},
                {"label":"Brutality","notation":"HP,LK,LK,HK,BL,BL,LP,LP,HP,BL,BL","pretty":"HP,LK,LK,HK,BL,BL,LP,LP,HP,BL,BL"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,LP","pretty":"HP,HP,LP"},
                {"label":"(18%)","notation":"HP,HP,HK","pretty":"HP,HP,HK"},
                {"label":"(19%)","notation":"[LK/HK],HK,LP","pretty":"[LK/HK],HK,LP"},
                {"label":"(26%)","notation":"HP,HP,LK,HK,LP","pretty":"HP,HP,LK,HK,LP"}
              ]
            }
          ]
        },
        {
          "id": "sindel",
          "name": "Sindel",
          "number": 9,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fireball-ground","notation":"_6,_6,LP","pretty":"→,→,LP"},
                {"label":"Fireball-air","notation":"_2,_6,LK in air","pretty":"↓,→,LK in air"},
                {"label":"Fly","notation":"_4,_4,_6,HK","pretty":"←,←,→,HK"},
                {"label":"Scream","notation":"_6,_6,_6,HP","pretty":"→,→,→,HP"},
                {"label":"Double Fireball           (TRILOGY)","notation":"_4,_4,_6,LP","pretty":"←,←,→,LP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1            (sweep)","notation":"RUN,RUN,BL,RUN,BL","pretty":"RUN,RUN,BL,RUN,BL"},
                {"label":"Fatality 2            (close)","notation":"RUN,BL,BL,RUN_+BL","pretty":"RUN,BL,BL,RUN+BL"},
                {"label":"Animality","notation":"_6,_6,_8,HP","pretty":"→,→,↑,HP"},
                {"label":"Friendship","notation":"RUN,RUN,RUN,RUN,RUN,_8","pretty":"RUN,RUN,RUN,RUN,RUN,↑"},
                {"label":"Babality","notation":"RUN,RUN,RUN,_8","pretty":"RUN,RUN,RUN,↑"},
                {"label":"Stage Fatality","notation":"_2,_2,_2,LP","pretty":"↓,↓,↓,LP"},
                {"label":"Brutality","notation":"HP,BL,LK,BL,LK,HK,BL,HK,LK,BL,LP","pretty":"HP,BL,LK,BL,LK,HK,BL,HK,LK,BL,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"HP,HP,_2_+HP(juggle)","pretty":"HP,HP,↓+HP(juggle)"},
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(25%)","notation":"HP,HP,LP,HK","pretty":"HP,HP,LP,HK"},
                {"label":"(27%)","notation":"[LK/HK],HP,HP,_2_+HP(juggle)","pretty":"[LK/HK],HP,HP,↓+HP(juggle)"},
                {"label":"(33%)","notation":"[LK/HK],HP,HP,LP,HK","pretty":"[LK/HK],HP,HP,LP,HK"}
              ]
            }
          ]
        },
        {
          "id": "jax",
          "name": "Jax",
          "number": 10,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Missile-single","notation":"_4,_6,HP","pretty":"←,→,HP"},
                {"label":"Missile-double","notation":"_6,_6,_4,_4,HP","pretty":"→,→,←,←,HP"},
                {"label":"Gotcha Grab","notation":"_6,_6_+LP, LPx3","pretty":"→,→+LP, LPx3"},
                {"label":"Backbreaker (in air)","notation":"BL in air","pretty":"BL in air"},
                {"label":"Quad Slam","notation":"throw, HPx4","pretty":"throw, HPx4"},
                {"label":"Ground Smash","notation":"hold LK for 3 sec","pretty":"hold LK for 3 sec"},
                {"label":"Dashing Pound","notation":"_6,_6,HK","pretty":"→,→,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1          (close)","notation":"hold BL _8,_2,_6,_8","pretty":"hold BL ↑,↓,→,↑"},
                {"label":"Fatality 2              (far)","notation":"RUN,BL,RUN,RUN,LK","pretty":"RUN,BL,RUN,RUN,LK"},
                {"label":"Animality           (close)","notation":"hold LP _6,_6,_2,_6","pretty":"hold LP →,→,↓,→"},
                {"label":"Friendship                (sweep)","notation":"LK,RUN,RUN,LK","pretty":"LK,RUN,RUN,LK"},
                {"label":"Babality","notation":"_2,_2,_2,LK","pretty":"↓,↓,↓,LK"},
                {"label":"Stage Fatality","notation":"_2,_6,_2,LP","pretty":"↓,→,↓,LP"},
                {"label":"Brutality","notation":"HP,HP,HP,BL,LP,HP,HP,HP,BL,LP,HP","pretty":"HP,HP,HP,BL,LP,HP,HP,HP,BL,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(24%)","notation":"HP,HP,BL,LP,_4_+HP","pretty":"HP,HP,BL,LP,←+HP"},
                {"label":"(33%)","notation":"[LK/HK],HK,_2_+HP,HP,BL,LP,_4_+HP","pretty":"[LK/HK],HK,↓+HP,HP,BL,LP,←+HP"}
              ]
            }
          ]
        },
        {
          "id": "sonya",
          "name": "Sonya",
          "number": 11,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Force Wave","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Leg Grab","notation":"_2_+LP_+BL","pretty":"↓+LP+BL"},
                {"label":"Square Wave","notation":"_6,_4,HP","pretty":"→,←,HP"},
                {"label":"Bicycle Kick","notation":"_4,_4,_2,HK","pretty":"←,←,↓,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1       (far)","notation":"hold BL_+RUN _8,_8,_4,_2","pretty":"hold BL+RUN ↑,↑,←,↓"},
                {"label":"Fatality 2","notation":"_4,_6,_2,_2,RUN","pretty":"←,→,↓,↓,RUN"},
                {"label":"Animality           (close)","notation":"hold LP _4,_6,_2,_6","pretty":"hold LP ←,→,↓,→"},
                {"label":"Friendship              (sweep)","notation":"_4,_6,_4,_2,RUN","pretty":"←,→,←,↓,RUN"},
                {"label":"Babality","notation":"_2,_2,_6,LK","pretty":"↓,↓,→,LK"},
                {"label":"Stage Fatality","notation":"_6,_6,_2,HP","pretty":"→,→,↓,HP"},
                {"label":"Brutality","notation":"HP,LK,BL,HP,LK,BL,HP,LP,BL,HK,LK","pretty":"HP,LK,BL,HP,LK,BL,HP,LP,BL,HK,LK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,_8_+LP(juggle)","pretty":"HP,HP,↑+LP(juggle)"},
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(22%)","notation":"HP,HP,LP,_4_+HP","pretty":"HP,HP,LP,←+HP"},
                {"label":"(27%)","notation":"[LK/HK],HP,HP,LP,_4_+HP","pretty":"[LK/HK],HP,HP,LP,←+HP"},
                {"label":"(27%)","notation":"[LK/HK],HK,HP,HP,_8_+LP(juggle)","pretty":"[LK/HK],HK,HP,HP,↑+LP(juggle)"},
                {"label":"(31%)","notation":"[LK/HK],HK,HP,HP,LP,_4_+HP","pretty":"[LK/HK],HK,HP,HP,LP,←+HP"}
              ]
            }
          ]
        },
        {
          "id": "kitana",
          "name": "Kitana",
          "number": 12,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fan Lift","notation":"_4,_4,_4,HP","pretty":"←,←,←,HP"},
                {"label":"Fan Throw (also in air)","notation":"_6,_6,HP_+LP","pretty":"→,→,HP+LP"},
                {"label":"Square Wave","notation":"_2,_4,HP","pretty":"↓,←,HP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1             (close)","notation":"RUN,RUN,BL,BL,LK","pretty":"RUN,RUN,BL,BL,LK"},
                {"label":"Fatality 2               (close)","notation":"_4,_2,_6,_6,HK","pretty":"←,↓,→,→,HK"},
                {"label":"Animality               (sweep)","notation":"_2,_2,_2,_2,RUN","pretty":"↓,↓,↓,↓,RUN"},
                {"label":"Friendship","notation":"_2,_4,_6,_6,LP","pretty":"↓,←,→,→,LP"},
                {"label":"Babality","notation":"_6,_6,_2,_6,HK","pretty":"→,→,↓,→,HK"},
                {"label":"Stage Fatality","notation":"_6,_2,_2,LK","pretty":"→,↓,↓,LK"},
                {"label":"Brutality:","notation":"HP,HP,BL,HK,BL,LK,BL,LP,BL,HP,BL","pretty":"HP,HP,BL,HK,BL,LK,BL,LP,BL,HP,BL"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(25%)","notation":"HP,HP,_4_+LP,_6_+HP","pretty":"HP,HP,←+LP,→+HP"}
              ]
            }
          ]
        },
        {
          "id": "stryker",
          "name": "Stryker",
          "number": 13,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Grenade-high","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Grenade-low","notation":"_2,_4,LP","pretty":"↓,←,LP"},
                {"label":"Gun Shot","notation":"_4,_6,HP","pretty":"←,→,HP"},
                {"label":"Baton Sweep","notation":"_6,_4,LP","pretty":"→,←,LP"},
                {"label":"Baton Toss","notation":"_6,_6,HK","pretty":"→,→,HK"},
                {"label":"Double Grenades-1         (TRILOGY)","notation":"_6,_2,_4,HP","pretty":"→,↓,←,HP"},
                {"label":"Double Grenades-2         (TRILOGY)","notation":"_6,_2,_4,LP","pretty":"→,↓,←,LP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_2,_6,_2,_6,BL","pretty":"↓,→,↓,→,BL"},
                {"label":"Fatality 2                    (far)","notation":"_6,_6,_6,LK","pretty":"→,→,→,LK"},
                {"label":"Animality                (sweep)","notation":"RUN,RUN,RUN,BL","pretty":"RUN,RUN,RUN,BL"},
                {"label":"Friendship","notation":"LP,RUN,RUN,LP","pretty":"LP,RUN,RUN,LP"},
                {"label":"Babality","notation":"_2,_6,_6,_4,HP","pretty":"↓,→,→,←,HP"},
                {"label":"Stage Fatality","notation":"_6,_8,_8,HK","pretty":"→,↑,↑,HK"},
                {"label":"Brutality:","notation":"HP,LP,HK,LK,HP,LP,LK,HK,HP,LK,LK","pretty":"HP,LP,HK,LK,HP,LP,LK,HK,HP,LK,LK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,LP(juggle)","pretty":"HP,HP,LP(juggle)"},
                {"label":"(19%)","notation":"[LK/HK],LK,_4_+HK","pretty":"[LK/HK],LK,←+HK"},
                {"label":"(23%)","notation":"[LK/HK],HP,HP,LP(juggle)","pretty":"[LK/HK],HP,HP,LP(juggle)"},
                {"label":"(23%)","notation":"[LK/HK],LK,_4_+LP,_4_+HK","pretty":"[LK/HK],LK,←+LP,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "scorpion",
          "name": "Scorpion",
          "number": 14,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Spear","notation":"_4,_4,LP","pretty":"←,←,LP"},
                {"label":"Teleport Punch","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Air Throw","notation":"BL in air","pretty":"BL in air"},
                {"label":"Forward Teleport             (TRILOGY)","notation":"_2,_6,HP","pretty":"↓,→,HP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                   (jump)","notation":"_2,_2,_8,HK","pretty":"↓,↓,↑,HK"},
                {"label":"Fatality 2              (close)","notation":"_6,_6,_2,_8,RUN","pretty":"→,→,↓,↑,RUN"},
                {"label":"Animality                   (close)","notation":"_6,_8,_8,HK","pretty":"→,↑,↑,HK"},
                {"label":"Friendship               (sweep)","notation":"_4,_6,_6,_4,LK","pretty":"←,→,→,←,LK"},
                {"label":"Babality","notation":"_2,_4,_4,_6,HP","pretty":"↓,←,←,→,HP"},
                {"label":"Stage Fatality","notation":"_6,_8,_8,LP","pretty":"→,↑,↑,LP"},
                {"label":"Brutality","notation":"HP,HP,BL,HK,HK,LK,HK,HP,HP,LP,HP","pretty":"HP,HP,BL,HK,HK,LK,HK,HP,HP,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,_8_+LP","pretty":"HP,HP,↑+LP"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,LK","pretty":"[LK/HK],HK,LK,LK"},
                {"label":"(24%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "unmasked-sub-zero",
          "name": "Unmasked Sub-Zero",
          "number": 15,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Freeze","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Ice Shower-over","notation":"_2,_6,HP","pretty":"↓,→,HP"},
                {"label":"Ice Shower-front","notation":"_2,_6,_4,HP","pretty":"↓,→,←,HP"},
                {"label":"Ice Shower-behind","notation":"_2,_4,_6,HP","pretty":"↓,←,→,HP"},
                {"label":"Ice Clone","notation":"_2,_4,LP","pretty":"↓,←,LP"},
                {"label":"Slide","notation":"_4_+LP_+BL_+LK","pretty":"←+LP+BL+LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1             (close)","notation":"BL,BL,RUN,BL,RUN","pretty":"BL,BL,RUN,BL,RUN"},
                {"label":"Fatality 2              (sweep)","notation":"_4,_4,_2,_4,RUN","pretty":"←,←,↓,←,RUN"},
                {"label":"Animality                      (close)","notation":"_6,_8,_8","pretty":"→,↑,↑"},
                {"label":"Friendship                (sweep)","notation":"LK,RUN,RUN,_8","pretty":"LK,RUN,RUN,↑"},
                {"label":"Babality","notation":"_2,_4,_4,HK","pretty":"↓,←,←,HK"},
                {"label":"Stage Fatality","notation":"_4,_2,_6,_6,HK","pretty":"←,↓,→,→,HK"},
                {"label":"Brutality","notation":"HP,LK,HK,LP,HP,HK,HK,HP,HP,LP,HP","pretty":"HP,LK,HK,LP,HP,HK,HK,HP,HP,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,_4_+HK","pretty":"HP,HP,←+HK"},
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(22%)","notation":"HP,HP,LP,_4_+HK","pretty":"HP,HP,LP,←+HK"},
                {"label":"(26%)","notation":"HP,HP,LK,HK,_4_+HK","pretty":"HP,HP,LK,HK,←+HK"},
                {"label":"(23%)","notation":"HP,HP,LP,LK,HK,_4_+HK","pretty":"HP,HP,LP,LK,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "jade",
          "name": "Jade",
          "number": 16,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Boomerang-straight","notation":"_4,_6,LP","pretty":"←,→,LP"},
                {"label":"Boomerang-returning","notation":"_4,_4,_6,LP","pretty":"←,←,→,LP"},
                {"label":"Boomerang-high","notation":"_4,_6,HP","pretty":"←,→,HP"},
                {"label":"Boomerang-low","notation":"_4,_6,LK","pretty":"←,→,LK"},
                {"label":"Projectile Invisibility","notation":"_4,_6,HK","pretty":"←,→,HK"},
                {"label":"Crunch Kick","notation":"_2,_6,LK","pretty":"↓,→,LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_8,_8,_2,_6,HP","pretty":"↑,↑,↓,→,HP"},
                {"label":"Fatality 2           (close)","notation":"RUN,RUN,RUN,BL,RUN","pretty":"RUN,RUN,RUN,BL,RUN"},
                {"label":"Animality                (close)","notation":"_6,_2,_6,_6,LK","pretty":"→,↓,→,→,LK"},
                {"label":"Friendship","notation":"_4,_2,_4,_4,HK","pretty":"←,↓,←,←,HK"},
                {"label":"Babality","notation":"_2,_2,_6,_2,HK","pretty":"↓,↓,→,↓,HK"},
                {"label":"Stage Fatality","notation":"_4,_6,_2,RUN","pretty":"←,→,↓,RUN"},
                {"label":"Brutality","notation":"HP,LK,HP,LP,HK,HK,LK,BL,BL,HP,HK","pretty":"HP,LK,HP,LP,HK,HK,LK,BL,BL,HP,HK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(22%)","notation":"HP,HP,_2_+LP,_2_+HP","pretty":"HP,HP,↓+LP,↓+HP"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(25%)","notation":"HP,HP,_2_+LP,LK,HK,_4_+LK,_4_+HK","pretty":"HP,HP,↓+LP,LK,HK,←+LK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "liu-kang",
          "name": "Liu Kang",
          "number": 17,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fireball-high (also in air)","notation":"_6,_6,HP","pretty":"→,→,HP"},
                {"label":"Fireball-low","notation":"_6,_6,LP","pretty":"→,→,LP"},
                {"label":"Flying Kick","notation":"_6,_6,HK","pretty":"→,→,HK"},
                {"label":"Bicycle Kick","notation":"hold LK for 3 sec","pretty":"hold LK for 3 sec"},
                {"label":"Fast Bicycle            (TRILOGY)","notation":"hold LK _4,_6","pretty":"hold LK ←,→"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1","notation":"_6,_6,_2,_2,LK","pretty":"→,→,↓,↓,LK"},
                {"label":"Fatality 2","notation":"_8,_2,_8,_8,BL_+RUN","pretty":"↑,↓,↑,↑,BL+RUN"},
                {"label":"Animality                      (sweep)","notation":"_2,_2,_8","pretty":"↓,↓,↑"},
                {"label":"Friendship          (sweep)","notation":"RUN,RUN,RUN,_2_+RUN","pretty":"RUN,RUN,RUN,↓+RUN"},
                {"label":"Babality","notation":"_2,_2,_2,HK","pretty":"↓,↓,↓,HK"},
                {"label":"Stage Fatality","notation":"RUN,BL,BL,LK","pretty":"RUN,BL,BL,LK"},
                {"label":"Brutality","notation":"HP,LP,HP,BL,LK,HK,LK,HK,LP,LP,HP","pretty":"HP,LP,HP,BL,LK,HK,LK,HK,LP,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(13%)","notation":"HP,HP,_4_+LP(juggle)","pretty":"HP,HP,←+LP(juggle)"},
                {"label":"(22%)","notation":"[LK/HK],LK,HK,LK","pretty":"[LK/HK],LK,HK,LK"},
                {"label":"(25%)","notation":"HP,LK,LK,HK,LK","pretty":"HP,LK,LK,HK,LK"},
                {"label":"(29%)","notation":"HP,HP,BL,LK,LK,HK,LK","pretty":"HP,HP,BL,LK,LK,HK,LK"}
              ]
            }
          ]
        },
        {
          "id": "sektor",
          "name": "Sektor",
          "number": 18,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Teleport Punch (also in air)","notation":"_6,_6,LK","pretty":"→,→,LK"},
                {"label":"Missile","notation":"_6,_6,LP","pretty":"→,→,LP"},
                {"label":"Homing Missile","notation":"_6,_2,_4,HP","pretty":"→,↓,←,HP"},
                {"label":"Double Missile            (TRILOGY)","notation":"_4,_4,_6,HP","pretty":"←,←,→,HP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                (sweep)","notation":"LP,RUN,RUN,BL","pretty":"LP,RUN,RUN,BL"},
                {"label":"Fatality 2                (jump)","notation":"_6,_6,_6,_4,BL","pretty":"→,→,→,←,BL"},
                {"label":"Animality                   (close)","notation":"_6,_6,_2,_8","pretty":"→,→,↓,↑"},
                {"label":"Friendship             (far)","notation":"RUN,RUN,RUN,RUN,_2","pretty":"RUN,RUN,RUN,RUN,↓"},
                {"label":"Babality","notation":"_4,_2,_2,_2,HK","pretty":"←,↓,↓,↓,HK"},
                {"label":"Stage Fatality","notation":"RUN,RUN,RUN,_2","pretty":"RUN,RUN,RUN,↓"},
                {"label":"Brutality","notation":"HP,HP,BL,BL,HK,HK,LK,LK,LP,LP,HP","pretty":"HP,HP,BL,BL,HK,HK,LK,LK,LP,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"[LK/HK],HK","pretty":"[LK/HK],HK"},
                {"label":"(18%)","notation":"HP,HP,_2_+LP(juggle)","pretty":"HP,HP,↓+LP(juggle)"},
                {"label":"(22%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"},
                {"label":"(26%)","notation":"HP,HP,HK,HK,_4_+HK","pretty":"HP,HP,HK,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "classic-sub-zero",
          "name": "Classic Sub-Zero",
          "number": 19,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Freeze","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Ground Freeze","notation":"_2,_4,LK","pretty":"↓,←,LK"},
                {"label":"Slide","notation":"_4_+LP_+BL_+LK","pretty":"←+LP+BL+LK"},
                {"label":"Ice Shower-over              (TRILOGY)","notation":"_2,_6,HP","pretty":"↓,→,HP"},
                {"label":"Ice Shower-front          (TRILOGY)","notation":"_2,_6,_4,HP","pretty":"↓,→,←,HP"},
                {"label":"Ice Shower-behind         (TRILOGY)","notation":"_2,_4,_6,HP","pretty":"↓,←,→,HP"},
                {"label":"Ice Clone                    (TRILOGY)","notation":"_2,_4,LP","pretty":"↓,←,LP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_2,_2,_2,_6,HP","pretty":"↓,↓,↓,→,HP"},
                {"label":"Fatality 2(INCLUDED MAME)(close)","notation":"_2,_6,_6,_6,HP","pretty":"↓,→,→,→,HP"},
                {"label":"Friendship               (close)","notation":"_2,_4,_4,_6,LK","pretty":"↓,←,←,→,LK"},
                {"label":"Babality","notation":"_2,_4,_4,HK","pretty":"↓,←,←,HK"},
                {"label":"Stage Fatality","notation":"_6,_2,_6,_6,HP","pretty":"→,↓,→,→,HP"},
                {"label":"Brutality","notation":"HP,LP,HP,BL,LK,LK,HK,HK,LP,HP,LP","pretty":"HP,LP,HP,BL,LK,LK,HK,HK,LP,HP,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"LK,_4_+HK,_6_+LK","pretty":"LK,←+HK,→+LK"},
                {"label":"(22%)","notation":"HP,HP,_2_+LP,_2_+HP(juggle)","pretty":"HP,HP,↓+LP,↓+HP(juggle)"},
                {"label":"(26%)","notation":"HP,HP,LK,_4_+HK,_6_+LK","pretty":"HP,HP,LK,←+HK,→+LK"}
              ]
            }
          ]
        },
        {
          "id": "cyrax",
          "name": "Cyrax",
          "number": 20,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Grenade-close","notation":"HOLD LK _4,_4,HK","pretty":"HOLD LK ←,←,HK"},
                {"label":"Grenade-far","notation":"HOLD LK _6,_6,HK","pretty":"HOLD LK →,→,HK"},
                {"label":"Net","notation":"_4,_4,LK","pretty":"←,←,LK"},
                {"label":"Teleport","notation":"_6,_2,BL","pretty":"→,↓,BL"},
                {"label":"Air Throw                foe in air","notation":"_2,_6,BL,LP","pretty":"↓,→,BL,LP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1","notation":"_2,_2,_8,_2,HP","pretty":"↓,↓,↑,↓,HP"},
                {"label":"Fatality 2              (close)","notation":"_2,_2,_6,_8,RUN","pretty":"↓,↓,→,↑,RUN"},
                {"label":"Animality                   (close)","notation":"_8,_8,_2,_2","pretty":"↑,↑,↓,↓"},
                {"label":"Friendship","notation":"RUN,RUN,RUN,_8","pretty":"RUN,RUN,RUN,↑"},
                {"label":"Babality","notation":"_6,_6,_4,HP","pretty":"→,→,←,HP"},
                {"label":"Stage Fatality","notation":"RUN,BL,RUN","pretty":"RUN,BL,RUN"},
                {"label":"Brutality","notation":"HP,HK,HP,HK,HK,HP,HK,HP,HK,LK,LP","pretty":"HP,HK,HP,HK,HK,HP,HK,HP,HK,LK,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,LP","pretty":"HP,HP,LP"},
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(30%)","notation":"HP,HP,HK,HP,HK,_4_+HK","pretty":"HP,HP,HK,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "shang-tsung",
          "name": "Shang Tsung",
          "number": 21,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Flaming Skull-single","notation":"_4,_4,HP","pretty":"←,←,HP"},
                {"label":"Flaming Skull-double","notation":"_4,_4,_6,HP","pretty":"←,←,→,HP"},
                {"label":"Flaming Skull-triple","notation":"_4,_4,_6,_6,HP","pretty":"←,←,→,→,HP"},
                {"label":"Volcanic Eruption","notation":"_6,_4,_4,LK","pretty":"→,←,←,LK"}
              ]
            },
            {
              "name": "Morphs",
              "moves": [
                {"label":"Cyrax","notation":"BL,BL,BL","pretty":"BL,BL,BL"},
                {"label":"Jade","notation":"_6,_6,_2,_2,BL","pretty":"→,→,↓,↓,BL"},
                {"label":"Jax","notation":"_6,_6,_2,LP","pretty":"→,→,↓,LP"},
                {"label":"Kabal","notation":"LP,BL,HK","pretty":"LP,BL,HK"},
                {"label":"Kano","notation":"_4,_6,BL","pretty":"←,→,BL"},
                {"label":"Kitana","notation":"_6,_2,_6,RUN","pretty":"→,↓,→,RUN"},
                {"label":"Kung Lao","notation":"RUN,RUN,BL,RUN","pretty":"RUN,RUN,BL,RUN"},
                {"label":"Liu Kang","notation":"_4,_8,_6,_2","pretty":"←,↑,→,↓"},
                {"label":"Nightwolf","notation":"_8,_8,_8","pretty":"↑,↑,↑"},
                {"label":"Reptile","notation":"RUN,BL,BL,HK","pretty":"RUN,BL,BL,HK"},
                {"label":"Scorpion","notation":"_2,_2,_6,LP","pretty":"↓,↓,→,LP"},
                {"label":"Sektor","notation":"_2,_6,_4,RUN","pretty":"↓,→,←,RUN"},
                {"label":"Sheeva","notation":"_6,_2,_6,LK or hold LK _6,_2,_6","pretty":"→,↓,→,LK or hold LK →,↓,→"},
                {"label":"Sindel","notation":"_4,_2,_4,LK","pretty":"←,↓,←,LK"},
                {"label":"Sonya","notation":"_2_+LP_+BL_+RUN","pretty":"↓+LP+BL+RUN"},
                {"label":"Stryker","notation":"_6,_6,_6,HK","pretty":"→,→,→,HK"},
                {"label":"Unmasked Sub-Zero","notation":"_6,_2,_6,HP","pretty":"→,↓,→,HP"},
                {"label":"Mileena","notation":"RUN,BL,HK","pretty":"RUN,BL,HK"},
                {"label":"Ermac","notation":"_2,_2,_8","pretty":"↓,↓,↑"},
                {"label":"Classic Sub-Zero","notation":"BL,BL,RUN,RUN","pretty":"BL,BL,RUN,RUN"}
              ]
            },
            {
              "name": "Trilogy Morphs",
              "moves": [
                {"label":"Human Smoke","notation":"BL,RUN,LK","pretty":"BL,RUN,LK"},
                {"label":"Robot Smoke","notation":"_4,_4,_2,LK","pretty":"←,←,↓,LK"},
                {"label":"Rain","notation":"RUN,BL,LK","pretty":"RUN,BL,LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1          (close)","notation":"hold LP _2,_6,_6,_2","pretty":"hold LP ↓,→,→,↓"},
                {"label":"Fatality 2        (close)","notation":"hold LP RUN,BL,RUN,BL","pretty":"hold LP RUN,BL,RUN,BL"},
                {"label":"Animality           (sweep)","notation":"hold HP RUN,RUN,RUN","pretty":"hold HP RUN,RUN,RUN"},
                {"label":"Friendship","notation":"LK,RUN,RUN,_2","pretty":"LK,RUN,RUN,↓"},
                {"label":"Babality","notation":"RUN,RUN,RUN,LK","pretty":"RUN,RUN,RUN,LK"},
                {"label":"Stage Fatality","notation":"_8,_8,_4,LP","pretty":"↑,↑,←,LP"},
                {"label":"Brutality:","notation":"HP,BL,BL,BL,LK,HP,LP,LP,BL,BL,BL","pretty":"HP,BL,BL,BL,LK,HP,LP,LP,BL,BL,BL"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(22%)","notation":"HP,HP,LP,_4_+HK","pretty":"HP,HP,LP,←+HK"},
                {"label":"(27%)","notation":"[LK/HK],HP,HP,LP,_4_+HK","pretty":"[LK/HK],HP,HP,LP,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "mileena",
          "name": "Mileena",
          "number": 22,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Ground Roll","notation":"_4,_4,_2,HK","pretty":"←,←,↓,HK"},
                {"label":"Sai Throw (also in air)","notation":"hold HP for 2 sec","pretty":"hold HP for 2 sec"},
                {"label":"Teleport Kick","notation":"_6,_6,LK","pretty":"→,→,LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                 (far)","notation":"_4,_4,_4,_6,LK","pretty":"←,←,←,→,LK"},
                {"label":"Fatality 2               (close)","notation":"_2,_6,_2,_6,LP","pretty":"↓,→,↓,→,LP"},
                {"label":"Animality                (close)","notation":"_6,_2,_2,_6,HK","pretty":"→,↓,↓,→,HK"},
                {"label":"Friendship","notation":"_2,_2,_4,_6,HP","pretty":"↓,↓,←,→,HP"},
                {"label":"Babality","notation":"_2,_2,_6,_6,HP","pretty":"↓,↓,→,→,HP"},
                {"label":"Stage Fatality","notation":"_2,_2,_2,LP","pretty":"↓,↓,↓,LP"},
                {"label":"Brutality","notation":"HP,LP,LP,HP,BL,HK,LK,HK,BL,HP,LP","pretty":"HP,LP,LP,HP,BL,HK,LK,HK,BL,HP,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(16%)","notation":"[LK/HK],HK,_2,_6,LK(juggle)","pretty":"[LK/HK],HK,↓,→,LK(juggle)"},
                {"label":"(22%)","notation":"HP,HP,_8_+LP,_2_+LP","pretty":"HP,HP,↑+LP,↓+LP"},
                {"label":"(23%)","notation":"[LK/HK],HK,_8_+LK,_8_+HK","pretty":"[LK/HK],HK,↑+LK,↑+HK"},
                {"label":"(24%)","notation":"HP,HP,HK,HK,_2,_6,LK(juggle)","pretty":"HP,HP,HK,HK,↓,→,LK(juggle)"},
                {"label":"(30%)","notation":"HP,HP,HK,HK,_8_+LK,_8_+HK","pretty":"HP,HP,HK,HK,↑+LK,↑+HK"}
              ]
            }
          ]
        },
        {
          "id": "sheeva",
          "name": "Sheeva",
          "number": 23,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Teleport Stomp","notation":"_2,_8","pretty":"↓,↑"},
                {"label":"Ground Stomp","notation":"_4,_2,_4,HK","pretty":"←,↓,←,HK"},
                {"label":"Fireball","notation":"_2,_6,HP","pretty":"↓,→,HP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_6,_2,_2,_6,LP","pretty":"→,↓,↓,→,LP"},
                {"label":"Fatality 2             (close)","notation":"hold HK _4,_6,_6","pretty":"hold HK ←,→,→"},
                {"label":"Animality               (close)","notation":"RUN,BL,BL,BL,BL","pretty":"RUN,BL,BL,BL,BL"},
                {"label":"Friendship              (sweep)","notation":"_6,_6,_2,_6, HP","pretty":"→,→,↓,→, HP"},
                {"label":"Babality","notation":"_2,_2,_2,_4,HK","pretty":"↓,↓,↓,←,HK"},
                {"label":"Stage Fatality","notation":"_2,_6,_2,_6,LP","pretty":"↓,→,↓,→,LP"},
                {"label":"Brutality","notation":"HP,LP,BL,LK,HK,BL,HK,LK,BL,LP,HP","pretty":"HP,LP,BL,LK,HK,BL,HK,LK,BL,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(25%)","notation":"HP,HP,LP,_6_+HP(juggle)","pretty":"HP,HP,LP,→+HP(juggle)"},
                {"label":"(27%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(42%)","notation":"HP,HP,LP,HK,HK,LK,_4_+HK","pretty":"HP,HP,LP,HK,HK,LK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "rain",
          "name": "Rain",
          "number": 24,
          "availability": "UMK3 UK",
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Orb","notation":"_2,_6,HP","pretty":"↓,→,HP"},
                {"label":"Lightning","notation":"_4,_4,HP","pretty":"←,←,HP"},
                {"label":"Super Roundhouse","notation":"_4_+HK","pretty":"←+HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1:              (sweep)","notation":"_2,_2,_4,_6,HK","pretty":"↓,↓,←,→,HK"},
                {"label":"Babality","notation":"_6,_4,_4,HP","pretty":"→,←,←,HP"},
                {"label":"Friendship:","notation":"_6,_6,_6,LP","pretty":"→,→,→,LP"},
                {"label":"Stage Fatality","notation":"_6,_2,_6,LP","pretty":"→,↓,→,LP"},
                {"label":"Brutality","notation":"HP,HP,BL,LK,HK,BL,LK,HK,BL,HP,LP","pretty":"HP,HP,BL,LK,HK,BL,LK,HK,BL,HP,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(22%)","notation":"HP,HP,LP,HP","pretty":"HP,HP,LP,HP"},
                {"label":"(23%)","notation":"[LK/HK],HK,LP,HP","pretty":"[LK/HK],HK,LP,HP"},
                {"label":"(30%)","notation":"[LK/HK],HK,LK,HK,_4_+HK(juggle)","pretty":"[LK/HK],HK,LK,HK,←+HK(juggle)"}
              ]
            }
          ]
        },
        {
          "id": "noob-saibot",
          "name": "Noob Saibot",
          "number": 25,
          "availability": "Team Edition",
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Teleport","notation":"_2,_8","pretty":"↓,↑","platformOverrides":{"sega":{"label":"Pile Driver"}}},
                {"label":"Shadow Throw","notation":"_6,_6,HP","pretty":"→,→,HP","platforms":["sega"],"platformStatus":{"sega":"unverified"},"platformNotes":{"sega":"Комбинация для SEGA добавлена по присланному списку и требует проверки."}},
                {"label":"Disabler","notation":"_2,_6,LP","pretty":"↓,→,LP","platforms":["sega"],"platformStatus":{"sega":"unverified"},"platformNotes":{"sega":"Комбинация для SEGA добавлена по присланному списку и требует проверки."}}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Brutality","notation":"HP,LK,LP,BL,LK,HK,HP,LP,BL,LK,HK","pretty":"HP,LK,LP,BL,LK,HK,HP,LP,BL,LK,HK","platforms":["sega"],"platformStatus":{"sega":"unverified"},"platformNotes":{"sega":"Комбинация Brutality для SEGA добавлена по присланному списку и требует проверки."}}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"HK,LP(juggle)","pretty":"HK,LP(juggle)"},
                {"label":"(18%)","notation":"HP,HP,LP(juggle)","pretty":"HP,HP,LP(juggle)"},
                {"label":"(19%)","notation":"HK,_2_+LP,_2_+HP(juggle)","pretty":"HK,↓+LP,↓+HP(juggle)"},
                {"label":"(22%)","notation":"HP,HP,_2_+LP,_2_+HP(juggle)","pretty":"HP,HP,↓+LP,↓+HP(juggle)"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(27%)","notation":"HP,HP,HK,HK,LK,_4_+HK","pretty":"HP,HP,HK,HK,LK,←+HK"},
                {"label":"Combo 1","notation":"LK,LK,LK","pretty":"LK,LK,LK","platforms":["sega"],"platformStatus":{"sega":"unverified"},"platformNotes":{"sega":"Автокомбо для SEGA добавлено со скриншота и требует проверки."}},
                {"label":"(23%)","notation":"LK,LK,LK,LK","pretty":"LK,LK,LK,LK","platforms":["sega"],"platformStatus":{"sega":"unverified"},"platformNotes":{"sega":"Автокомбо для SEGA добавлено из таблицы UMK3 Sega."}},
                {"label":"(22%)","notation":"HP,HP,LP,HK","pretty":"HP,HP,LP,HK","platforms":["sega"],"platformStatus":{"sega":"unverified"},"platformNotes":{"sega":"Автокомбо для SEGA добавлено со скриншота и сверено с таблицей UMK3 Sega."}},
                {"label":"Combo 3","notation":"HP,HP,LP,HK,_2,_8,_6","pretty":"HP,HP,LP,HK,↓,↑,→","platforms":["sega"],"platformStatus":{"sega":"unverified"},"platformNotes":{"sega":"Автокомбо для SEGA добавлено со скриншота и требует проверки."}},
                {"label":"Combo 4","notation":"_8,HK,LK,LP,HP,HP,HP,LP,HK,_2,_8,_6","pretty":"↑,HK,LK,LP,HP,HP,HP,LP,HK,↓,↑,→","platforms":["sega"],"platformStatus":{"sega":"unverified"},"platformNotes":{"sega":"Автокомбо для SEGA добавлено со скриншота и требует проверки."}},
                {"label":"Combo 5","notation":"_8,LP,HP,LK,HK,HK,LK,LK,LK","pretty":"↑,LP,HP,LK,HK,HK,LK,LK,LK","platforms":["sega"],"platformStatus":{"sega":"unverified"},"platformNotes":{"sega":"Автокомбо для SEGA добавлено со скриншота и требует проверки."}}
              ]
            }
          ]
        },
        {
          "id": "motaro",
          "name": "Motaro",
          "number": 26,
          "availability": "UMK3 UK · скрытый боец",
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fireball:","notation":"_6,_2,_4,HP","pretty":"→,↓,←,HP"},
                {"label":"Teleport:","notation":"_2,_8","pretty":"↓,↑"},
                {"label":"Grab And Smash:","notation":"_6,_6,HP","pretty":"→,→,HP"}
              ]
            }
          ]
        },
        {
          "id": "shao-kahn",
          "name": "Shao Kahn",
          "number": 27,
          "availability": "UMK3 UK · скрытый боец",
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fireball:","notation":"_4,_4,_6,LP","pretty":"←,←,→,LP"},
                {"label":"Slam:","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Upper Slam:","notation":"_2,_6,HP","pretty":"↓,→,HP"},
                {"label":"Hammer:","notation":"_4,_6,HP","pretty":"←,→,HP"},
                {"label":"Taunt 1:","notation":"_2,_2,LK","pretty":"↓,↓,LK"},
                {"label":"Taunt 2:","notation":"_2,_2,HK","pretty":"↓,↓,HK"}
              ]
            }
          ]
        }
      ]
    },
    "umk3tm": {
      "id": "umk3tm",
      "fighters": [
        {
          "id": "kabal",
          "name": "Kabal",
          "number": 1,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Dash","notation":"_4,_6,LK","pretty":"←,→,LK"},
                {"label":"Eye Spark (also in air)","notation":"_4,_4,HP","pretty":"←,←,HP"},
                {"label":"Ground Saw","notation":"_4,_4,_4,RUN","pretty":"←,←,←,RUN"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (sweep)","notation":"_2,_2,_4,_6,BL","pretty":"↓,↓,←,→,BL"},
                {"label":"Fatality 2              (close)","notation":"RUN,BL,BL,BL,HK","pretty":"RUN,BL,BL,BL,HK"},
                {"label":"Animality           (close)","notation":"hold HP _6,_6,_2,_6","pretty":"hold HP →,→,↓,→"},
                {"label":"Friendship            (sweep)","notation":"RUN,LK,RUN,RUN,_8","pretty":"RUN,LK,RUN,RUN,↑"},
                {"label":"Babality","notation":"RUN,RUN,LK","pretty":"RUN,RUN,LK"},
                {"label":"Stage Fatality","notation":"BL,BL,HK","pretty":"BL,BL,HK"},
                {"label":"Brutality","notation":"HP,BL,LK,LK,LK,HK,LP,LP,LP,HP,LP","pretty":"HP,BL,LK,LK,LK,HK,LP,LP,LP,HP,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(13%)","notation":"HP,HP,_2_+HP(juggle)","pretty":"HP,HP,↓+HP(juggle)"},
                {"label":"(18%)","notation":"[LK/HK],LK,_4_+HK","pretty":"[LK/HK],LK,←+HK"},
                {"label":"(17%)","notation":"HP,HP,_2_+LP,_2_+HP","pretty":"HP,HP,↓+LP,↓+HP"},
                {"label":"(19%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"},
                {"label":"(24%)","notation":"[LK/HK],LK,HK,_4_+HK","pretty":"[LK/HK],LK,HK,←+HK"},
                {"label":"(15%)","notation":"[LK/HK],LK,HP,HP,_2_+HP(juggle)","pretty":"[LK/HK],LK,HP,HP,↓+HP(juggle)"},
                {"label":"(17%)","notation":"[LK/HK],LK,HP,HP,_2_+LP,_2_+HP","pretty":"[LK/HK],LK,HP,HP,↓+LP,↓+HP"},
                {"label":"(18%)","notation":"[LK/HK],LK,HP,HP,HK,_4_+HK","pretty":"[LK/HK],LK,HP,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "human-smoke",
          "name": "Human Smoke",
          "number": 2,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Spear","notation":"_4,_4,LP","pretty":"←,←,LP"},
                {"label":"Teleport Punch","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Air Throw","notation":"BL in air","pretty":"BL in air"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1            (close)","notation":"RUN,BL,RUN,RUN,HK","pretty":"RUN,BL,RUN,RUN,HK"},
                {"label":"Babality","notation":"_2,_4,_4,_6,HP","pretty":"↓,←,←,→,HP"},
                {"label":"Stage Fatality","notation":"_6,_8,_8,LP","pretty":"→,↑,↑,LP"},
                {"label":"Brutality","notation":"HP,HP,BL,LK,HK,HP,HK,HP,HK,LP,LK","pretty":"HP,HP,BL,LK,HK,HP,HK,HP,HK,LP,LK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"[LK/HK],LP(juggle)","pretty":"[LK/HK],LP(juggle)"},
                {"label":"(18%)","notation":"HP,HP,_8_+LP","pretty":"HP,HP,↑+LP"},
                {"label":"(19%)","notation":"[LK/HK],_2_+LP,_2_+HP(juggle)","pretty":"[LK/HK],↓+LP,↓+HP(juggle)"},
                {"label":"(23%)","notation":"HK,HK,LK,_4_+HK","pretty":"HK,HK,LK,←+HK"},
                {"label":"(24%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "kung-lao",
          "name": "Kung Lao",
          "number": 3,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Hat Throw","notation":"_4,_6,LP","pretty":"←,→,LP"},
                {"label":"Teleport","notation":"_2,_8","pretty":"↓,↑"},
                {"label":"Torpedo Kick (in air)","notation":"_2_+HK in air","pretty":"↓+HK in air"},
                {"label":"Spin Attack (tap RUN to spin)","notation":"_6,_2,_6,RUN","pretty":"→,↓,→,RUN"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1","notation":"RUN,BL,RUN,BL,_2","pretty":"RUN,BL,RUN,BL,↓"},
                {"label":"Fatality 2               (sweep)","notation":"_6,_6,_4,_2,HP","pretty":"→,→,←,↓,HP"},
                {"label":"Animality            (close)","notation":"RUN,RUN,RUN,RUN,BL","pretty":"RUN,RUN,RUN,RUN,BL"},
                {"label":"Friendship                (sweep)","notation":"RUN,LP,RUN,LK","pretty":"RUN,LP,RUN,LK"},
                {"label":"Babality","notation":"_2,_6,_6,HP","pretty":"↓,→,→,HP"},
                {"label":"Stage Fatality","notation":"_2,_2,_6,_6,LK","pretty":"↓,↓,→,→,LK"},
                {"label":"Brutality","notation":"HP,LP,LK,HK,BL,HP,LP,LK,HK,BL,HP","pretty":"HP,LP,LK,HK,BL,HP,LP,LK,HK,BL,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,LK,_4_+HK","pretty":"HP,LK,←+HK"},
                {"label":"(19%)","notation":"[LK/HK],LK,HK","pretty":"[LK/HK],LK,HK"},
                {"label":"(34%)","notation":"HP,LP,HP,LP,LK,LK,HK","pretty":"HP,LP,HP,LP,LK,LK,HK"}
              ]
            }
          ]
        },
        {
          "id": "kano",
          "name": "Kano",
          "number": 4,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Knife Throw","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Knife Uppercut","notation":"_2,_6,HP","pretty":"↓,→,HP"},
                {"label":"Cannonball-straight","notation":"hold LK for 3 sec","pretty":"hold LK for 3 sec"},
                {"label":"Cannonball-upward","notation":"_6,_2,_6,HK","pretty":"→,↓,→,HK"},
                {"label":"Neck Choke","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Air Throw","notation":"BL in air","pretty":"BL in air"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1          (close)","notation":"hold LP _6,_2,_2,_6","pretty":"hold LP →,↓,↓,→"},
                {"label":"Fatality 2                  (sweep)","notation":"LP,BL,BL,HK","pretty":"LP,BL,BL,HK"},
                {"label":"Animality              (close)","notation":"hold HP BL,BL,BL","pretty":"hold HP BL,BL,BL"},
                {"label":"Friendship                (sweep)","notation":"LK,RUN,RUN,HK","pretty":"LK,RUN,RUN,HK"},
                {"label":"Babality","notation":"_6,_6,_2,_2,LK","pretty":"→,→,↓,↓,LK"},
                {"label":"Stage Fatality","notation":"_8,_8,_4,LK","pretty":"↑,↑,←,LK"},
                {"label":"Brutality","notation":"HP,LP,BL,LP,HP,BL,HK,LK,BL,HK,LK","pretty":"HP,LP,BL,LP,HP,BL,HK,LK,BL,HK,LK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"[LK/HK],LP(juggle)","pretty":"[LK/HK],LP(juggle)"},
                {"label":"(18%)","notation":"HP,HP,LP(juggle)","pretty":"HP,HP,LP(juggle)"},
                {"label":"(19%)","notation":"[LK/HK],_2_+LP,_2_+HP(juggle)","pretty":"[LK/HK],↓+LP,↓+HP(juggle)"},
                {"label":"(22%)","notation":"HP,HP,_2_+LP,_2_+HP(juggle)","pretty":"HP,HP,↓+LP,↓+HP(juggle)"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(26%)","notation":"HP,HP,HK,LK,_4_+HK","pretty":"HP,HP,HK,LK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "reptile",
          "name": "Reptile",
          "number": 5,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Acid Spit","notation":"_6,_6,HP","pretty":"→,→,HP"},
                {"label":"Forceball-slow","notation":"_4,_4,HP_+LP","pretty":"←,←,HP+LP"},
                {"label":"Forceball-fast","notation":"_6,_6,HP_+LP","pretty":"→,→,HP+LP"},
                {"label":"Slide","notation":"_4_+LP_+BL_+LK","pretty":"←+LP+BL+LK"},
                {"label":"Invisibility","notation":"_8,_2,HK","pretty":"↑,↓,HK"},
                {"label":"Super Elbow","notation":"_4,_6,LK","pretty":"←,→,LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                   (jump)","notation":"_4,_6,_2,BL","pretty":"←,→,↓,BL"},
                {"label":"Fatality 2               (sweep)","notation":"_6,_6,_8,_8,HK","pretty":"→,→,↑,↑,HK"},
                {"label":"Animality                (close)","notation":"_2,_2,_2,_8,HK","pretty":"↓,↓,↓,↑,HK"},
                {"label":"Friendship               (close)","notation":"_2,_6,_6,_4,HK","pretty":"↓,→,→,←,HK"},
                {"label":"Babality","notation":"_6,_6,_4,_2,LK","pretty":"→,→,←,↓,LK"},
                {"label":"Stage Fatality","notation":"BL,RUN,BL,BL","pretty":"BL,RUN,BL,BL"},
                {"label":"Brutality","notation":"HP,BL,HK,HK,BL,HP,LP,LK,LK,BL,LP","pretty":"HP,BL,HK,HK,BL,HP,LP,LK,LK,BL,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,_2_+LP(juggle)","pretty":"HP,HP,↓+LP(juggle)"},
                {"label":"(21%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(24%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "ermac",
          "name": "Ermac",
          "number": 6,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fire Teleport Punch","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Green Sphere","notation":"_2,_4,LP","pretty":"↓,←,LP"},
                {"label":"Soul Slam","notation":"_4,_2,_4,HK","pretty":"←,↓,←,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1            (sweep)","notation":"_2,_8,_2,_2,_2,BL","pretty":"↓,↑,↓,↓,↓,BL"},
                {"label":"Fatality 2            (close)","notation":"RUN,BL,RUN,RUN,HK","pretty":"RUN,BL,RUN,RUN,HK"},
                {"label":"Stage Fatality","notation":"RUN,RUN,RUN,RUN,LK","pretty":"RUN,RUN,RUN,RUN,LK"},
                {"label":"Babality","notation":"_2,_2,_4,_4,HP","pretty":"↓,↓,←,←,HP"},
                {"label":"Brutality","notation":"HP,HP,LP,BL,HK,LK,BL,HP,LP,LK,HK","pretty":"HP,HP,LP,BL,HK,LK,BL,HP,LP,LK,HK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"[LK/HK],LP(juggle)","pretty":"[LK/HK],LP(juggle)"},
                {"label":"(19%)","notation":"HP,HP,_4_+LP,_2,_6,LP(juggle)","pretty":"HP,HP,←+LP,↓,→,LP(juggle)"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(24%)","notation":"HP,HP,_4_+LP,HK,LK","pretty":"HP,HP,←+LP,HK,LK"}
              ]
            }
          ]
        },
        {
          "id": "nightwolf",
          "name": "Nightwolf",
          "number": 7,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Arrow","notation":"_2,_4,LP","pretty":"↓,←,LP"},
                {"label":"Hatchet Uppercut","notation":"_2,_6,HP","pretty":"↓,→,HP"},
                {"label":"Shadow Charge","notation":"_6,_6,LK","pretty":"→,→,LK"},
                {"label":"Reflect Shield","notation":"_4,_4,_4,HK","pretty":"←,←,←,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_8,_8,_4,_6,BL","pretty":"↑,↑,←,→,BL"},
                {"label":"Fatality 2                  (sweep)","notation":"_4,_4,_2,HP","pretty":"←,←,↓,HP"},
                {"label":"Animality                   (close)","notation":"_6,_6,_2,_2","pretty":"→,→,↓,↓"},
                {"label":"Friendship               (sweep)","notation":"RUN,RUN,RUN,_2","pretty":"RUN,RUN,RUN,↓"},
                {"label":"Babality","notation":"_6,_4,_6,_4,LP","pretty":"→,←,→,←,LP"},
                {"label":"Stage Fatality","notation":"RUN,RUN,BL","pretty":"RUN,RUN,BL"},
                {"label":"Brutality","notation":"HP,HP,HK,LK,LK,BL,BL,LP,LP,HP,HK","pretty":"HP,HP,HK,LK,LK,BL,BL,LP,LP,HP,HK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(22%)","notation":"HP,HP,LP,HK","pretty":"HP,HP,LP,HK"},
                {"label":"(23%)","notation":"HP,HP,LP,Hatchet Uppercut(juggle)","pretty":"HP,HP,LP,Hatchet Uppercut(juggle)"},
                {"label":"(27%)","notation":"[LK/HK],HP,HP,LP,HK","pretty":"[LK/HK],HP,HP,LP,HK"},
                {"label":"(28%)","notation":"[LK/HK],HP,HP,LP,Hatchet Uppercut(juggle)","pretty":"[LK/HK],HP,HP,LP,Hatchet Uppercut(juggle)"}
              ]
            }
          ]
        },
        {
          "id": "robot-smoke",
          "name": "Robot Smoke",
          "number": 8,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Harpoon","notation":"_4,_4,LP","pretty":"←,←,LP"},
                {"label":"Teleport Punch (also in  air)","notation":"_6,_6,LK","pretty":"→,→,LK"},
                {"label":"Invisibility","notation":"_8,_8,RUN","pretty":"↑,↑,RUN"},
                {"label":"Air Throw","notation":"BL in air","pretty":"BL in air"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                    (far)","notation":"_8,_8,_6,_2","pretty":"↑,↑,→,↓"},
                {"label":"Fatality 2     (sweep)","notation":"hold BL_+RUN _2,_2,_6,_8","pretty":"hold BL+RUN ↓,↓,→,↑"},
                {"label":"Animality                    (jump)","notation":"_2,_6,_6,BL","pretty":"↓,→,→,BL"},
                {"label":"Frienship                (sweep)","notation":"RUN,RUN,RUN,HK","pretty":"RUN,RUN,RUN,HK"},
                {"label":"Babality","notation":"_2,_2,_4,_4,HK","pretty":"↓,↓,←,←,HK"},
                {"label":"Stage Fatality","notation":"_6,_6,_2,LK","pretty":"→,→,↓,LK"},
                {"label":"Brutality","notation":"HP,LK,LK,HK,BL,BL,LP,LP,HP,BL,BL","pretty":"HP,LK,LK,HK,BL,BL,LP,LP,HP,BL,BL"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,LP","pretty":"HP,HP,LP"},
                {"label":"(18%)","notation":"HP,HP,HK","pretty":"HP,HP,HK"},
                {"label":"(19%)","notation":"[LK/HK],HK,LP","pretty":"[LK/HK],HK,LP"},
                {"label":"(26%)","notation":"HP,HP,LK,HK,LP","pretty":"HP,HP,LK,HK,LP"}
              ]
            }
          ]
        },
        {
          "id": "sindel",
          "name": "Sindel",
          "number": 9,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fireball-ground","notation":"_6,_6,LP","pretty":"→,→,LP"},
                {"label":"Fireball-air","notation":"_2,_6,LK in air","pretty":"↓,→,LK in air"},
                {"label":"Fly","notation":"_4,_4,_6,HK","pretty":"←,←,→,HK"},
                {"label":"Scream","notation":"_6,_6,_6,HP","pretty":"→,→,→,HP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1            (sweep)","notation":"RUN,RUN,BL,RUN,BL","pretty":"RUN,RUN,BL,RUN,BL"},
                {"label":"Fatality 2            (close)","notation":"RUN,BL,BL,RUN_+BL","pretty":"RUN,BL,BL,RUN+BL"},
                {"label":"Animality","notation":"_6,_6,_8,HP","pretty":"→,→,↑,HP"},
                {"label":"Friendship","notation":"RUN,RUN,RUN,RUN,RUN,_8","pretty":"RUN,RUN,RUN,RUN,RUN,↑"},
                {"label":"Babality","notation":"RUN,RUN,RUN,_8","pretty":"RUN,RUN,RUN,↑"},
                {"label":"Stage Fatality","notation":"_2,_2,_2,LP","pretty":"↓,↓,↓,LP"},
                {"label":"Brutality","notation":"HP,BL,LK,BL,LK,HK,BL,HK,LK,BL,LP","pretty":"HP,BL,LK,BL,LK,HK,BL,HK,LK,BL,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"HP,HP,_2_+HP(juggle)","pretty":"HP,HP,↓+HP(juggle)"},
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(25%)","notation":"HP,HP,LP,HK","pretty":"HP,HP,LP,HK"},
                {"label":"(27%)","notation":"[LK/HK],HP,HP,_2_+HP(juggle)","pretty":"[LK/HK],HP,HP,↓+HP(juggle)"},
                {"label":"(33%)","notation":"[LK/HK],HP,HP,LP,HK","pretty":"[LK/HK],HP,HP,LP,HK"}
              ]
            }
          ]
        },
        {
          "id": "jax",
          "name": "Jax",
          "number": 10,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Missile-single","notation":"_4,_6,HP","pretty":"←,→,HP"},
                {"label":"Missile-double","notation":"_6,_6,_4,_4,HP","pretty":"→,→,←,←,HP"},
                {"label":"Gotcha Grab","notation":"_6,_6_+LP, LPx3","pretty":"→,→+LP, LPx3"},
                {"label":"Backbreaker (in air)","notation":"BL in air","pretty":"BL in air"},
                {"label":"Quad Slam","notation":"throw, HPx4","pretty":"throw, HPx4"},
                {"label":"Ground Smash","notation":"hold LK for 3 sec","pretty":"hold LK for 3 sec"},
                {"label":"Dashing Pound","notation":"_6,_6,HK","pretty":"→,→,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1          (close)","notation":"hold BL _8,_2,_6,_8","pretty":"hold BL ↑,↓,→,↑"},
                {"label":"Fatality 2              (far)","notation":"RUN,BL,RUN,RUN,LK","pretty":"RUN,BL,RUN,RUN,LK"},
                {"label":"Animality           (close)","notation":"hold LP _6,_6,_2,_6","pretty":"hold LP →,→,↓,→"},
                {"label":"Friendship                (sweep)","notation":"LK,RUN,RUN,LK","pretty":"LK,RUN,RUN,LK"},
                {"label":"Babality","notation":"_2,_2,_2,LK","pretty":"↓,↓,↓,LK"},
                {"label":"Stage Fatality","notation":"_2,_6,_2,LP","pretty":"↓,→,↓,LP"},
                {"label":"Brutality","notation":"HP,HP,HP,BL,LP,HP,HP,HP,BL,LP,HP","pretty":"HP,HP,HP,BL,LP,HP,HP,HP,BL,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(24%)","notation":"HP,HP,BL,LP,_4_+HP","pretty":"HP,HP,BL,LP,←+HP"},
                {"label":"(33%)","notation":"[LK/HK],HK,_2_+HP,HP,BL,LP,_4_+HP","pretty":"[LK/HK],HK,↓+HP,HP,BL,LP,←+HP"}
              ]
            }
          ]
        },
        {
          "id": "sonya",
          "name": "Sonya",
          "number": 11,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Force Wave","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Leg Grab","notation":"_2_+LP_+BL","pretty":"↓+LP+BL"},
                {"label":"Square Wave","notation":"_6,_4,HP","pretty":"→,←,HP"},
                {"label":"Bicycle Kick","notation":"_4,_4,_2,HK","pretty":"←,←,↓,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1       (far)","notation":"hold BL_+RUN _8,_8,_4,_2","pretty":"hold BL+RUN ↑,↑,←,↓"},
                {"label":"Fatality 2","notation":"_4,_6,_2,_2,RUN","pretty":"←,→,↓,↓,RUN"},
                {"label":"Animality           (close)","notation":"hold LP _4,_6,_2,_6","pretty":"hold LP ←,→,↓,→"},
                {"label":"Friendship              (sweep)","notation":"_4,_6,_4,_2,RUN","pretty":"←,→,←,↓,RUN"},
                {"label":"Babality","notation":"_2,_2,_6,LK","pretty":"↓,↓,→,LK"},
                {"label":"Stage Fatality","notation":"_6,_6,_2,HP","pretty":"→,→,↓,HP"},
                {"label":"Brutality","notation":"HP,LK,BL,HP,LK,BL,HP,LP,BL,HK,LK","pretty":"HP,LK,BL,HP,LK,BL,HP,LP,BL,HK,LK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,_8_+LP(juggle)","pretty":"HP,HP,↑+LP(juggle)"},
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(22%)","notation":"HP,HP,LP,_4_+HP","pretty":"HP,HP,LP,←+HP"},
                {"label":"(27%)","notation":"[LK/HK],HP,HP,LP,_4_+HP","pretty":"[LK/HK],HP,HP,LP,←+HP"},
                {"label":"(27%)","notation":"[LK/HK],HK,HP,HP,_8_+LP(juggle)","pretty":"[LK/HK],HK,HP,HP,↑+LP(juggle)"},
                {"label":"(31%)","notation":"[LK/HK],HK,HP,HP,LP,_4_+HP","pretty":"[LK/HK],HK,HP,HP,LP,←+HP"}
              ]
            }
          ]
        },
        {
          "id": "kitana",
          "name": "Kitana",
          "number": 12,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fan Lift","notation":"_4,_4,_4,HP","pretty":"←,←,←,HP"},
                {"label":"Fan Throw (also in air)","notation":"_6,_6,HP_+LP","pretty":"→,→,HP+LP"},
                {"label":"Square Wave","notation":"_2,_4,HP","pretty":"↓,←,HP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1             (close)","notation":"RUN,RUN,BL,BL,LK","pretty":"RUN,RUN,BL,BL,LK"},
                {"label":"Fatality 2               (close)","notation":"_4,_2,_6,_6,HK","pretty":"←,↓,→,→,HK"},
                {"label":"Animality               (sweep)","notation":"_2,_2,_2,_2,RUN","pretty":"↓,↓,↓,↓,RUN"},
                {"label":"Friendship","notation":"_2,_4,_6,_6,LP","pretty":"↓,←,→,→,LP"},
                {"label":"Babality","notation":"_6,_6,_2,_6,HK","pretty":"→,→,↓,→,HK"},
                {"label":"Stage Fatality","notation":"_6,_2,_2,LK","pretty":"→,↓,↓,LK"},
                {"label":"Brutality:","notation":"HP,HP,BL,HK,BL,LK,BL,LP,BL,HP,BL","pretty":"HP,HP,BL,HK,BL,LK,BL,LP,BL,HP,BL"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(25%)","notation":"HP,HP,_4_+LP,_6_+HP","pretty":"HP,HP,←+LP,→+HP"}
              ]
            }
          ]
        },
        {
          "id": "stryker",
          "name": "Stryker",
          "number": 13,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Grenade-high","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Grenade-low","notation":"_2,_4,LP","pretty":"↓,←,LP"},
                {"label":"Gun Shot","notation":"_4,_6,HP","pretty":"←,→,HP"},
                {"label":"Baton Sweep","notation":"_6,_4,LP","pretty":"→,←,LP"},
                {"label":"Baton Toss","notation":"_6,_6,HK","pretty":"→,→,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_2,_6,_2,_6,BL","pretty":"↓,→,↓,→,BL"},
                {"label":"Fatality 2                    (far)","notation":"_6,_6,_6,LK","pretty":"→,→,→,LK"},
                {"label":"Animality                (sweep)","notation":"RUN,RUN,RUN,BL","pretty":"RUN,RUN,RUN,BL"},
                {"label":"Friendship","notation":"LP,RUN,RUN,LP","pretty":"LP,RUN,RUN,LP"},
                {"label":"Babality","notation":"_2,_6,_6,_4,HP","pretty":"↓,→,→,←,HP"},
                {"label":"Stage Fatality","notation":"_6,_8,_8,HK","pretty":"→,↑,↑,HK"},
                {"label":"Brutality:","notation":"HP,LP,HK,LK,HP,LP,LK,HK,HP,LK,LK","pretty":"HP,LP,HK,LK,HP,LP,LK,HK,HP,LK,LK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,LP(juggle)","pretty":"HP,HP,LP(juggle)"},
                {"label":"(19%)","notation":"[LK/HK],LK,_4_+HK","pretty":"[LK/HK],LK,←+HK"},
                {"label":"(23%)","notation":"[LK/HK],HP,HP,LP(juggle)","pretty":"[LK/HK],HP,HP,LP(juggle)"},
                {"label":"(23%)","notation":"[LK/HK],LK,_4_+LP,_4_+HK","pretty":"[LK/HK],LK,←+LP,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "scorpion",
          "name": "Scorpion",
          "number": 14,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Spear","notation":"_4,_4,LP","pretty":"←,←,LP"},
                {"label":"Teleport Punch","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Air Throw","notation":"BL in air","pretty":"BL in air"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                   (jump)","notation":"_2,_2,_8,HK","pretty":"↓,↓,↑,HK"},
                {"label":"Fatality 2              (close)","notation":"_6,_6,_2,_8,RUN","pretty":"→,→,↓,↑,RUN"},
                {"label":"Animality                   (close)","notation":"_6,_8,_8,HK","pretty":"→,↑,↑,HK"},
                {"label":"Friendship               (sweep)","notation":"_4,_6,_6,_4,LK","pretty":"←,→,→,←,LK"},
                {"label":"Babality","notation":"_2,_4,_4,_6,HP","pretty":"↓,←,←,→,HP"},
                {"label":"Stage Fatality","notation":"_6,_8,_8,LP","pretty":"→,↑,↑,LP"},
                {"label":"Brutality","notation":"HP,HP,BL,HK,HK,LK,HK,HP,HP,LP,HP","pretty":"HP,HP,BL,HK,HK,LK,HK,HP,HP,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,_8_+LP","pretty":"HP,HP,↑+LP"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,LK","pretty":"[LK/HK],HK,LK,LK"},
                {"label":"(24%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "unmasked-sub-zero",
          "name": "Unmasked Sub-Zero",
          "number": 15,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Freeze","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Ice Shower-over","notation":"_2,_6,HP","pretty":"↓,→,HP"},
                {"label":"Ice Shower-front","notation":"_2,_6,_4,HP","pretty":"↓,→,←,HP"},
                {"label":"Ice Shower-behind","notation":"_2,_4,_6,HP","pretty":"↓,←,→,HP"},
                {"label":"Ice Clone","notation":"_2,_4,LP","pretty":"↓,←,LP"},
                {"label":"Slide","notation":"_4_+LP_+BL_+LK","pretty":"←+LP+BL+LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1             (close)","notation":"BL,BL,RUN,BL,RUN","pretty":"BL,BL,RUN,BL,RUN"},
                {"label":"Fatality 2              (sweep)","notation":"_4,_4,_2,_4,RUN","pretty":"←,←,↓,←,RUN"},
                {"label":"Animality                      (close)","notation":"_6,_8,_8","pretty":"→,↑,↑"},
                {"label":"Friendship                (sweep)","notation":"LK,RUN,RUN,_8","pretty":"LK,RUN,RUN,↑"},
                {"label":"Babality","notation":"_2,_4,_4,HK","pretty":"↓,←,←,HK"},
                {"label":"Stage Fatality","notation":"_4,_2,_6,_6,HK","pretty":"←,↓,→,→,HK"},
                {"label":"Brutality","notation":"HP,LK,HK,LP,HP,HK,HK,HP,HP,LP,HP","pretty":"HP,LK,HK,LP,HP,HK,HK,HP,HP,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,_4_+HK","pretty":"HP,HP,←+HK"},
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(22%)","notation":"HP,HP,LP,_4_+HK","pretty":"HP,HP,LP,←+HK"},
                {"label":"(26%)","notation":"HP,HP,LK,HK,_4_+HK","pretty":"HP,HP,LK,HK,←+HK"},
                {"label":"(23%)","notation":"HP,HP,LP,LK,HK,_4_+HK","pretty":"HP,HP,LP,LK,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "jade",
          "name": "Jade",
          "number": 16,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Boomerang-straight","notation":"_4,_6,LP","pretty":"←,→,LP"},
                {"label":"Boomerang-returning","notation":"_4,_4,_6,LP","pretty":"←,←,→,LP"},
                {"label":"Boomerang-high","notation":"_4,_6,HP","pretty":"←,→,HP"},
                {"label":"Boomerang-low","notation":"_4,_6,LK","pretty":"←,→,LK"},
                {"label":"Projectile Invisibility","notation":"_4,_6,HK","pretty":"←,→,HK"},
                {"label":"Crunch Kick","notation":"_2,_6,LK","pretty":"↓,→,LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_8,_8,_2,_6,HP","pretty":"↑,↑,↓,→,HP"},
                {"label":"Fatality 2           (close)","notation":"RUN,RUN,RUN,BL,RUN","pretty":"RUN,RUN,RUN,BL,RUN"},
                {"label":"Animality                (close)","notation":"_6,_2,_6,_6,LK","pretty":"→,↓,→,→,LK"},
                {"label":"Friendship","notation":"_4,_2,_4,_4,HK","pretty":"←,↓,←,←,HK"},
                {"label":"Babality","notation":"_2,_2,_6,_2,HK","pretty":"↓,↓,→,↓,HK"},
                {"label":"Stage Fatality","notation":"_4,_6,_2,RUN","pretty":"←,→,↓,RUN"},
                {"label":"Brutality","notation":"HP,LK,HP,LP,HK,HK,LK,BL,BL,HP,HK","pretty":"HP,LK,HP,LP,HK,HK,LK,BL,BL,HP,HK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(22%)","notation":"HP,HP,_2_+LP,_2_+HP","pretty":"HP,HP,↓+LP,↓+HP"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(25%)","notation":"HP,HP,_2_+LP,LK,HK,_4_+LK,_4_+HK","pretty":"HP,HP,↓+LP,LK,HK,←+LK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "liu-kang",
          "name": "Liu Kang",
          "number": 17,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fireball-high (also in air)","notation":"_6,_6,HP","pretty":"→,→,HP"},
                {"label":"Fireball-low","notation":"_6,_6,LP","pretty":"→,→,LP"},
                {"label":"Flying Kick","notation":"_6,_6,HK","pretty":"→,→,HK"},
                {"label":"Bicycle Kick","notation":"hold LK for 3 sec","pretty":"hold LK for 3 sec"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1","notation":"_6,_6,_2,_2,LK","pretty":"→,→,↓,↓,LK"},
                {"label":"Fatality 2","notation":"_8,_2,_8,_8,BL_+RUN","pretty":"↑,↓,↑,↑,BL+RUN"},
                {"label":"Animality                      (sweep)","notation":"_2,_2,_8","pretty":"↓,↓,↑"},
                {"label":"Friendship          (sweep)","notation":"RUN,RUN,RUN,_2_+RUN","pretty":"RUN,RUN,RUN,↓+RUN"},
                {"label":"Babality","notation":"_2,_2,_2,HK","pretty":"↓,↓,↓,HK"},
                {"label":"Stage Fatality","notation":"RUN,BL,BL,LK","pretty":"RUN,BL,BL,LK"},
                {"label":"Brutality","notation":"HP,LP,HP,BL,LK,HK,LK,HK,LP,LP,HP","pretty":"HP,LP,HP,BL,LK,HK,LK,HK,LP,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(13%)","notation":"HP,HP,_4_+LP(juggle)","pretty":"HP,HP,←+LP(juggle)"},
                {"label":"(22%)","notation":"[LK/HK],LK,HK,LK","pretty":"[LK/HK],LK,HK,LK"},
                {"label":"(25%)","notation":"HP,LK,LK,HK,LK","pretty":"HP,LK,LK,HK,LK"},
                {"label":"(29%)","notation":"HP,HP,BL,LK,LK,HK,LK","pretty":"HP,HP,BL,LK,LK,HK,LK"}
              ]
            }
          ]
        },
        {
          "id": "sektor",
          "name": "Sektor",
          "number": 18,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Teleport Punch (also in air)","notation":"_6,_6,LK","pretty":"→,→,LK"},
                {"label":"Missile","notation":"_6,_6,LP","pretty":"→,→,LP"},
                {"label":"Homing Missile","notation":"_6,_2,_4,HP","pretty":"→,↓,←,HP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                (sweep)","notation":"LP,RUN,RUN,BL","pretty":"LP,RUN,RUN,BL"},
                {"label":"Fatality 2                (jump)","notation":"_6,_6,_6,_4,BL","pretty":"→,→,→,←,BL"},
                {"label":"Animality                   (close)","notation":"_6,_6,_2,_8","pretty":"→,→,↓,↑"},
                {"label":"Friendship             (far)","notation":"RUN,RUN,RUN,RUN,_2","pretty":"RUN,RUN,RUN,RUN,↓"},
                {"label":"Babality","notation":"_4,_2,_2,_2,HK","pretty":"←,↓,↓,↓,HK"},
                {"label":"Stage Fatality","notation":"RUN,RUN,RUN,_2","pretty":"RUN,RUN,RUN,↓"},
                {"label":"Brutality","notation":"HP,HP,BL,BL,HK,HK,LK,LK,LP,LP,HP","pretty":"HP,HP,BL,BL,HK,HK,LK,LK,LP,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"[LK/HK],HK","pretty":"[LK/HK],HK"},
                {"label":"(18%)","notation":"HP,HP,_2_+LP(juggle)","pretty":"HP,HP,↓+LP(juggle)"},
                {"label":"(22%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"},
                {"label":"(26%)","notation":"HP,HP,HK,HK,_4_+HK","pretty":"HP,HP,HK,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "classic-sub-zero",
          "name": "Classic Sub-Zero",
          "number": 19,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Freeze","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Ground Freeze","notation":"_2,_4,LK","pretty":"↓,←,LK"},
                {"label":"Slide","notation":"_4_+LP_+BL_+LK","pretty":"←+LP+BL+LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_2,_2,_2,_6,HP","pretty":"↓,↓,↓,→,HP"},
                {"label":"Fatality 2(INCLUDED MAME)(close)","notation":"_2,_6,_6,_6,HP","pretty":"↓,→,→,→,HP"},
                {"label":"Friendship               (close)","notation":"_2,_4,_4,_6,LK","pretty":"↓,←,←,→,LK"},
                {"label":"Babality","notation":"_2,_4,_4,HK","pretty":"↓,←,←,HK"},
                {"label":"Stage Fatality","notation":"_6,_2,_6,_6,HP","pretty":"→,↓,→,→,HP"},
                {"label":"Brutality","notation":"HP,LP,HP,BL,LK,LK,HK,HK,LP,HP,LP","pretty":"HP,LP,HP,BL,LK,LK,HK,HK,LP,HP,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"LK,_4_+HK,_6_+LK","pretty":"LK,←+HK,→+LK"},
                {"label":"(22%)","notation":"HP,HP,_2_+LP,_2_+HP(juggle)","pretty":"HP,HP,↓+LP,↓+HP(juggle)"},
                {"label":"(26%)","notation":"HP,HP,LK,_4_+HK,_6_+LK","pretty":"HP,HP,LK,←+HK,→+LK"}
              ]
            }
          ]
        },
        {
          "id": "cyrax",
          "name": "Cyrax",
          "number": 20,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Grenade-close","notation":"HOLD LK _4,_4,HK","pretty":"HOLD LK ←,←,HK"},
                {"label":"Grenade-far","notation":"HOLD LK _6,_6,HK","pretty":"HOLD LK →,→,HK"},
                {"label":"Net","notation":"_4,_4,LK","pretty":"←,←,LK"},
                {"label":"Teleport","notation":"_6,_2,BL","pretty":"→,↓,BL"},
                {"label":"Air Throw                foe in air","notation":"_2,_6,BL,LP","pretty":"↓,→,BL,LP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1","notation":"_2,_2,_8,_2,HP","pretty":"↓,↓,↑,↓,HP"},
                {"label":"Fatality 2              (close)","notation":"_2,_2,_6,_8,RUN","pretty":"↓,↓,→,↑,RUN"},
                {"label":"Animality                   (close)","notation":"_8,_8,_2,_2","pretty":"↑,↑,↓,↓"},
                {"label":"Friendship","notation":"RUN,RUN,RUN,_8","pretty":"RUN,RUN,RUN,↑"},
                {"label":"Babality","notation":"_6,_6,_4,HP","pretty":"→,→,←,HP"},
                {"label":"Stage Fatality","notation":"RUN,BL,RUN","pretty":"RUN,BL,RUN"},
                {"label":"Brutality","notation":"HP,HK,HP,HK,HK,HP,HK,HP,HK,LK,LP","pretty":"HP,HK,HP,HK,HK,HP,HK,HP,HK,LK,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,LP","pretty":"HP,HP,LP"},
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(30%)","notation":"HP,HP,HK,HP,HK,_4_+HK","pretty":"HP,HP,HK,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "shang-tsung",
          "name": "Shang Tsung",
          "number": 21,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Flaming Skull-single","notation":"_4,_4,HP","pretty":"←,←,HP"},
                {"label":"Flaming Skull-double","notation":"_4,_4,_6,HP","pretty":"←,←,→,HP"},
                {"label":"Flaming Skull-triple","notation":"_4,_4,_6,_6,HP","pretty":"←,←,→,→,HP"},
                {"label":"Volcanic Eruption","notation":"_6,_4,_4,LK","pretty":"→,←,←,LK"}
              ]
            },
            {
              "name": "Morphs",
              "moves": [
                {"label":"Cyrax","notation":"BL,BL,BL","pretty":"BL,BL,BL"},
                {"label":"Jade","notation":"_6,_6,_2,_2,BL","pretty":"→,→,↓,↓,BL"},
                {"label":"Jax","notation":"_6,_6,_2,LP","pretty":"→,→,↓,LP"},
                {"label":"Kabal","notation":"LP,BL,HK","pretty":"LP,BL,HK"},
                {"label":"Kano","notation":"_4,_6,BL","pretty":"←,→,BL"},
                {"label":"Kitana","notation":"_6,_2,_6,RUN","pretty":"→,↓,→,RUN"},
                {"label":"Kung Lao","notation":"RUN,RUN,BL,RUN","pretty":"RUN,RUN,BL,RUN"},
                {"label":"Liu Kang","notation":"_4,_8,_6,_2","pretty":"←,↑,→,↓"},
                {"label":"Nightwolf","notation":"_8,_8,_8","pretty":"↑,↑,↑"},
                {"label":"Reptile","notation":"RUN,BL,BL,HK","pretty":"RUN,BL,BL,HK"},
                {"label":"Scorpion","notation":"_2,_2,_6,LP","pretty":"↓,↓,→,LP"},
                {"label":"Sektor","notation":"_2,_6,_4,RUN","pretty":"↓,→,←,RUN"},
                {"label":"Sheeva","notation":"_6,_2,_6,LK or hold LK _6,_2,_6","pretty":"→,↓,→,LK or hold LK →,↓,→"},
                {"label":"Sindel","notation":"_4,_2,_4,LK","pretty":"←,↓,←,LK"},
                {"label":"Sonya","notation":"_2_+LP_+BL_+RUN","pretty":"↓+LP+BL+RUN"},
                {"label":"Stryker","notation":"_6,_6,_6,HK","pretty":"→,→,→,HK"},
                {"label":"Unmasked Sub-Zero","notation":"_6,_2,_6,HP","pretty":"→,↓,→,HP"},
                {"label":"Mileena","notation":"RUN,BL,HK","pretty":"RUN,BL,HK"},
                {"label":"Ermac","notation":"_2,_2,_8","pretty":"↓,↓,↑"},
                {"label":"Classic Sub-Zero","notation":"BL,BL,RUN,RUN","pretty":"BL,BL,RUN,RUN"},
                {"label":"Human Smoke","notation":"BL,RUN,LK","pretty":"BL,RUN,LK"},
                {"label":"Robot Smoke","notation":"_4,_4,_2,LK","pretty":"←,←,↓,LK"},
                {"label":"Noob Saibot","notation":"_6,_2,_2,_4,HK","pretty":"→,↓,↓,←,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1          (close)","notation":"hold LP _2,_6,_6,_2","pretty":"hold LP ↓,→,→,↓"},
                {"label":"Fatality 2        (close)","notation":"hold LP RUN,BL,RUN,BL","pretty":"hold LP RUN,BL,RUN,BL"},
                {"label":"Animality           (sweep)","notation":"hold HP RUN,RUN,RUN","pretty":"hold HP RUN,RUN,RUN"},
                {"label":"Friendship","notation":"LK,RUN,RUN,_2","pretty":"LK,RUN,RUN,↓"},
                {"label":"Babality","notation":"RUN,RUN,RUN,LK","pretty":"RUN,RUN,RUN,LK"},
                {"label":"Stage Fatality","notation":"_8,_8,_4,LP","pretty":"↑,↑,←,LP"},
                {"label":"Brutality:","notation":"HP,BL,BL,BL,LK,HP,LP,LP,BL,BL,BL","pretty":"HP,BL,BL,BL,LK,HP,LP,LP,BL,BL,BL"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(22%)","notation":"HP,HP,LP,_4_+HK","pretty":"HP,HP,LP,←+HK"},
                {"label":"(27%)","notation":"[LK/HK],HP,HP,LP,_4_+HK","pretty":"[LK/HK],HP,HP,LP,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "mileena",
          "name": "Mileena",
          "number": 22,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Ground Roll","notation":"_4,_4,_2,HK","pretty":"←,←,↓,HK"},
                {"label":"Sai Throw (also in air)","notation":"hold HP for 2 sec","pretty":"hold HP for 2 sec"},
                {"label":"Teleport Kick","notation":"_6,_6,LK","pretty":"→,→,LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                 (far)","notation":"_4,_4,_4,_6,LK","pretty":"←,←,←,→,LK"},
                {"label":"Fatality 2               (close)","notation":"_2,_6,_2,_6,LP","pretty":"↓,→,↓,→,LP"},
                {"label":"Animality                (close)","notation":"_6,_2,_2,_6,HK","pretty":"→,↓,↓,→,HK"},
                {"label":"Friendship","notation":"_2,_2,_4,_6,HP","pretty":"↓,↓,←,→,HP"},
                {"label":"Babality","notation":"_2,_2,_6,_6,HP","pretty":"↓,↓,→,→,HP"},
                {"label":"Stage Fatality","notation":"_2,_2,_2,LP","pretty":"↓,↓,↓,LP"},
                {"label":"Brutality","notation":"HP,LP,LP,HP,BL,HK,LK,HK,BL,HP,LP","pretty":"HP,LP,LP,HP,BL,HK,LK,HK,BL,HP,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(16%)","notation":"[LK/HK],HK,_2,_6,LK(juggle)","pretty":"[LK/HK],HK,↓,→,LK(juggle)"},
                {"label":"(22%)","notation":"HP,HP,_8_+LP,_2_+LP","pretty":"HP,HP,↑+LP,↓+LP"},
                {"label":"(23%)","notation":"[LK/HK],HK,_8_+LK,_8_+HK","pretty":"[LK/HK],HK,↑+LK,↑+HK"},
                {"label":"(24%)","notation":"HP,HP,HK,HK,_2,_6,LK(juggle)","pretty":"HP,HP,HK,HK,↓,→,LK(juggle)"},
                {"label":"(30%)","notation":"HP,HP,HK,HK,_8_+LK,_8_+HK","pretty":"HP,HP,HK,HK,↑+LK,↑+HK"}
              ]
            }
          ]
        },
        {
          "id": "sheeva",
          "name": "Sheeva",
          "number": 23,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Teleport Stomp","notation":"_2,_8","pretty":"↓,↑"},
                {"label":"Ground Stomp","notation":"_4,_2,_4,HK","pretty":"←,↓,←,HK"},
                {"label":"Fireball","notation":"_2,_6,HP","pretty":"↓,→,HP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_6,_2,_2,_6,LP","pretty":"→,↓,↓,→,LP"},
                {"label":"Fatality 2             (close)","notation":"hold HK _4,_6,_6","pretty":"hold HK ←,→,→"},
                {"label":"Animality               (close)","notation":"RUN,BL,BL,BL,BL","pretty":"RUN,BL,BL,BL,BL"},
                {"label":"Friendship              (sweep)","notation":"_6,_6,_2,_6, HP","pretty":"→,→,↓,→, HP"},
                {"label":"Babality","notation":"_2,_2,_2,_4,HK","pretty":"↓,↓,↓,←,HK"},
                {"label":"Stage Fatality","notation":"_2,_6,_2,_6,LP","pretty":"↓,→,↓,→,LP"},
                {"label":"Brutality","notation":"HP,LP,BL,LK,HK,BL,HK,LK,BL,LP,HP","pretty":"HP,LP,BL,LK,HK,BL,HK,LK,BL,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(25%)","notation":"HP,HP,LP,_6_+HP(juggle)","pretty":"HP,HP,LP,→+HP(juggle)"},
                {"label":"(27%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(42%)","notation":"HP,HP,LP,HK,HK,LK,_4_+HK","pretty":"HP,HP,LP,HK,HK,LK,←+HK"}
              ]
            }
          ]
        }
      ]
    },
    "umk3tm60": {
      "id": "umk3tm60",
      "fighters": [
        {
          "id": "kabal",
          "name": "Kabal",
          "number": 1,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Dash","notation":"_4,_6,LK","pretty":"←,→,LK"},
                {"label":"Eye Spark (also in air)","notation":"_4,_4,HP","pretty":"←,←,HP"},
                {"label":"Ground Saw","notation":"_4,_4,_4,RUN","pretty":"←,←,←,RUN"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (sweep)","notation":"_2,_2,_4,_6,BL","pretty":"↓,↓,←,→,BL"},
                {"label":"Fatality 2              (close)","notation":"RUN,BL,BL,BL,HK","pretty":"RUN,BL,BL,BL,HK"},
                {"label":"Animality           (close)","notation":"hold HP _6,_6,_2,_6","pretty":"hold HP →,→,↓,→"},
                {"label":"Friendship            (sweep)","notation":"RUN,LK,RUN,RUN,_8","pretty":"RUN,LK,RUN,RUN,↑"},
                {"label":"Babality","notation":"RUN,RUN,LK","pretty":"RUN,RUN,LK"},
                {"label":"Stage Fatality","notation":"BL,BL,HK","pretty":"BL,BL,HK"},
                {"label":"Brutality","notation":"HP,BL,LK,LK,LK,HK,LP,LP,LP,HP,LP","pretty":"HP,BL,LK,LK,LK,HK,LP,LP,LP,HP,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(13%)","notation":"HP,HP,_2_+HP(juggle)","pretty":"HP,HP,↓+HP(juggle)"},
                {"label":"(18%)","notation":"[LK/HK],LK,_4_+HK","pretty":"[LK/HK],LK,←+HK"},
                {"label":"(17%)","notation":"HP,HP,_2_+LP,_2_+HP","pretty":"HP,HP,↓+LP,↓+HP"},
                {"label":"(19%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"},
                {"label":"(24%)","notation":"[LK/HK],LK,HK,_4_+HK","pretty":"[LK/HK],LK,HK,←+HK"},
                {"label":"(15%)","notation":"[LK/HK],LK,HP,HP,_2_+HP(juggle)","pretty":"[LK/HK],LK,HP,HP,↓+HP(juggle)"},
                {"label":"(17%)","notation":"[LK/HK],LK,HP,HP,_2_+LP,_2_+HP","pretty":"[LK/HK],LK,HP,HP,↓+LP,↓+HP"},
                {"label":"(18%)","notation":"[LK/HK],LK,HP,HP,HK,_4_+HK","pretty":"[LK/HK],LK,HP,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "human-smoke",
          "name": "Human Smoke",
          "number": 2,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Spear","notation":"_4,_4,LP","pretty":"←,←,LP"},
                {"label":"Teleport Punch","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Air Throw","notation":"BL in air","pretty":"BL in air"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1            (close)","notation":"RUN,BL,RUN,RUN,HK","pretty":"RUN,BL,RUN,RUN,HK"},
                {"label":"Babality","notation":"_2,_4,_4,_6,HP","pretty":"↓,←,←,→,HP"},
                {"label":"Stage Fatality","notation":"_6,_8,_8,LP","pretty":"→,↑,↑,LP"},
                {"label":"Brutality","notation":"HP,HP,BL,LK,HK,HP,HK,HP,HK,LP,LK","pretty":"HP,HP,BL,LK,HK,HP,HK,HP,HK,LP,LK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"[LK/HK],LP(juggle)","pretty":"[LK/HK],LP(juggle)"},
                {"label":"(18%)","notation":"HP,HP,_8_+LP","pretty":"HP,HP,↑+LP"},
                {"label":"(19%)","notation":"[LK/HK],_2_+LP,_2_+HP(juggle)","pretty":"[LK/HK],↓+LP,↓+HP(juggle)"},
                {"label":"(23%)","notation":"HK,HK,LK,_4_+HK","pretty":"HK,HK,LK,←+HK"},
                {"label":"(24%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "kung-lao",
          "name": "Kung Lao",
          "number": 3,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Hat Throw","notation":"_4,_6,LP","pretty":"←,→,LP"},
                {"label":"Teleport","notation":"_2,_8","pretty":"↓,↑"},
                {"label":"Torpedo Kick (in air)","notation":"_2_+HK in air","pretty":"↓+HK in air"},
                {"label":"Spin Attack (tap RUN to spin)","notation":"_6,_2,_6,RUN","pretty":"→,↓,→,RUN"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1","notation":"RUN,BL,RUN,BL,_2","pretty":"RUN,BL,RUN,BL,↓"},
                {"label":"Fatality 2               (sweep)","notation":"_6,_6,_4,_2,HP","pretty":"→,→,←,↓,HP"},
                {"label":"Animality            (close)","notation":"RUN,RUN,RUN,RUN,BL","pretty":"RUN,RUN,RUN,RUN,BL"},
                {"label":"Friendship                (sweep)","notation":"RUN,LP,RUN,LK","pretty":"RUN,LP,RUN,LK"},
                {"label":"Babality","notation":"_2,_6,_6,HP","pretty":"↓,→,→,HP"},
                {"label":"Stage Fatality","notation":"_2,_2,_6,_6,LK","pretty":"↓,↓,→,→,LK"},
                {"label":"Brutality","notation":"HP,LP,LK,HK,BL,HP,LP,LK,HK,BL,HP","pretty":"HP,LP,LK,HK,BL,HP,LP,LK,HK,BL,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,LK,_4_+HK","pretty":"HP,LK,←+HK"},
                {"label":"(19%)","notation":"[LK/HK],LK,HK","pretty":"[LK/HK],LK,HK"},
                {"label":"(34%)","notation":"HP,LP,HP,LP,LK,LK,HK","pretty":"HP,LP,HP,LP,LK,LK,HK"}
              ]
            }
          ]
        },
        {
          "id": "kano",
          "name": "Kano",
          "number": 4,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Knife Throw","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Knife Uppercut","notation":"_2,_6,HP","pretty":"↓,→,HP"},
                {"label":"Cannonball-straight","notation":"hold LK for 3 sec","pretty":"hold LK for 3 sec"},
                {"label":"Cannonball-upward","notation":"_6,_2,_6,HK","pretty":"→,↓,→,HK"},
                {"label":"Neck Choke","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Air Throw","notation":"BL in air","pretty":"BL in air"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1          (close)","notation":"hold LP _6,_2,_2,_6","pretty":"hold LP →,↓,↓,→"},
                {"label":"Fatality 2                  (sweep)","notation":"LP,BL,BL,HK","pretty":"LP,BL,BL,HK"},
                {"label":"Animality              (close)","notation":"hold HP BL,BL,BL","pretty":"hold HP BL,BL,BL"},
                {"label":"Friendship                (sweep)","notation":"LK,RUN,RUN,HK","pretty":"LK,RUN,RUN,HK"},
                {"label":"Babality","notation":"_6,_6,_2,_2,LK","pretty":"→,→,↓,↓,LK"},
                {"label":"Stage Fatality","notation":"_8,_8,_4,LK","pretty":"↑,↑,←,LK"},
                {"label":"Brutality","notation":"HP,LP,BL,LP,HP,BL,HK,LK,BL,HK,LK","pretty":"HP,LP,BL,LP,HP,BL,HK,LK,BL,HK,LK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"[LK/HK],LP(juggle)","pretty":"[LK/HK],LP(juggle)"},
                {"label":"(18%)","notation":"HP,HP,LP(juggle)","pretty":"HP,HP,LP(juggle)"},
                {"label":"(19%)","notation":"[LK/HK],_2_+LP,_2_+HP(juggle)","pretty":"[LK/HK],↓+LP,↓+HP(juggle)"},
                {"label":"(22%)","notation":"HP,HP,_2_+LP,_2_+HP(juggle)","pretty":"HP,HP,↓+LP,↓+HP(juggle)"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(26%)","notation":"HP,HP,HK,LK,_4_+HK","pretty":"HP,HP,HK,LK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "reptile",
          "name": "Reptile",
          "number": 5,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Acid Spit","notation":"_6,_6,HP","pretty":"→,→,HP"},
                {"label":"Forceball-slow","notation":"_4,_4,HP_+LP","pretty":"←,←,HP+LP"},
                {"label":"Forceball-fast","notation":"_6,_6,HP_+LP","pretty":"→,→,HP+LP"},
                {"label":"Slide","notation":"_4_+LP_+BL_+LK","pretty":"←+LP+BL+LK"},
                {"label":"Invisibility","notation":"_8,_2,HK","pretty":"↑,↓,HK"},
                {"label":"Super Elbow","notation":"_4,_6,LK","pretty":"←,→,LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                   (jump)","notation":"_4,_6,_2,BL","pretty":"←,→,↓,BL"},
                {"label":"Fatality 2               (sweep)","notation":"_6,_6,_8,_8,HK","pretty":"→,→,↑,↑,HK"},
                {"label":"Animality                (close)","notation":"_2,_2,_2,_8,HK","pretty":"↓,↓,↓,↑,HK"},
                {"label":"Friendship               (close)","notation":"_2,_6,_6,_4,HK","pretty":"↓,→,→,←,HK"},
                {"label":"Babality","notation":"_6,_6,_4,_2,LK","pretty":"→,→,←,↓,LK"},
                {"label":"Stage Fatality","notation":"BL,RUN,BL,BL","pretty":"BL,RUN,BL,BL"},
                {"label":"Brutality","notation":"HP,BL,HK,HK,BL,HP,LP,LK,LK,BL,LP","pretty":"HP,BL,HK,HK,BL,HP,LP,LK,LK,BL,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,_2_+LP(juggle)","pretty":"HP,HP,↓+LP(juggle)"},
                {"label":"(21%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(24%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "ermac",
          "name": "Ermac",
          "number": 6,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fire Teleport Punch","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Green Sphere","notation":"_2,_4,LP","pretty":"↓,←,LP"},
                {"label":"Soul Slam","notation":"_4,_2,_4,HK","pretty":"←,↓,←,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1            (sweep)","notation":"_2,_8,_2,_2,_2,BL","pretty":"↓,↑,↓,↓,↓,BL"},
                {"label":"Fatality 2            (close)","notation":"RUN,BL,RUN,RUN,HK","pretty":"RUN,BL,RUN,RUN,HK"},
                {"label":"Stage Fatality","notation":"RUN,RUN,RUN,RUN,LK","pretty":"RUN,RUN,RUN,RUN,LK"},
                {"label":"Babality","notation":"_2,_2,_4,_4,HP","pretty":"↓,↓,←,←,HP"},
                {"label":"Brutality","notation":"HP,HP,LP,BL,HK,LK,BL,HP,LP,LK,HK","pretty":"HP,HP,LP,BL,HK,LK,BL,HP,LP,LK,HK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"[LK/HK],LP(juggle)","pretty":"[LK/HK],LP(juggle)"},
                {"label":"(19%)","notation":"HP,HP,_4_+LP,_2,_6,LP(juggle)","pretty":"HP,HP,←+LP,↓,→,LP(juggle)"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(24%)","notation":"HP,HP,_4_+LP,HK,LK","pretty":"HP,HP,←+LP,HK,LK"}
              ]
            }
          ]
        },
        {
          "id": "nightwolf",
          "name": "Nightwolf",
          "number": 7,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Arrow","notation":"_2,_4,LP","pretty":"↓,←,LP"},
                {"label":"Hatchet Uppercut","notation":"_2,_6,HP","pretty":"↓,→,HP"},
                {"label":"Shadow Charge","notation":"_6,_6,LK","pretty":"→,→,LK"},
                {"label":"Reflect Shield","notation":"_4,_4,_4,HK","pretty":"←,←,←,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_8,_8,_4,_6,BL","pretty":"↑,↑,←,→,BL"},
                {"label":"Fatality 2                  (sweep)","notation":"_4,_4,_2,HP","pretty":"←,←,↓,HP"},
                {"label":"Animality                   (close)","notation":"_6,_6,_2,_2","pretty":"→,→,↓,↓"},
                {"label":"Friendship               (sweep)","notation":"RUN,RUN,RUN,_2","pretty":"RUN,RUN,RUN,↓"},
                {"label":"Babality","notation":"_6,_4,_6,_4,LP","pretty":"→,←,→,←,LP"},
                {"label":"Stage Fatality","notation":"RUN,RUN,BL","pretty":"RUN,RUN,BL"},
                {"label":"Brutality","notation":"HP,HP,HK,LK,LK,BL,BL,LP,LP,HP,HK","pretty":"HP,HP,HK,LK,LK,BL,BL,LP,LP,HP,HK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(22%)","notation":"HP,HP,LP,HK","pretty":"HP,HP,LP,HK"},
                {"label":"(23%)","notation":"HP,HP,LP,Hatchet Uppercut(juggle)","pretty":"HP,HP,LP,Hatchet Uppercut(juggle)"},
                {"label":"(27%)","notation":"[LK/HK],HP,HP,LP,HK","pretty":"[LK/HK],HP,HP,LP,HK"},
                {"label":"(28%)","notation":"[LK/HK],HP,HP,LP,Hatchet Uppercut(juggle)","pretty":"[LK/HK],HP,HP,LP,Hatchet Uppercut(juggle)"}
              ]
            }
          ]
        },
        {
          "id": "robot-smoke",
          "name": "Robot Smoke",
          "number": 8,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Harpoon","notation":"_4,_4,LP","pretty":"←,←,LP"},
                {"label":"Teleport Punch (also in  air)","notation":"_6,_6,LK","pretty":"→,→,LK"},
                {"label":"Invisibility","notation":"_8,_8,RUN","pretty":"↑,↑,RUN"},
                {"label":"Air Throw","notation":"BL in air","pretty":"BL in air"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                    (far)","notation":"_8,_8,_6,_2","pretty":"↑,↑,→,↓"},
                {"label":"Fatality 2     (sweep)","notation":"hold BL_+RUN _2,_2,_6,_8","pretty":"hold BL+RUN ↓,↓,→,↑"},
                {"label":"Animality                    (jump)","notation":"_2,_6,_6,BL","pretty":"↓,→,→,BL"},
                {"label":"Frienship                (sweep)","notation":"RUN,RUN,RUN,HK","pretty":"RUN,RUN,RUN,HK"},
                {"label":"Babality","notation":"_2,_2,_4,_4,HK","pretty":"↓,↓,←,←,HK"},
                {"label":"Stage Fatality","notation":"_6,_6,_2,LK","pretty":"→,→,↓,LK"},
                {"label":"Brutality","notation":"HP,LK,LK,HK,BL,BL,LP,LP,HP,BL,BL","pretty":"HP,LK,LK,HK,BL,BL,LP,LP,HP,BL,BL"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,LP","pretty":"HP,HP,LP"},
                {"label":"(18%)","notation":"HP,HP,HK","pretty":"HP,HP,HK"},
                {"label":"(19%)","notation":"[LK/HK],HK,LP","pretty":"[LK/HK],HK,LP"},
                {"label":"(26%)","notation":"HP,HP,LK,HK,LP","pretty":"HP,HP,LK,HK,LP"}
              ]
            }
          ]
        },
        {
          "id": "sindel",
          "name": "Sindel",
          "number": 9,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fireball-ground","notation":"_6,_6,LP","pretty":"→,→,LP"},
                {"label":"Fireball-air","notation":"_2,_6,LK in air","pretty":"↓,→,LK in air"},
                {"label":"Fly","notation":"_4,_4,_6,HK","pretty":"←,←,→,HK"},
                {"label":"Scream","notation":"_6,_6,_6,HP","pretty":"→,→,→,HP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1            (sweep)","notation":"RUN,RUN,BL,RUN,BL","pretty":"RUN,RUN,BL,RUN,BL"},
                {"label":"Fatality 2            (close)","notation":"RUN,BL,BL,RUN_+BL","pretty":"RUN,BL,BL,RUN+BL"},
                {"label":"Animality","notation":"_6,_6,_8,HP","pretty":"→,→,↑,HP"},
                {"label":"Friendship","notation":"RUN,RUN,RUN,RUN,RUN,_8","pretty":"RUN,RUN,RUN,RUN,RUN,↑"},
                {"label":"Babality","notation":"RUN,RUN,RUN,_8","pretty":"RUN,RUN,RUN,↑"},
                {"label":"Stage Fatality","notation":"_2,_2,_2,LP","pretty":"↓,↓,↓,LP"},
                {"label":"Brutality","notation":"HP,BL,LK,BL,LK,HK,BL,HK,LK,BL,LP","pretty":"HP,BL,LK,BL,LK,HK,BL,HK,LK,BL,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"HP,HP,_2_+HP(juggle)","pretty":"HP,HP,↓+HP(juggle)"},
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(25%)","notation":"HP,HP,LP,HK","pretty":"HP,HP,LP,HK"},
                {"label":"(27%)","notation":"[LK/HK],HP,HP,_2_+HP(juggle)","pretty":"[LK/HK],HP,HP,↓+HP(juggle)"},
                {"label":"(33%)","notation":"[LK/HK],HP,HP,LP,HK","pretty":"[LK/HK],HP,HP,LP,HK"}
              ]
            }
          ]
        },
        {
          "id": "jax",
          "name": "Jax",
          "number": 10,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Missile-single","notation":"_4,_6,HP","pretty":"←,→,HP"},
                {"label":"Missile-double","notation":"_6,_6,_4,_4,HP","pretty":"→,→,←,←,HP"},
                {"label":"Gotcha Grab","notation":"_6,_6_+LP, LPx3","pretty":"→,→+LP, LPx3"},
                {"label":"Backbreaker (in air)","notation":"BL in air","pretty":"BL in air"},
                {"label":"Quad Slam","notation":"throw, HPx4","pretty":"throw, HPx4"},
                {"label":"Ground Smash","notation":"hold LK for 3 sec","pretty":"hold LK for 3 sec"},
                {"label":"Dashing Pound","notation":"_6,_6,HK","pretty":"→,→,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1          (close)","notation":"hold BL _8,_2,_6,_8","pretty":"hold BL ↑,↓,→,↑"},
                {"label":"Fatality 2              (far)","notation":"RUN,BL,RUN,RUN,LK","pretty":"RUN,BL,RUN,RUN,LK"},
                {"label":"Animality           (close)","notation":"hold LP _6,_6,_2,_6","pretty":"hold LP →,→,↓,→"},
                {"label":"Friendship                (sweep)","notation":"LK,RUN,RUN,LK","pretty":"LK,RUN,RUN,LK"},
                {"label":"Babality","notation":"_2,_2,_2,LK","pretty":"↓,↓,↓,LK"},
                {"label":"Stage Fatality","notation":"_2,_6,_2,LP","pretty":"↓,→,↓,LP"},
                {"label":"Brutality","notation":"HP,HP,HP,BL,LP,HP,HP,HP,BL,LP,HP","pretty":"HP,HP,HP,BL,LP,HP,HP,HP,BL,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(24%)","notation":"HP,HP,BL,LP,_4_+HP","pretty":"HP,HP,BL,LP,←+HP"},
                {"label":"(33%)","notation":"[LK/HK],HK,_2_+HP,HP,BL,LP,_4_+HP","pretty":"[LK/HK],HK,↓+HP,HP,BL,LP,←+HP"}
              ]
            }
          ]
        },
        {
          "id": "sonya",
          "name": "Sonya",
          "number": 11,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Force Wave","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Leg Grab","notation":"_2_+LP_+BL","pretty":"↓+LP+BL"},
                {"label":"Square Wave","notation":"_6,_4,HP","pretty":"→,←,HP"},
                {"label":"Bicycle Kick","notation":"_4,_4,_2,HK","pretty":"←,←,↓,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1       (far)","notation":"hold BL_+RUN _8,_8,_4,_2","pretty":"hold BL+RUN ↑,↑,←,↓"},
                {"label":"Fatality 2","notation":"_4,_6,_2,_2,RUN","pretty":"←,→,↓,↓,RUN"},
                {"label":"Animality           (close)","notation":"hold LP _4,_6,_2,_6","pretty":"hold LP ←,→,↓,→"},
                {"label":"Friendship              (sweep)","notation":"_4,_6,_4,_2,RUN","pretty":"←,→,←,↓,RUN"},
                {"label":"Babality","notation":"_2,_2,_6,LK","pretty":"↓,↓,→,LK"},
                {"label":"Stage Fatality","notation":"_6,_6,_2,HP","pretty":"→,→,↓,HP"},
                {"label":"Brutality","notation":"HP,LK,BL,HP,LK,BL,HP,LP,BL,HK,LK","pretty":"HP,LK,BL,HP,LK,BL,HP,LP,BL,HK,LK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,_8_+LP(juggle)","pretty":"HP,HP,↑+LP(juggle)"},
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(22%)","notation":"HP,HP,LP,_4_+HP","pretty":"HP,HP,LP,←+HP"},
                {"label":"(27%)","notation":"[LK/HK],HP,HP,LP,_4_+HP","pretty":"[LK/HK],HP,HP,LP,←+HP"},
                {"label":"(27%)","notation":"[LK/HK],HK,HP,HP,_8_+LP(juggle)","pretty":"[LK/HK],HK,HP,HP,↑+LP(juggle)"},
                {"label":"(31%)","notation":"[LK/HK],HK,HP,HP,LP,_4_+HP","pretty":"[LK/HK],HK,HP,HP,LP,←+HP"}
              ]
            }
          ]
        },
        {
          "id": "kitana",
          "name": "Kitana",
          "number": 12,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fan Lift","notation":"_4,_4,_4,HP","pretty":"←,←,←,HP"},
                {"label":"Fan Throw (also in air)","notation":"_6,_6,HP_+LP","pretty":"→,→,HP+LP"},
                {"label":"Square Wave","notation":"_2,_4,HP","pretty":"↓,←,HP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1             (close)","notation":"RUN,RUN,BL,BL,LK","pretty":"RUN,RUN,BL,BL,LK"},
                {"label":"Fatality 2               (close)","notation":"_4,_2,_6,_6,HK","pretty":"←,↓,→,→,HK"},
                {"label":"Animality               (sweep)","notation":"_2,_2,_2,_2,RUN","pretty":"↓,↓,↓,↓,RUN"},
                {"label":"Friendship","notation":"_2,_4,_6,_6,LP","pretty":"↓,←,→,→,LP"},
                {"label":"Babality","notation":"_6,_6,_2,_6,HK","pretty":"→,→,↓,→,HK"},
                {"label":"Stage Fatality","notation":"_6,_2,_2,LK","pretty":"→,↓,↓,LK"},
                {"label":"Brutality:","notation":"HP,HP,BL,HK,BL,LK,BL,LP,BL,HP,BL","pretty":"HP,HP,BL,HK,BL,LK,BL,LP,BL,HP,BL"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(25%)","notation":"HP,HP,_4_+LP,_6_+HP","pretty":"HP,HP,←+LP,→+HP"}
              ]
            }
          ]
        },
        {
          "id": "stryker",
          "name": "Stryker",
          "number": 13,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Grenade-high","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Grenade-low","notation":"_2,_4,LP","pretty":"↓,←,LP"},
                {"label":"Gun Shot","notation":"_4,_6,HP","pretty":"←,→,HP"},
                {"label":"Baton Sweep","notation":"_6,_4,LP","pretty":"→,←,LP"},
                {"label":"Baton Toss","notation":"_6,_6,HK","pretty":"→,→,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_2,_6,_2,_6,BL","pretty":"↓,→,↓,→,BL"},
                {"label":"Fatality 2                    (far)","notation":"_6,_6,_6,LK","pretty":"→,→,→,LK"},
                {"label":"Animality                (sweep)","notation":"RUN,RUN,RUN,BL","pretty":"RUN,RUN,RUN,BL"},
                {"label":"Friendship","notation":"LP,RUN,RUN,LP","pretty":"LP,RUN,RUN,LP"},
                {"label":"Babality","notation":"_2,_6,_6,_4,HP","pretty":"↓,→,→,←,HP"},
                {"label":"Stage Fatality","notation":"_6,_8,_8,HK","pretty":"→,↑,↑,HK"},
                {"label":"Brutality:","notation":"HP,LP,HK,LK,HP,LP,LK,HK,HP,LK,LK","pretty":"HP,LP,HK,LK,HP,LP,LK,HK,HP,LK,LK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,LP(juggle)","pretty":"HP,HP,LP(juggle)"},
                {"label":"(19%)","notation":"[LK/HK],LK,_4_+HK","pretty":"[LK/HK],LK,←+HK"},
                {"label":"(23%)","notation":"[LK/HK],HP,HP,LP(juggle)","pretty":"[LK/HK],HP,HP,LP(juggle)"},
                {"label":"(23%)","notation":"[LK/HK],LK,_4_+LP,_4_+HK","pretty":"[LK/HK],LK,←+LP,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "scorpion",
          "name": "Scorpion",
          "number": 14,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Spear","notation":"_4,_4,LP","pretty":"←,←,LP"},
                {"label":"Teleport Punch","notation":"_2,_4,HP","pretty":"↓,←,HP"},
                {"label":"Air Throw","notation":"BL in air","pretty":"BL in air"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                   (jump)","notation":"_2,_2,_8,HK","pretty":"↓,↓,↑,HK"},
                {"label":"Fatality 2              (close)","notation":"_6,_6,_2,_8,RUN","pretty":"→,→,↓,↑,RUN"},
                {"label":"Animality                   (close)","notation":"_6,_8,_8,HK","pretty":"→,↑,↑,HK"},
                {"label":"Friendship               (sweep)","notation":"_4,_6,_6,_4,LK","pretty":"←,→,→,←,LK"},
                {"label":"Babality","notation":"_2,_4,_4,_6,HP","pretty":"↓,←,←,→,HP"},
                {"label":"Stage Fatality","notation":"_6,_8,_8,LP","pretty":"→,↑,↑,LP"},
                {"label":"Brutality","notation":"HP,HP,BL,HK,HK,LK,HK,HP,HP,LP,HP","pretty":"HP,HP,BL,HK,HK,LK,HK,HP,HP,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,_8_+LP","pretty":"HP,HP,↑+LP"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,LK","pretty":"[LK/HK],HK,LK,LK"},
                {"label":"(24%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "unmasked-sub-zero",
          "name": "Unmasked Sub-Zero",
          "number": 15,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Freeze","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Ice Shower-over","notation":"_2,_6,HP","pretty":"↓,→,HP"},
                {"label":"Ice Shower-front","notation":"_2,_6,_4,HP","pretty":"↓,→,←,HP"},
                {"label":"Ice Shower-behind","notation":"_2,_4,_6,HP","pretty":"↓,←,→,HP"},
                {"label":"Ice Clone","notation":"_2,_4,LP","pretty":"↓,←,LP"},
                {"label":"Slide","notation":"_4_+LP_+BL_+LK","pretty":"←+LP+BL+LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1             (close)","notation":"BL,BL,RUN,BL,RUN","pretty":"BL,BL,RUN,BL,RUN"},
                {"label":"Fatality 2              (sweep)","notation":"_4,_4,_2,_4,RUN","pretty":"←,←,↓,←,RUN"},
                {"label":"Animality                      (close)","notation":"_6,_8,_8","pretty":"→,↑,↑"},
                {"label":"Friendship                (sweep)","notation":"LK,RUN,RUN,_8","pretty":"LK,RUN,RUN,↑"},
                {"label":"Babality","notation":"_2,_4,_4,HK","pretty":"↓,←,←,HK"},
                {"label":"Stage Fatality","notation":"_4,_2,_6,_6,HK","pretty":"←,↓,→,→,HK"},
                {"label":"Brutality","notation":"HP,LK,HK,LP,HP,HK,HK,HP,HP,LP,HP","pretty":"HP,LK,HK,LP,HP,HK,HK,HP,HP,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,_4_+HK","pretty":"HP,HP,←+HK"},
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(22%)","notation":"HP,HP,LP,_4_+HK","pretty":"HP,HP,LP,←+HK"},
                {"label":"(26%)","notation":"HP,HP,LK,HK,_4_+HK","pretty":"HP,HP,LK,HK,←+HK"},
                {"label":"(23%)","notation":"HP,HP,LP,LK,HK,_4_+HK","pretty":"HP,HP,LP,LK,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "jade",
          "name": "Jade",
          "number": 16,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Boomerang-straight","notation":"_4,_6,LP","pretty":"←,→,LP"},
                {"label":"Boomerang-returning","notation":"_4,_4,_6,LP","pretty":"←,←,→,LP"},
                {"label":"Boomerang-high","notation":"_4,_6,HP","pretty":"←,→,HP"},
                {"label":"Boomerang-low","notation":"_4,_6,LK","pretty":"←,→,LK"},
                {"label":"Projectile Invisibility","notation":"_4,_6,HK","pretty":"←,→,HK"},
                {"label":"Crunch Kick","notation":"_2,_6,LK","pretty":"↓,→,LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_8,_8,_2,_6,HP","pretty":"↑,↑,↓,→,HP"},
                {"label":"Fatality 2           (close)","notation":"RUN,RUN,RUN,BL,RUN","pretty":"RUN,RUN,RUN,BL,RUN"},
                {"label":"Animality                (close)","notation":"_6,_2,_6,_6,LK","pretty":"→,↓,→,→,LK"},
                {"label":"Friendship","notation":"_4,_2,_4,_4,HK","pretty":"←,↓,←,←,HK"},
                {"label":"Babality","notation":"_2,_2,_6,_2,HK","pretty":"↓,↓,→,↓,HK"},
                {"label":"Stage Fatality","notation":"_4,_6,_2,RUN","pretty":"←,→,↓,RUN"},
                {"label":"Brutality","notation":"HP,LK,HP,LP,HK,HK,LK,BL,BL,HP,HK","pretty":"HP,LK,HP,LP,HK,HK,LK,BL,BL,HP,HK"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(22%)","notation":"HP,HP,_2_+LP,_2_+HP","pretty":"HP,HP,↓+LP,↓+HP"},
                {"label":"(23%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(25%)","notation":"HP,HP,_2_+LP,LK,HK,_4_+LK,_4_+HK","pretty":"HP,HP,↓+LP,LK,HK,←+LK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "liu-kang",
          "name": "Liu Kang",
          "number": 17,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Fireball-high (also in air)","notation":"_6,_6,HP","pretty":"→,→,HP"},
                {"label":"Fireball-low","notation":"_6,_6,LP","pretty":"→,→,LP"},
                {"label":"Flying Kick","notation":"_6,_6,HK","pretty":"→,→,HK"},
                {"label":"Bicycle Kick","notation":"hold LK for 3 sec","pretty":"hold LK for 3 sec"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1","notation":"_6,_6,_2,_2,LK","pretty":"→,→,↓,↓,LK"},
                {"label":"Fatality 2","notation":"_8,_2,_8,_8,BL_+RUN","pretty":"↑,↓,↑,↑,BL+RUN"},
                {"label":"Animality                      (sweep)","notation":"_2,_2,_8","pretty":"↓,↓,↑"},
                {"label":"Friendship          (sweep)","notation":"RUN,RUN,RUN,_2_+RUN","pretty":"RUN,RUN,RUN,↓+RUN"},
                {"label":"Babality","notation":"_2,_2,_2,HK","pretty":"↓,↓,↓,HK"},
                {"label":"Stage Fatality","notation":"RUN,BL,BL,LK","pretty":"RUN,BL,BL,LK"},
                {"label":"Brutality","notation":"HP,LP,HP,BL,LK,HK,LK,HK,LP,LP,HP","pretty":"HP,LP,HP,BL,LK,HK,LK,HK,LP,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(13%)","notation":"HP,HP,_4_+LP(juggle)","pretty":"HP,HP,←+LP(juggle)"},
                {"label":"(22%)","notation":"[LK/HK],LK,HK,LK","pretty":"[LK/HK],LK,HK,LK"},
                {"label":"(25%)","notation":"HP,LK,LK,HK,LK","pretty":"HP,LK,LK,HK,LK"},
                {"label":"(29%)","notation":"HP,HP,BL,LK,LK,HK,LK","pretty":"HP,HP,BL,LK,LK,HK,LK"}
              ]
            }
          ]
        },
        {
          "id": "sektor",
          "name": "Sektor",
          "number": 18,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Teleport Punch (also in air)","notation":"_6,_6,LK","pretty":"→,→,LK"},
                {"label":"Missile","notation":"_6,_6,LP","pretty":"→,→,LP"},
                {"label":"Homing Missile","notation":"_6,_2,_4,HP","pretty":"→,↓,←,HP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                (sweep)","notation":"LP,RUN,RUN,BL","pretty":"LP,RUN,RUN,BL"},
                {"label":"Fatality 2                (jump)","notation":"_6,_6,_6,_4,BL","pretty":"→,→,→,←,BL"},
                {"label":"Animality                   (close)","notation":"_6,_6,_2,_8","pretty":"→,→,↓,↑"},
                {"label":"Friendship             (far)","notation":"RUN,RUN,RUN,RUN,_2","pretty":"RUN,RUN,RUN,RUN,↓"},
                {"label":"Babality","notation":"_4,_2,_2,_2,HK","pretty":"←,↓,↓,↓,HK"},
                {"label":"Stage Fatality","notation":"RUN,RUN,RUN,_2","pretty":"RUN,RUN,RUN,↓"},
                {"label":"Brutality","notation":"HP,HP,BL,BL,HK,HK,LK,LK,LP,LP,HP","pretty":"HP,HP,BL,BL,HK,HK,LK,LK,LP,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(15%)","notation":"[LK/HK],HK","pretty":"[LK/HK],HK"},
                {"label":"(18%)","notation":"HP,HP,_2_+LP(juggle)","pretty":"HP,HP,↓+LP(juggle)"},
                {"label":"(22%)","notation":"HP,HP,HK,_4_+HK","pretty":"HP,HP,HK,←+HK"},
                {"label":"(26%)","notation":"HP,HP,HK,HK,_4_+HK","pretty":"HP,HP,HK,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "classic-sub-zero",
          "name": "Classic Sub-Zero",
          "number": 19,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Freeze","notation":"_2,_6,LP","pretty":"↓,→,LP"},
                {"label":"Ground Freeze","notation":"_2,_4,LK","pretty":"↓,←,LK"},
                {"label":"Slide","notation":"_4_+LP_+BL_+LK","pretty":"←+LP+BL+LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_2,_2,_2,_6,HP","pretty":"↓,↓,↓,→,HP"},
                {"label":"Fatality 2(INCLUDED MAME)(close)","notation":"_2,_6,_6,_6,HP","pretty":"↓,→,→,→,HP"},
                {"label":"Friendship               (close)","notation":"_2,_4,_4,_6,LK","pretty":"↓,←,←,→,LK"},
                {"label":"Babality","notation":"_2,_4,_4,HK","pretty":"↓,←,←,HK"},
                {"label":"Stage Fatality","notation":"_6,_2,_6,_6,HP","pretty":"→,↓,→,→,HP"},
                {"label":"Brutality","notation":"HP,LP,HP,BL,LK,LK,HK,HK,LP,HP,LP","pretty":"HP,LP,HP,BL,LK,LK,HK,HK,LP,HP,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"LK,_4_+HK,_6_+LK","pretty":"LK,←+HK,→+LK"},
                {"label":"(22%)","notation":"HP,HP,_2_+LP,_2_+HP(juggle)","pretty":"HP,HP,↓+LP,↓+HP(juggle)"},
                {"label":"(26%)","notation":"HP,HP,LK,_4_+HK,_6_+LK","pretty":"HP,HP,LK,←+HK,→+LK"}
              ]
            }
          ]
        },
        {
          "id": "cyrax",
          "name": "Cyrax",
          "number": 20,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Grenade-close","notation":"HOLD LK _4,_4,HK","pretty":"HOLD LK ←,←,HK"},
                {"label":"Grenade-far","notation":"HOLD LK _6,_6,HK","pretty":"HOLD LK →,→,HK"},
                {"label":"Net","notation":"_4,_4,LK","pretty":"←,←,LK"},
                {"label":"Teleport","notation":"_6,_2,BL","pretty":"→,↓,BL"},
                {"label":"Air Throw                foe in air","notation":"_2,_6,BL,LP","pretty":"↓,→,BL,LP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1","notation":"_2,_2,_8,_2,HP","pretty":"↓,↓,↑,↓,HP"},
                {"label":"Fatality 2              (close)","notation":"_2,_2,_6,_8,RUN","pretty":"↓,↓,→,↑,RUN"},
                {"label":"Animality                   (close)","notation":"_8,_8,_2,_2","pretty":"↑,↑,↓,↓"},
                {"label":"Friendship","notation":"RUN,RUN,RUN,_8","pretty":"RUN,RUN,RUN,↑"},
                {"label":"Babality","notation":"_6,_6,_4,HP","pretty":"→,→,←,HP"},
                {"label":"Stage Fatality","notation":"RUN,BL,RUN","pretty":"RUN,BL,RUN"},
                {"label":"Brutality","notation":"HP,HK,HP,HK,HK,HP,HK,HP,HK,LK,LP","pretty":"HP,HK,HP,HK,HK,HP,HK,HP,HK,LK,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(18%)","notation":"HP,HP,LP","pretty":"HP,HP,LP"},
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(30%)","notation":"HP,HP,HK,HP,HK,_4_+HK","pretty":"HP,HP,HK,HP,HK,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "shang-tsung",
          "name": "Shang Tsung",
          "number": 21,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Flaming Skull-single","notation":"_4,_4,HP","pretty":"←,←,HP"},
                {"label":"Flaming Skull-double","notation":"_4,_4,_6,HP","pretty":"←,←,→,HP"},
                {"label":"Flaming Skull-triple","notation":"_4,_4,_6,_6,HP","pretty":"←,←,→,→,HP"},
                {"label":"Volcanic Eruption","notation":"_6,_4,_4,LK","pretty":"→,←,←,LK"}
              ]
            },
            {
              "name": "Morphs",
              "moves": [
                {"label":"Cyrax","notation":"BL,BL,BL","pretty":"BL,BL,BL"},
                {"label":"Jade","notation":"_6,_6,_2,_2,BL","pretty":"→,→,↓,↓,BL"},
                {"label":"Jax","notation":"_6,_6,_2,LP","pretty":"→,→,↓,LP"},
                {"label":"Kabal","notation":"LP,BL,HK","pretty":"LP,BL,HK"},
                {"label":"Kano","notation":"_4,_6,BL","pretty":"←,→,BL"},
                {"label":"Kitana","notation":"_6,_2,_6,RUN","pretty":"→,↓,→,RUN"},
                {"label":"Kung Lao","notation":"RUN,RUN,BL,RUN","pretty":"RUN,RUN,BL,RUN"},
                {"label":"Liu Kang","notation":"_4,_8,_6,_2","pretty":"←,↑,→,↓"},
                {"label":"Nightwolf","notation":"_8,_8,_8","pretty":"↑,↑,↑"},
                {"label":"Reptile","notation":"RUN,BL,BL,HK","pretty":"RUN,BL,BL,HK"},
                {"label":"Scorpion","notation":"_2,_2,_6,LP","pretty":"↓,↓,→,LP"},
                {"label":"Sektor","notation":"_2,_6,_4,RUN","pretty":"↓,→,←,RUN"},
                {"label":"Sheeva","notation":"_6,_2,_6,LK or hold LK _6,_2,_6","pretty":"→,↓,→,LK or hold LK →,↓,→"},
                {"label":"Sindel","notation":"_4,_2,_4,LK","pretty":"←,↓,←,LK"},
                {"label":"Sonya","notation":"_2_+LP_+BL_+RUN","pretty":"↓+LP+BL+RUN"},
                {"label":"Stryker","notation":"_6,_6,_6,HK","pretty":"→,→,→,HK"},
                {"label":"Unmasked Sub-Zero","notation":"_6,_2,_6,HP","pretty":"→,↓,→,HP"},
                {"label":"Mileena","notation":"RUN,BL,HK","pretty":"RUN,BL,HK"},
                {"label":"Ermac","notation":"_2,_2,_8","pretty":"↓,↓,↑"},
                {"label":"Classic Sub-Zero","notation":"BL,BL,RUN,RUN","pretty":"BL,BL,RUN,RUN"},
                {"label":"Human Smoke","notation":"BL,RUN,LK","pretty":"BL,RUN,LK"},
                {"label":"Robot Smoke","notation":"_4,_4,_2,LK","pretty":"←,←,↓,LK"},
                {"label":"Noob Saibot","notation":"_6,_2,_2,_4,HK","pretty":"→,↓,↓,←,HK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1          (close)","notation":"hold LP _2,_6,_6,_2","pretty":"hold LP ↓,→,→,↓"},
                {"label":"Fatality 2        (close)","notation":"hold LP RUN,BL,RUN,BL","pretty":"hold LP RUN,BL,RUN,BL"},
                {"label":"Animality           (sweep)","notation":"hold HP RUN,RUN,RUN","pretty":"hold HP RUN,RUN,RUN"},
                {"label":"Friendship","notation":"LK,RUN,RUN,_2","pretty":"LK,RUN,RUN,↓"},
                {"label":"Babality","notation":"RUN,RUN,RUN,LK","pretty":"RUN,RUN,RUN,LK"},
                {"label":"Stage Fatality","notation":"_8,_8,_4,LP","pretty":"↑,↑,←,LP"},
                {"label":"Brutality:","notation":"HP,BL,BL,BL,LK,HP,LP,LP,BL,BL,BL","pretty":"HP,BL,BL,BL,LK,HP,LP,LP,BL,BL,BL"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(19%)","notation":"[LK/HK],HK,_4_+HK","pretty":"[LK/HK],HK,←+HK"},
                {"label":"(22%)","notation":"HP,HP,LP,_4_+HK","pretty":"HP,HP,LP,←+HK"},
                {"label":"(27%)","notation":"[LK/HK],HP,HP,LP,_4_+HK","pretty":"[LK/HK],HP,HP,LP,←+HK"}
              ]
            }
          ]
        },
        {
          "id": "mileena",
          "name": "Mileena",
          "number": 22,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Ground Roll","notation":"_4,_4,_2,HK","pretty":"←,←,↓,HK"},
                {"label":"Sai Throw (also in air)","notation":"hold HP for 2 sec","pretty":"hold HP for 2 sec"},
                {"label":"Teleport Kick","notation":"_6,_6,LK","pretty":"→,→,LK"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1                 (far)","notation":"_4,_4,_4,_6,LK","pretty":"←,←,←,→,LK"},
                {"label":"Fatality 2               (close)","notation":"_2,_6,_2,_6,LP","pretty":"↓,→,↓,→,LP"},
                {"label":"Animality                (close)","notation":"_6,_2,_2,_6,HK","pretty":"→,↓,↓,→,HK"},
                {"label":"Friendship","notation":"_2,_2,_4,_6,HP","pretty":"↓,↓,←,→,HP"},
                {"label":"Babality","notation":"_2,_2,_6,_6,HP","pretty":"↓,↓,→,→,HP"},
                {"label":"Stage Fatality","notation":"_2,_2,_2,LP","pretty":"↓,↓,↓,LP"},
                {"label":"Brutality","notation":"HP,LP,LP,HP,BL,HK,LK,HK,BL,HP,LP","pretty":"HP,LP,LP,HP,BL,HK,LK,HK,BL,HP,LP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(16%)","notation":"[LK/HK],HK,_2,_6,LK(juggle)","pretty":"[LK/HK],HK,↓,→,LK(juggle)"},
                {"label":"(22%)","notation":"HP,HP,_8_+LP,_2_+LP","pretty":"HP,HP,↑+LP,↓+LP"},
                {"label":"(23%)","notation":"[LK/HK],HK,_8_+LK,_8_+HK","pretty":"[LK/HK],HK,↑+LK,↑+HK"},
                {"label":"(24%)","notation":"HP,HP,HK,HK,_2,_6,LK(juggle)","pretty":"HP,HP,HK,HK,↓,→,LK(juggle)"},
                {"label":"(30%)","notation":"HP,HP,HK,HK,_8_+LK,_8_+HK","pretty":"HP,HP,HK,HK,↑+LK,↑+HK"}
              ]
            }
          ]
        },
        {
          "id": "sheeva",
          "name": "Sheeva",
          "number": 23,
          "categories": [
            {
              "name": "Special Moves",
              "moves": [
                {"label":"Teleport Stomp","notation":"_2,_8","pretty":"↓,↑"},
                {"label":"Ground Stomp","notation":"_4,_2,_4,HK","pretty":"←,↓,←,HK"},
                {"label":"Fireball","notation":"_2,_6,HP","pretty":"↓,→,HP"}
              ]
            },
            {
              "name": "Finishing Moves",
              "moves": [
                {"label":"Fatality 1               (close)","notation":"_6,_2,_2,_6,LP","pretty":"→,↓,↓,→,LP"},
                {"label":"Fatality 2             (close)","notation":"hold HK _4,_6,_6","pretty":"hold HK ←,→,→"},
                {"label":"Animality               (close)","notation":"RUN,BL,BL,BL,BL","pretty":"RUN,BL,BL,BL,BL"},
                {"label":"Friendship              (sweep)","notation":"_6,_6,_2,_6, HP","pretty":"→,→,↓,→, HP"},
                {"label":"Babality","notation":"_2,_2,_2,_4,HK","pretty":"↓,↓,↓,←,HK"},
                {"label":"Stage Fatality","notation":"_2,_6,_2,_6,LP","pretty":"↓,→,↓,→,LP"},
                {"label":"Brutality","notation":"HP,LP,BL,LK,HK,BL,HK,LK,BL,LP,HP","pretty":"HP,LP,BL,LK,HK,BL,HK,LK,BL,LP,HP"}
              ]
            },
            {
              "name": "Autokombos",
              "moves": [
                {"label":"(25%)","notation":"HP,HP,LP,_6_+HP(juggle)","pretty":"HP,HP,LP,→+HP(juggle)"},
                {"label":"(27%)","notation":"[LK/HK],HK,LK,_4_+HK","pretty":"[LK/HK],HK,LK,←+HK"},
                {"label":"(42%)","notation":"HP,HP,LP,HK,HK,LK,_4_+HK","pretty":"HP,HP,LP,HK,HK,LK,←+HK"}
              ]
            }
          ]
        }
      ]
    }
  }
};
