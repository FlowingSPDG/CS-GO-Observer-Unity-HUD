using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSGSI;
using CSGSI.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class GSI_Receiver : MonoBehaviour {
    GameStateListener gsl;

    // general informations,such as rounds,phases,maps,etc
    public string gsi_phase;
    public string gsi_map;
    // public string gsi_team1;
    // public string gsi_team2;
    public int gsi_team1_score;
    public int gsi_team2_score;

    // all players information
    public object gsi_all_players;
    public object[] gsi_all_players_array;
    public string gsi_all_players_string;
    public string gsi_auth;

    // main informations who you observing at
    public int spec_hp;
    public int spec_ap;
    public int spec_ammo_loaded;
    public int spec_ammo_max;
    public int spec_ammo_reserve;
    public int spec_stats_kills;
    public int spec_stats_deaths;
    public int spec_stats_assists;
    public string spec_name;
    public string spec_team;
    public string spec_weapon_active;
    public string[] spec_greandes;
    public string spec_steamid;


    // to link other Unity components
    public Text comp_spec_name;
    public Text comp_spec_hp;
    public Text comp_spec_ap;
    public Text comp_spec_ammo_loaded;
    public Text comp_spec_ammo_max;
    public Text comp_spec_ammo_reserve;
    public Text comp_spec_stats_kills;
    public Text comp_spec_stats_assists;
    public Text comp_spec_stats_deaths;

    public Text comp_score_team1;
    public Text comp_score_team2;
    /*
    public Text comp_; // etc
    */

    void Start () {
        gsl = new GameStateListener("http://192.168.1.14:3001");
        gsl.NewGameState += new NewGameStateHandler(OnNewGameState);
        if (!gsl.Start())
        {
            Debug.Log("GSI Server failed");
        }
        Debug.Log("GSI Server launched!");
    }
	
	// Update is called once per frame
	void Update () {
        comp_spec_name.text = spec_name.ToString();
        comp_spec_hp.text = spec_hp.ToString();
        comp_spec_ap.text = spec_ap.ToString();

        comp_score_team1.text = Convert.ToString(gsi_team1_score);
        comp_score_team2.text = Convert.ToString(gsi_team2_score);


        comp_spec_ammo_loaded.text = spec_ammo_loaded.ToString();
        comp_spec_ammo_reserve.text = spec_ammo_reserve.ToString();
        comp_spec_ammo_max.text = spec_ammo_max.ToString();
        comp_spec_stats_kills.text = Convert.ToString(spec_stats_kills);
        comp_spec_stats_deaths.text = Convert.ToString(spec_stats_deaths);
        comp_spec_stats_assists.text = Convert.ToString(spec_stats_assists);

    }

    void OnNewGameState(GameState gs)
    {
        gsi_phase = gs.Round.Phase.ToString();
        gsi_map = gs.Map.Name;

        // gsi_team1 = gs.Map.TeamCT.JSON; // チーム名
        gsi_team1_score = gs.Map.TeamCT.Score;
        // gsi_team2 = gs.Map.TeamT.JSON; // チーム名
        gsi_team2_score = gs.Map.TeamT.Score;

        //gsi_all_players = JsonConvert.DeserializeObject(gs.AllPlayers.JSON);
        gsi_all_players_string = gs.AllPlayers.JSON;
        
        var gsi_all_players = JsonConvert.DeserializeObject<Dictionary<string, object>>(gsi_all_players_string);
        //var gsi_all_players_state = JsonConvert.DeserializeObject<Dictionary<string, string>>(gsi_all_players_string + "[state]");
        //var gsi_all_players_match_stats = JsonConvert.DeserializeObject<Dictionary<string, string>>(gsi_all_players_string + "match_stats");

        foreach (var key in gsi_all_players.Keys)
        {
            Debug.Log("SteamID : " + key);
            Debug.Log("dic : " + gsi_all_players[key]);
            //var gsi_all_players_state = JsonConvert.DeserializeObject(gsi_all_players[key]);
        }


        gsi_auth = gs.Auth.Token;

        spec_hp = gs.Player.State.Health;
        spec_ap = gs.Player.State.Armor;
        spec_ammo_loaded = gs.Player.Weapons.ActiveWeapon.AmmoClip;
        spec_ammo_max = gs.Player.Weapons.ActiveWeapon.AmmoClipMax;
        //spec_ammo_reserve = gs.Player.Weapons.ActiveWeapon.AmmoReserve;
        spec_stats_kills = gs.Player.MatchStats.Kills;
        spec_stats_deaths = gs.Player.MatchStats.Deaths;
        spec_stats_assists = gs.Player.MatchStats.Assists;

        spec_name = gs.Player.Name;
        spec_weapon_active = gs.Player.Weapons.ActiveWeapon.Name;
        spec_team = gs.Player.Team;
        spec_steamid = gs.Player.SteamID;

        bool winner_scene_loaded = false;
        if (gs.Round.Phase.ToString() == "Over")
        {
            //Debug.Log(gs.Round.WinTeam.ToString());
            if (!winner_scene_loaded)
            {
                switch (gs.Round.WinTeam.ToString())
                {
                    case "CT":
                        Debug.Log("CT wins");
                        SceneManager.LoadScene("CTWin", LoadSceneMode.Additive);
                        winner_scene_loaded = true;
                        break;
                    case "T":
                        Debug.Log("T wins");
                        SceneManager.LoadScene("TWin", LoadSceneMode.Additive);
                        winner_scene_loaded = true;
                        break;
                    case "default":
                        Debug.Log("Unknown");
                        winner_scene_loaded = false;
                        break;
                }
            }
        }
        else if (gs.Round.Phase.ToString() == "FreezeTime")
        {
            Debug.Log("Freeze time now");
            winner_scene_loaded = false;
        }
    }
}
